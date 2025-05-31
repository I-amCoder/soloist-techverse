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

    public async Task<IActionResult> Index()
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
        
        // Get popular categories
        var categoryStats = await _context.Items
            .Where(i => i.Status == ItemStatus.Active)
            .GroupBy(i => i.Category)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
        viewModel.PopularCategories = categoryStats;
        
        // Get recent items for display (mix of lost and found)
        viewModel.RecentItems = await _context.Items
            .Include(i => i.User)
            .Where(i => i.Status == ItemStatus.Active)
            .OrderByDescending(i => i.DateReported)
            .Take(8)
            .ToListAsync();
            
        // Get featured items (items with high engagement)
        viewModel.FeaturedItems = await _context.Items
            .Include(i => i.User)
            .Include(i => i.ClaimRequests)
            .Where(i => i.Status == ItemStatus.Active)
            .OrderByDescending(i => i.ClaimRequests.Count)
            .Take(4)
            .ToListAsync();
        
        // Get recent success stories
        var recentSuccesses = await _context.Items
            .Include(i => i.User)
            .Where(i => i.Status == ItemStatus.Resolved)
            .OrderByDescending(i => i.DateReported)
            .Take(3)
            .Select(i => new SuccessStoryViewModel
            {
                ItemName = i.Name,
                OwnerName = i.User.FirstName,
                ResolvedDate = i.DateReported,
                Category = i.Category.ToString()
            })
            .ToListAsync();
        viewModel.RecentSuccessStories = recentSuccesses;
        
        // User-specific information
        if (User.Identity?.IsAuthenticated == true)
        {
            viewModel.IsUserLoggedIn = true;
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                viewModel.UserName = $"{user.FirstName} {user.LastName}";
                viewModel.UserItemsCount = await _context.Items.CountAsync(i => i.UserId == user.Id);
                viewModel.UserClaimsCount = await _context.ClaimRequests.CountAsync(c => c.ClaimantId == user.Id);
            }
        }

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
