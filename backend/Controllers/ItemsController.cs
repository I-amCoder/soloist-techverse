using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Models.Identity;
using AspnetCoreMvcFull.Models.ViewModels;

namespace AspnetCoreMvcFull.Controllers
{
    [Authorize]
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ItemsController(ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: Lost Items
        public async Task<IActionResult> Lost(string? category, string? location, string? search)
        {
            var items = _context.Items
                .Include(i => i.User)
                .Where(i => i.Type == ItemType.Lost && i.Status == ItemStatus.Active);

            // Apply filters
            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                if (Enum.TryParse<ItemCategory>(category, out var categoryEnum))
                {
                    items = items.Where(i => i.Category == categoryEnum);
                }
            }

            if (!string.IsNullOrEmpty(location))
            {
                items = items.Where(i => i.Location.Contains(location));
            }

            if (!string.IsNullOrEmpty(search))
            {
                items = items.Where(i => i.Name.Contains(search) || i.Description.Contains(search));
            }

            ViewBag.Categories = Enum.GetValues<ItemCategory>();
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedLocation = location;
            ViewBag.SearchTerm = search;

            return View(await items.OrderByDescending(i => i.DateReported).ToListAsync());
        }

        // GET: Found Items
        public async Task<IActionResult> Found(string? category, string? location, string? search)
        {
            var items = _context.Items
                .Include(i => i.User)
                .Where(i => i.Type == ItemType.Found && i.Status == ItemStatus.Active);

            // Apply same filters as Lost
            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                if (Enum.TryParse<ItemCategory>(category, out var categoryEnum))
                {
                    items = items.Where(i => i.Category == categoryEnum);
                }
            }

            if (!string.IsNullOrEmpty(location))
            {
                items = items.Where(i => i.Location.Contains(location));
            }

            if (!string.IsNullOrEmpty(search))
            {
                items = items.Where(i => i.Name.Contains(search) || i.Description.Contains(search));
            }

            ViewBag.Categories = Enum.GetValues<ItemCategory>();
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedLocation = location;
            ViewBag.SearchTerm = search;

