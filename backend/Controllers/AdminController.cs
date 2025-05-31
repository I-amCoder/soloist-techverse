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
    [Authorize(Roles = "Admin,Manager")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AdminController(ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin Dashboard
        public async Task<IActionResult> Index()
        {
            var dashboardData = new AdminDashboardViewModel
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalItems = await _context.Items.CountAsync(),
                TotalClaims = await _context.ClaimRequests.CountAsync(),
                ActiveItems = await _context.Items.CountAsync(i => i.Status == ItemStatus.Active),
                PendingClaims = await _context.ClaimRequests.CountAsync(c => c.Status == ClaimStatus.Pending),
                ResolvedItems = await _context.Items.CountAsync(i => i.Status == ItemStatus.Resolved),
                
                // Recent activity
                RecentItems = await _context.Items
                    .Include(i => i.User)
                    .OrderByDescending(i => i.DateReported)
                    .Take(5)
                    .ToListAsync(),
                    
                RecentClaims = await _context.ClaimRequests
                    .Include(c => c.Item)
                    .Include(c => c.Claimant)
                    .OrderByDescending(c => c.RequestDate)
                    .Take(5)
                    .ToListAsync(),

                // Statistics by category
                ItemsByCategory = await _context.Items
                    .GroupBy(i => i.Category)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Category, x => x.Count),

                // Statistics by type
                LostItemsCount = await _context.Items.CountAsync(i => i.Type == ItemType.Lost),
                FoundItemsCount = await _context.Items.CountAsync(i => i.Type == ItemType.Found)
            };

            return View(dashboardData);
        }

        // GET: Manage Users
        public async Task<IActionResult> Users(string? search, string? role)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                usersQuery = usersQuery.Where(u => 
                    u.FirstName.Contains(search) || 
                    u.LastName.Contains(search) || 
                    u.Email.Contains(search));
            }

            var users = await usersQuery
                .OrderBy(u => u.FirstName)
                .ToListAsync();

            var userViewModels = new List<UserManagementViewModel>();
            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var itemCount = await _context.Items.CountAsync(i => i.UserId == user.Id);
                var claimCount = await _context.ClaimRequests.CountAsync(c => c.ClaimantId == user.Id);

                userViewModels.Add(new UserManagementViewModel
                {
                    User = user,
                    Roles = userRoles.ToList(),
                    ItemsPosted = itemCount,
                    ClaimsSubmitted = claimCount
                });
            }

            if (!string.IsNullOrEmpty(role))
            {
                userViewModels = userViewModels.Where(u => u.Roles.Contains(role)).ToList();
            }

            ViewBag.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.SearchTerm = search;
            ViewBag.SelectedRole = role;

            return View(userViewModels);
        }

        // GET: Manage Items
        public async Task<IActionResult> Items(string? search, ItemStatus? status, ItemType? type)
        {
            var itemsQuery = _context.Items
                .Include(i => i.User)
                .Include(i => i.ClaimRequests)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                itemsQuery = itemsQuery.Where(i => 
                    i.Name.Contains(search) || 
                    i.Description.Contains(search) ||
                    i.Location.Contains(search));
            }

            if (status.HasValue)
            {
                itemsQuery = itemsQuery.Where(i => i.Status == status.Value);
            }

            if (type.HasValue)
            {
                itemsQuery = itemsQuery.Where(i => i.Type == type.Value);
            }

            var items = await itemsQuery
                .OrderByDescending(i => i.DateReported)
                .ToListAsync();

            ViewBag.SearchTerm = search;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedType = type;

            return View(items);
        }

        // GET: Manage Claims
        public async Task<IActionResult> Claims(ClaimStatus? status)
        {
            var claimsQuery = _context.ClaimRequests
                .Include(c => c.Item)
                    .ThenInclude(i => i.User)
                .Include(c => c.Claimant)
                .AsQueryable();

            if (status.HasValue)
            {
                claimsQuery = claimsQuery.Where(c => c.Status == status.Value);
            }

            var claims = await claimsQuery
                .OrderByDescending(c => c.RequestDate)
                .ToListAsync();

            ViewBag.SelectedStatus = status;
            return View(claims);
        }

        // POST: Archive Item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveItem(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                item.Status = ItemStatus.Resolved;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Item archived successfully.";
            }
            return RedirectToAction(nameof(Items));
        }

        // POST: Delete Item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.Items
                .Include(i => i.ClaimRequests)
                .FirstOrDefaultAsync(i => i.Id == id);
                
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Item deleted successfully.";
            }
            return RedirectToAction(nameof(Items));
        }

        // POST: Update User Role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, newRole);
                
                TempData["Success"] = $"User role updated to {newRole} successfully.";
            }
            return RedirectToAction(nameof(Users));
        }

        // GET: System Statistics
        public async Task<IActionResult> Statistics()
        {
            var stats = new SystemStatisticsViewModel
            {
                UserRegistrationsByMonth = await GetUserRegistrationsByMonth(),
                ItemPostsByMonth = await GetItemPostsByMonth(),
                ClaimSuccessRate = await GetClaimSuccessRate(),
                PopularLocations = await GetPopularLocations(),
                CategoryDistribution = await GetCategoryDistribution()
            };

            return View(stats);
        }

        private async Task<Dictionary<string, int>> GetUserRegistrationsByMonth()
        {
            var users = await _userManager.Users.ToListAsync();
            return users
                .GroupBy(u => u.CreatedAt.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => g.Count());
        }

        private async Task<Dictionary<string, int>> GetItemPostsByMonth()
        {
            return await _context.Items
                .GroupBy(i => i.DateReported.ToString("yyyy-MM"))
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Month, x => x.Count);
        }

        private async Task<double> GetClaimSuccessRate()
        {
            var totalClaims = await _context.ClaimRequests.CountAsync();
            var successfulClaims = await _context.ClaimRequests
                .CountAsync(c => c.Status == ClaimStatus.Completed);
            
            return totalClaims > 0 ? (double)successfulClaims / totalClaims * 100 : 0;
        }

        private async Task<Dictionary<string, int>> GetPopularLocations()
        {
            return await _context.Items
                .GroupBy(i => i.Location)
                .Select(g => new { Location = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToDictionaryAsync(x => x.Location, x => x.Count);
        }

        private async Task<Dictionary<ItemCategory, int>> GetCategoryDistribution()
        {
            return await _context.Items
                .GroupBy(i => i.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);
        }

        // POST: Deactivate User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = false;
                await _userManager.UpdateAsync(user);
                TempData["Success"] = $"User {user.FirstName} {user.LastName} has been deactivated.";
            }
            return RedirectToAction(nameof(Users));
        }

        // POST: Activate User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = true;
                await _userManager.UpdateAsync(user);
                TempData["Success"] = $"User {user.FirstName} {user.LastName} has been activated.";
            }
            return RedirectToAction(nameof(Users));
        }

        // POST: Reset User Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetUserPassword(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
                
                if (result.Succeeded)
                {
                    TempData["Success"] = $"Password reset for {user.FirstName} {user.LastName}. They should change it on next login.";
                }
                else
                {
                    TempData["Error"] = "Failed to reset password. " + string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            return RedirectToAction(nameof(Users));
        }

        // GET: User Posts (what user has posted)
        public async Task<IActionResult> UserPosts(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            var userPosts = await _context.Items
                .Include(i => i.User)
                .Include(i => i.ClaimRequests)
                .Where(i => i.UserId == id)
                .OrderByDescending(i => i.DateReported)
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.LostItems = userPosts.Where(i => i.Type == ItemType.Lost).ToList();
            ViewBag.FoundItems = userPosts.Where(i => i.Type == ItemType.Found).ToList();
            
            return View(userPosts);
        }

        // GET: User Claims (claims submitted by user)
        public async Task<IActionResult> UserClaims(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            var userClaims = await _context.ClaimRequests
                .Include(c => c.Item)
                .Include(c => c.Item.User)
                .Include(c => c.Claimant)
                .Where(c => c.ClaimantId == id)
                .OrderByDescending(c => c.RequestDate)
                .ToListAsync();

            ViewBag.User = user;
            return View(userClaims);
        }

        // GET: User Details (comprehensive overview)
        public async Task<IActionResult> UserDetails(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            
            var userPosts = await _context.Items
                .Where(i => i.UserId == id)
                .CountAsync();
                
            var userClaims = await _context.ClaimRequests
                .Where(c => c.ClaimantId == id)
                .CountAsync();

            var viewModel = new UserManagementViewModel
            {
                User = user,
                Roles = userRoles.ToList(),
                IsActive = user.IsActive,
                ItemsPosted = userPosts,
                ClaimsSubmitted = userClaims
            };

            return View(viewModel);
        }

        // POST: Admin Delete User Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserPost(int itemId, string userId)
        {
            var item = await _context.Items.FindAsync(itemId);
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Item deleted successfully.";
            }
            return RedirectToAction(nameof(UserPosts), new { id = userId });
        }

        // POST: Admin Edit User Post Status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePostStatus(int itemId, string userId, ItemStatus status)
        {
            var item = await _context.Items.FindAsync(itemId);
            if (item != null)
            {
                item.Status = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Item status updated to {status}.";
            }
            return RedirectToAction(nameof(UserPosts), new { id = userId });
        }

        // POST: Admin Delete User Claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserClaim(int claimId, string userId)
        {
            var claim = await _context.ClaimRequests.FindAsync(claimId);
            if (claim != null)
            {
                _context.ClaimRequests.Remove(claim);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Claim deleted successfully.";
            }
            return RedirectToAction(nameof(UserClaims), new { id = userId });
        }

        // POST: Admin Update Claim Status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateClaimStatus(int claimId, string userId, ClaimStatus status)
        {
            var claim = await _context.ClaimRequests.FindAsync(claimId);
            if (claim != null)
            {
                claim.Status = status;
                if (status == ClaimStatus.Approved)
                {
                    claim.ApprovedDate = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Claim status updated to {status}.";
            }
            return RedirectToAction(nameof(UserClaims), new { id = userId });
        }

        // GET: Items Management
        public async Task<IActionResult> Items()
        {
            var items = await _context.Items
                .Include(i => i.User)
                .Include(i => i.ClaimRequests)
                .OrderByDescending(i => i.DateReported)
                .ToListAsync();

            return View(items);
        }

        // GET: Escalated Items
        public async Task<IActionResult> EscalatedItems()
        {
            var escalatedItems = await _context.Items
                .Include(i => i.User)
                .Include(i => i.ClaimRequests)
                .ThenInclude(c => c.Claimant)
                .Where(i => i.IsEscalatedToAdmin && string.IsNullOrEmpty(i.AdminDecisionUserId))
                .OrderBy(i => i.EscalationDate)
                .ToListAsync();
            
            return View(escalatedItems);
        }

        // POST: Resolve Escalation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveEscalation(int itemId, int approvedClaimId, string adminNotes)
        {
            var admin = await _userManager.GetUserAsync(User);
            var item = await _context.Items
                .Include(i => i.ClaimRequests)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.IsEscalatedToAdmin);
            
            if (item != null)
            {
                item.AdminNotes = adminNotes;
                item.AdminDecisionUserId = admin!.Id;
                
                if (approvedClaimId > 0)
                {
                    // Approve selected claim, reject others
                    foreach (var claim in item.ClaimRequests.Where(c => c.Status == ClaimStatus.Pending))
                    {
                        if (claim.Id == approvedClaimId)
                        {
                            claim.Status = ClaimStatus.Approved;
                            claim.ApprovedDate = DateTime.UtcNow;
                        }
                        else
                        {
                            claim.Status = ClaimStatus.Rejected;
                        }
                    }
                    
                    // Mark item as resolved
                    item.Status = ItemStatus.Resolved;
                }
                else
                {
                    // Reject all claims
                    foreach (var claim in item.ClaimRequests.Where(c => c.Status == ClaimStatus.Pending))
                    {
                        claim.Status = ClaimStatus.Rejected;
                    }
                }
                
                await _context.SaveChangesAsync();
                TempData["Success"] = "Escalation resolved successfully. All parties will be notified.";
            }
            
            return RedirectToAction(nameof(EscalatedItems));
        }

        // GET: Pending Admin Requests
        public async Task<IActionResult> PendingRequests()
        {
            var pendingRequests = await _context.AdminRequests
                .Include(r => r.Item)
                .Include(r => r.Requester)
                .Include(r => r.Claimant)
                .Where(r => r.Status == AdminRequestStatus.Pending)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            return View(pendingRequests);
        }

        // GET: All Admin Requests
        public async Task<IActionResult> AllRequests()
        {
            var allRequests = await _context.AdminRequests
                .Include(r => r.Item)
                .Include(r => r.Requester)
                .Include(r => r.Claimant)
                .Include(r => r.ResolvedBy)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            return View(allRequests);
        }

        // GET: Request Details
        public async Task<IActionResult> RequestDetails(int id)
        {
            var request = await _context.AdminRequests
                .Include(r => r.Item)
                .ThenInclude(i => i.ClaimRequests)
                .ThenInclude(c => c.Claimant)
                .Include(r => r.Requester)
                .Include(r => r.Claimant)
                .Include(r => r.ResolvedBy)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int id, string? claimantEmail, string adminResponse)
        {
            var request = await _context.AdminRequests
                .Include(r => r.Item)
                .ThenInclude(i => i.ClaimRequests)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            var admin = await _userManager.GetUserAsync(User);
            
            // Validate claimant if provided
            string? validClaimantId = null;
            if (!string.IsNullOrWhiteSpace(claimantEmail))
            {
                var claimantUser = await _userManager.FindByEmailAsync(claimantEmail.Trim());
                if (claimantUser == null)
                {
                    claimantUser = await _userManager.FindByNameAsync(claimantEmail.Trim());
                }
                
                if (claimantUser != null)
                {
                    validClaimantId = claimantUser.Id;
                    
                    // Create or approve claim for this user
                    var existingClaim = request.Item.ClaimRequests
                        .FirstOrDefault(c => c.ClaimantId == validClaimantId);
                    
                    if (existingClaim != null)
                    {
                        existingClaim.Status = ClaimStatus.Approved;
                        existingClaim.ApprovedDate = DateTime.UtcNow;
                    }
                    else
                    {
                        // Create new claim request
                        var newClaim = new ClaimRequest
                        {
                            ItemId = request.ItemId,
                            ClaimantId = validClaimantId,
                            Status = ClaimStatus.Approved,
                            ApprovedDate = DateTime.UtcNow,
                            RequestDate = DateTime.UtcNow,
                            HandoverNotes = "Approved by admin"
                        };
                        _context.ClaimRequests.Add(newClaim);
                    }
                    
                    // Reject all other pending claims for this item
                    var otherClaims = request.Item.ClaimRequests
                        .Where(c => c.ClaimantId != validClaimantId && c.Status == ClaimStatus.Pending)
                        .ToList();
                    
                    foreach (var otherClaim in otherClaims)
                    {
                        otherClaim.Status = ClaimStatus.Rejected;
                    }
                    
                    // Mark item as claimed
                    request.Item.Status = ItemStatus.Claimed;
                }
                else
                {
                    TempData["Error"] = "Claimant user not found. Please check the email/username.";
                    return RedirectToAction("RequestDetails", new { id });
                }
            }
            
            request.Status = AdminRequestStatus.Approved;
            request.AdminResponse = adminResponse;
            request.ResolvedById = admin!.Id;
            request.ResolvedAt = DateTime.UtcNow;
            request.ClaimantId = validClaimantId;

            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Request approved successfully! Other pending claims have been rejected.";
            return RedirectToAction(nameof(PendingRequests));
        }

        [HttpPost]
        public async Task<IActionResult> RejectRequest(int id, string adminResponse)
        {
            var request = await _context.AdminRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            var admin = await _userManager.GetUserAsync(User);
            
            request.Status = AdminRequestStatus.Rejected;
            request.AdminResponse = adminResponse;
            request.ResolvedById = admin!.Id;
            request.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Request rejected!";
            return RedirectToAction(nameof(PendingRequests));
        }

        [HttpPost]
        public async Task<IActionResult> ResolveRequest(int id, string adminResponse)
        {
            var request = await _context.AdminRequests
                .Include(r => r.Item)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            var admin = await _userManager.GetUserAsync(User);
            
            request.Status = AdminRequestStatus.Resolved;
            request.AdminResponse = adminResponse;
            request.ResolvedById = admin!.Id;
            request.ResolvedAt = DateTime.UtcNow;
            
            // Mark item as resolved
            request.Item.Status = ItemStatus.Resolved;

            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Request resolved successfully!";
            return RedirectToAction(nameof(AllRequests));
        }
    }
} 
