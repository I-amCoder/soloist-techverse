using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Models.Identity;
using AspnetCoreMvcFull.Models.ViewModels;
using Microsoft.AspNetCore.SignalR;
using AspnetCoreMvcFull.Hubs;

namespace AspnetCoreMvcFull.Controllers
{
    [Authorize] 
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IHubContext<ItemHub> _hubContext;

        public ItemsController(ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            IHubContext<ItemHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _hubContext = hubContext;
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
                
                await _hubContext.Clients.All.SendAsync("ReceiveNewItemNotification", "Lost");
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
                
                await _hubContext.Clients.All.SendAsync("ReceiveNewItemNotification", "Found");
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
        public async Task<IActionResult> SubmitClaim(int itemId, string handoverNotes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Check if user already has a pending claim for this item
            var existingClaim = await _context.ClaimRequests
                .FirstOrDefaultAsync(c => c.ItemId == itemId && c.ClaimantId == user.Id);

            if (existingClaim != null)
            {
                TempData["Error"] = "You have already submitted a claim for this item.";
                return RedirectToAction("Details", new { id = itemId });
            }

            var claimRequest = new ClaimRequest
            {
                ItemId = itemId,
                ClaimantId = user.Id,
                HandoverNotes = handoverNotes
            };

            _context.ClaimRequests.Add(claimRequest);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your claim has been submitted successfully!";
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

        // GET: My Claims (items with claims submitted to them)
        public async Task<IActionResult> MyClaims()
        {
            var user = await _userManager.GetUserAsync(User);
            
            var myItemsWithClaims = await _context.Items
                .Include(i => i.ClaimRequests)
                .ThenInclude(c => c.Claimant)
                .Where(i => i.UserId == user!.Id && i.ClaimRequests.Any())
                .OrderByDescending(i => i.DateReported)
                .ToListAsync();
            
            return View(myItemsWithClaims);
        }

        // POST: Escalate to Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EscalateToAdmin(int itemId, string reason)
        {
            var user = await _userManager.GetUserAsync(User);
            var item = await _context.Items
                .Include(i => i.ClaimRequests)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == user!.Id);
            
            if (item != null && !item.IsEscalatedToAdmin)
            {
                item.IsEscalatedToAdmin = true;
                item.EscalationDate = DateTime.UtcNow;
                item.EscalationReason = reason;
                
                await _context.SaveChangesAsync();
                TempData["Success"] = "Item escalated to admin for review. You'll be notified once a decision is made.";
            }
            
            return RedirectToAction(nameof(MyClaims));
        }

        // POST: Update Claim Status (enhanced)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateClaimStatus(int claimId, ClaimStatus status)
        {
            var user = await _userManager.GetUserAsync(User);
            var claim = await _context.ClaimRequests
                .Include(c => c.Item)
                .FirstOrDefaultAsync(c => c.Id == claimId && c.Item.UserId == user!.Id);
            
            if (claim != null && !claim.Item.IsEscalatedToAdmin)
            {
                claim.Status = status;
                if (status == ClaimStatus.Approved)
                {
                    claim.ApprovedDate = DateTime.UtcNow;
                    // Auto-reject other pending claims for this item
                    var otherClaims = await _context.ClaimRequests
                        .Where(c => c.ItemId == claim.ItemId && c.Id != claimId && c.Status == ClaimStatus.Pending)
                        .ToListAsync();
                    
                    foreach (var otherClaim in otherClaims)
                    {
                        otherClaim.Status = ClaimStatus.Rejected;
                    }
                }
                
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Claim {status.ToString().ToLower()} successfully.";
            }
            
            return RedirectToAction(nameof(MyClaims));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int itemId, string receiverId, string content)
        {
            var sender = await _userManager.GetUserAsync(User);
            if (sender == null)
            {
                return Unauthorized();
            }

            var message = new Message
            {
                SenderId = sender.Id,
                ReceiverId = receiverId,
                Content = content
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Notify the receiver via SignalR or other means
            await _hubContext.Clients.User(receiverId).SendAsync("ReceiveMessage", message);

            return RedirectToAction("Details", new { id = itemId });
        }

        public async Task<IActionResult> GetMessages(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUser.Id && m.ReceiverId == userId) ||
                            (m.SenderId == userId && m.ReceiverId == currentUser.Id))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return Json(messages);
        }

        public async Task<IActionResult> Chat(int itemId, string receiverId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            ViewBag.ReceiverId = receiverId;
            ViewBag.ItemId = itemId;

            var messages = await _context.Messages
                .Where(m => (m.SenderId == user.Id && m.ReceiverId == receiverId) ||
                            (m.SenderId == receiverId && m.ReceiverId == user.Id))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return View("Chat", messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAdminRequest(int itemId, AdminRequestType requestType, string handoverNotes, string? claimantId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var item = await _context.Items.FindAsync(itemId);
            if (item == null)
            {
                return NotFound();
            }

            // Validate and find claimant if provided
            string? validClaimantId = null;
            if (!string.IsNullOrWhiteSpace(claimantId))
            {
                // Try to find user by email first
                var claimantUser = await _userManager.FindByEmailAsync(claimantId.Trim());
                
                // If not found by email, try by username
                if (claimantUser == null)
                {
                    claimantUser = await _userManager.FindByNameAsync(claimantId.Trim());
                }
                
                // If still not found, try by ID directly
                if (claimantUser == null)
                {
                    claimantUser = await _userManager.FindByIdAsync(claimantId.Trim());
                }
                
                if (claimantUser != null)
                {
                    validClaimantId = claimantUser.Id;
                }
                else
                {
                    TempData["Error"] = "Claimant user not found. Please check the email/username.";
                    return RedirectToAction("Details", new { id = itemId });
                }
            }

            var adminRequest = new AdminRequest
            {
                ItemId = itemId,
                RequesterId = user.Id,
                ClaimantId = validClaimantId,
                Type = requestType,
                HandoverNotes = handoverNotes
            };

            _context.AdminRequests.Add(adminRequest);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Admin request submitted successfully!";
            return RedirectToAction("Details", new { id = itemId });
        }
    }
} 