            return View(await items.OrderByDescending(i => i.DateReported).ToListAsync());
        }

        // GET: Post Lost Item
        [HttpGet("{controller}/report-lost")]
        public IActionResult PostLost()
        {
            ViewBag.Categories = Enum.GetValues<ItemCategory>();
            return View();
        }

        // POST: Post Lost Item
        [HttpPost("{controller}/report-lost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostLost(PostItemViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var item = new Item
                {
                    Name = model.Name,
                    Description = model.Description,
                    Category = model.Category,
                    Location = model.Location,
                    DateLostOrFound = model.DateLostOrFound,
                    ContactInfo = model.ContactInfo,
                    Type = ItemType.Lost,
                    UserId = user!.Id,
                    ImagePath = await SaveImageAsync(model.Image)
                };

                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Lost item posted successfully!";
                return RedirectToAction(nameof(Lost));
            }

            ViewBag.Categories = Enum.GetValues<ItemCategory>();
            return View(model);
        }

        // GET: Post Found Item
        [HttpGet("{controller}/report-found")]
        public IActionResult PostFound()
        {
            ViewBag.Categories = Enum.GetValues<ItemCategory>();
            return View();
        }

        // POST: Post Found Item
        [HttpPost("{controller}/report-found")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostFound(PostItemViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var item = new Item
                {
                    Name = model.Name,
                    Description = model.Description,
                    Category = model.Category,
                    Location = model.Location,
                    DateLostOrFound = model.DateLostOrFound,
                    ContactInfo = model.ContactInfo,
                    Type = ItemType.Found,
                    UserId = user!.Id,
                    ImagePath = await SaveImageAsync(model.Image)
                };

                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Found item posted successfully!";
                return RedirectToAction(nameof(Found));
            }

            ViewBag.Categories = Enum.GetValues<ItemCategory>();
            return View(model);
        }

        private async Task<string?> SaveImageAsync(IFormFile? image)
        {
            if (image == null || image.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "items");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/uploads/items/" + uniqueFileName;
        }

        // GET: My Items
        public async Task<IActionResult> MyItems()
        {
            var user = await _userManager.GetUserAsync(User);
            
            var myItems = await _context.Items
                .Where(i => i.UserId == user!.Id)
                .OrderByDescending(i => i.DateReported)
                .ToListAsync();

            ViewBag.LostItems = myItems.Where(i => i.Type == ItemType.Lost).ToList();
            ViewBag.FoundItems = myItems.Where(i => i.Type == ItemType.Found).ToList();
            
            return View(myItems);
        }

        // POST: Mark item as found/returned
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsReturned(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id && i.UserId == user!.Id);
            
            if (item != null)
            {
                item.Status = ItemStatus.Resolved;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Item marked as resolved successfully!";
            }
            
            return RedirectToAction(nameof(MyItems));
        }

        // POST: Delete item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id && i.UserId == user!.Id);
            
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Item deleted successfully!";
            }
            
            return RedirectToAction(nameof(MyItems));
        }

        // Admin/Manager only - bulk operations
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Moderate()
        {
            var flaggedItems = await _context.Items
                .Include(i => i.User)
                .Where(i => i.Status == ItemStatus.Active)
                .ToListAsync();

            return View(flaggedItems);
        }

        // POST: Submit claim request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(int itemId, string message)
        {
            var user = await _userManager.GetUserAsync(User);
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId && i.Status == ItemStatus.Active);
            
            if (item == null)
            {
                TempData["Error"] = "Item not found or no longer available.";
                return RedirectToAction(nameof(Lost));
            }
            
            // Check if user already submitted a claim for this item
            var existingClaim = await _context.ClaimRequests
                .FirstOrDefaultAsync(c => c.ItemId == itemId && c.ClaimantId == user!.Id && 
                                    (c.Status == ClaimStatus.Pending || c.Status == ClaimStatus.Approved || c.Status == ClaimStatus.HandedOver));
            
            if (existingClaim != null)
            {
                TempData["Error"] = "You have already submitted a claim for this item.";
                return RedirectToAction("Details", new { id = itemId });
            }
            
            // Don't allow claiming your own items
            if (item.UserId == user!.Id)
            {
                TempData["Error"] = "You cannot claim your own item.";
                return RedirectToAction("Details", new { id = itemId });
            }
            
            var claimRequest = new ClaimRequest
            {
                ItemId = itemId,
                ClaimantId = user.Id,
                Message = message,
                Status = ClaimStatus.Pending,
                RequestDate = DateTime.UtcNow
            };
            
            _context.ClaimRequests.Add(claimRequest);
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Claim request submitted successfully! The item owner will be notified.";
            return RedirectToAction("Details", new { id = itemId });
        }

        // GET: Item Details
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.Items
                .Include(i => i.User)
                .Include(i => i.ClaimRequests)
                    .ThenInclude(c => c.Claimant)
                .FirstOrDefaultAsync(i => i.Id == id);
            
            if (item == null)
            {
                return NotFound();
            }
            
            var user = await _userManager.GetUserAsync(User);
            
            // Check if current user has an active claim for this item
            ViewBag.HasClaimed = await _context.ClaimRequests
                .AnyAsync(c => c.ItemId == id && c.ClaimantId == user!.Id && 
                         (c.Status == ClaimStatus.Pending || c.Status == ClaimStatus.Approved || c.Status == ClaimStatus.HandedOver));
            
            // Get user's claim if exists
            ViewBag.UserClaim = await _context.ClaimRequests
                .FirstOrDefaultAsync(c => c.ItemId == id && c.ClaimantId == user!.Id);
            
            // Check if current user owns this item
            ViewBag.IsOwner = item.UserId == user!.Id;
            
            return View(item);
        }

        // GET: My Requests
        public async Task<IActionResult> MyRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            
            var myClaimRequests = await _context.ClaimRequests
                .Include(c => c.Item)
                    .ThenInclude(i => i.User)
                .Where(c => c.ClaimantId == user!.Id)
                .OrderByDescending(c => c.RequestDate)
                .ToListAsync();
            
            return View(myClaimRequests);
        }

        // POST: Approve claim (for item owners)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClaim(int claimId)
        {
            var user = await _userManager.GetUserAsync(User);
            var claim = await _context.ClaimRequests
                .Include(c => c.Item)
                .FirstOrDefaultAsync(c => c.Id == claimId && c.Item.UserId == user!.Id);
            
            if (claim == null)
            {
                TempData["Error"] = "Claim request not found.";
                return RedirectToAction(nameof(MyItems));
            }
            
            // Approve this claim and reject all others for this item
            claim.Status = ClaimStatus.Approved;
            claim.ApprovedDate = DateTime.UtcNow;
            
            // Mark item as claimed
            claim.Item.Status = ItemStatus.Claimed;
            
            // Reject all other pending claims for this item
            var otherClaims = await _context.ClaimRequests
                .Where(c => c.ItemId == claim.ItemId && c.Id != claimId && c.Status == ClaimStatus.Pending)
                .ToListAsync();
            
            foreach (var otherClaim in otherClaims)
            {
                otherClaim.Status = ClaimStatus.Rejected;
            }
            
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Claim approved! Please coordinate with the claimant to hand over the item.";
            return RedirectToAction("Details", new { id = claim.ItemId });
        }

        // POST: Mark item as handed over (for item owners)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsHandedOver(int claimId, string? handoverNotes)
        {
            var user = await _userManager.GetUserAsync(User);
            var claim = await _context.ClaimRequests
                .Include(c => c.Item)
                .FirstOrDefaultAsync(c => c.Id == claimId && c.Item.UserId == user!.Id && c.Status == ClaimStatus.Approved);
            
            if (claim == null)
            {
                TempData["Error"] = "Claim request not found or not approved.";
                return RedirectToAction(nameof(MyItems));
            }
            
            claim.Status = ClaimStatus.HandedOver;
            claim.HandedOverDate = DateTime.UtcNow;
            claim.HandoverNotes = handoverNotes;
            claim.Item.Status = ItemStatus.HandedOver;
            
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Item marked as handed over! Waiting for claimant confirmation.";
            return RedirectToAction("Details", new { id = claim.ItemId });
        }

        // POST: Confirm receipt (for claimants)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReceipt(int claimId)
        {
            var user = await _userManager.GetUserAsync(User);
            var claim = await _context.ClaimRequests
                .Include(c => c.Item)
                .FirstOrDefaultAsync(c => c.Id == claimId && c.ClaimantId == user!.Id && c.Status == ClaimStatus.HandedOver);
            
            if (claim == null)
            {
                TempData["Error"] = "Claim request not found or item not yet handed over.";
                return RedirectToAction(nameof(MyRequests));
            }
            
            claim.Status = ClaimStatus.Completed;
            claim.CompletedDate = DateTime.UtcNow;
            claim.Item.Status = ItemStatus.Resolved;
            
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Receipt confirmed! This claim has been successfully completed.";
            return RedirectToAction("Details", new { id = claim.ItemId });
        }

        // POST: Reject claim (for item owners)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClaim(int claimId)
        {
            var user = await _userManager.GetUserAsync(User);
            var claim = await _context.ClaimRequests
                .Include(c => c.Item)
                .FirstOrDefaultAsync(c => c.Id == claimId && c.Item.UserId == user!.Id);
            
            if (claim == null)
            {
                TempData["Error"] = "Claim request not found.";
                return RedirectToAction(nameof(MyItems));
            }
            
            claim.Status = ClaimStatus.Rejected;
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Claim request rejected.";
            return RedirectToAction("Details", new { id = claim.ItemId });
        }
    }
} 