using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using AspnetCoreMvcFull.Models.Identity;
using Microsoft.AspNetCore.Authorization;

namespace AspnetCoreMvcFull.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? category, string? type, string? search)
    {
        var viewModel = new HomePageViewModel();
        
        // Get basic statistics
        viewModel.TotalItemsReported = await _context.Items.CountAsync();
        viewModel.TotalItemsReturned = await _context.Items.CountAsync(i => i.Status == ItemStatus.Resolved);
        viewModel.ActiveUsers = await _context.Users.CountAsync(u => u.IsActive);
        viewModel.TotalSuccessStories = viewModel.TotalItemsReturned;
        
        // Get category statistics
        viewModel.LostItemsCount = await _context.Items.CountAsync(i => i.Type == ItemType.Lost && i.Status == ItemStatus.Active);
        viewModel.FoundItemsCount = await _context.Items.CountAsync(i => i.Type == ItemType.Found && i.Status == ItemStatus.Active);
        viewModel.RecentlyResolvedCount = await _context.Items.CountAsync(i => i.Status == ItemStatus.Resolved && i.DateReported >= DateTime.UtcNow.AddDays(-7));
        
        // Get popular categories with counts
        viewModel.PopularCategories = await _context.Items
            .Where(i => i.Status == ItemStatus.Active)
            .GroupBy(i => i.Category)
            .Select(g => new CategoryStatsViewModel
            {
                Category = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(c => c.Count)
            .Take(6)
            .ToListAsync();
        
        // Build query for recent items with filters
        var itemsQuery = _context.Items
            .Include(i => i.User)
            .Where(i => i.Status == ItemStatus.Active);
            
        // Apply filters
        if (!string.IsNullOrEmpty(type) && Enum.TryParse<ItemType>(type, true, out var itemType))
        {
            itemsQuery = itemsQuery.Where(i => i.Type == itemType);
        }
        
        if (!string.IsNullOrEmpty(category) && Enum.TryParse<ItemCategory>(category, true, out var itemCategory))
        {
            itemsQuery = itemsQuery.Where(i => i.Category == itemCategory);
        }
        
        if (!string.IsNullOrEmpty(search))
        {
            itemsQuery = itemsQuery.Where(i => 
                i.Name.Contains(search) || 
                i.Description.Contains(search) || 
                i.Location.Contains(search));
        }
        
        // Get recent items (filtered)
        viewModel.RecentItems = await itemsQuery
            .OrderByDescending(i => i.DateReported)
            .Take(6)
            .Select(i => new RecentItemViewModel
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description.Length > 80 ? i.Description.Substring(0, 80) + "..." : i.Description,
                Category = i.Category,
                Type = i.Type,
                Location = i.Location,
                DateReported = i.DateReported,
                ImagePath = i.ImagePath
            })
            .ToListAsync();
            
        // Get success stories
        viewModel.RecentSuccessStories = await _context.Items
            .Include(i => i.User)
            .Where(i => i.Status == ItemStatus.Resolved && i.DateResolved.HasValue)
            .OrderByDescending(i => i.DateResolved)
            .Take(3)
            .Select(i => new SuccessStoryViewModel
            {
                ItemName = i.Name,
                OwnerName = i.User.FirstName,
                ResolvedDate = i.DateResolved.Value
            })
            .ToListAsync();
            
        // Set current filter values for the view
        ViewBag.CurrentCategory = category;
        ViewBag.CurrentType = type;
        ViewBag.CurrentSearch = search;
        
        return View(viewModel);
    }

    // Public items browsing page
    public async Task<IActionResult> Items(string? category, string? type, string? search, int page = 1)
    {
        const int pageSize = 12;
        
        // Build query with filters
        var itemsQuery = _context.Items
            .Include(i => i.User)
            .Where(i => i.Status == ItemStatus.Active);
            
        // Apply filters
        if (!string.IsNullOrEmpty(type) && Enum.TryParse<ItemType>(type, true, out var itemType))
        {
            itemsQuery = itemsQuery.Where(i => i.Type == itemType);
        }
        
        if (!string.IsNullOrEmpty(category) && Enum.TryParse<ItemCategory>(category, true, out var itemCategory))
        {
            itemsQuery = itemsQuery.Where(i => i.Category == itemCategory);
        }
        
        if (!string.IsNullOrEmpty(search))
        {
            itemsQuery = itemsQuery.Where(i => 
                i.Name.Contains(search) || 
                i.Description.Contains(search) || 
                i.Location.Contains(search));
        }
        
        // Get total count for pagination
        var totalItems = await itemsQuery.CountAsync();
        
        // Get items for current page
        var items = await itemsQuery
            .OrderByDescending(i => i.DateReported)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new PublicItemViewModel
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Category = i.Category,
                Type = i.Type,
                Location = i.Location,
                DateReported = i.DateReported,
                ImagePath = i.ImagePath,
                ReporterName = $"{i.User.FirstName} {i.User.LastName[0]}."
            })
            .ToListAsync();
            
        var viewModel = new PublicItemsViewModel
        {
            Items = items,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
            TotalItems = totalItems,
            PageSize = pageSize,
            CurrentType = type,
            CurrentCategory = category,
            CurrentSearch = search
        };
        
        return View(viewModel);
    }

    // Public preview page for items (no authentication required)
    [AllowAnonymous]
    public async Task<IActionResult> Preview(int id)
    {
        var item = await _context.Items
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Id == id && i.Status == ItemStatus.Active);

        if (item == null)
        {
            return NotFound();
        }

        // Create view model for public preview
        var viewModel = new ItemPreviewViewModel
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Category = item.Category,
            Location = item.Location,
            Type = item.Type,
            DateReported = item.DateReported,
            ImagePath = item.ImagePath,
            ReporterName = $"{item.User.FirstName} {item.User.LastName[0]}.", // Partial name for privacy
            ContactMethod = "Contact through platform", // Don't expose direct contact
            IsOwnItem = User.Identity?.IsAuthenticated == true && item.UserId == _userManager.GetUserId(User)
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
