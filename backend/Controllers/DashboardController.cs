using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Models.Identity;

namespace AspnetCoreMvcFull.Controllers;

[Authorize]

public class DashboardController : Controller
{
  private readonly ApplicationDbContext _context;
  private readonly UserManager<ApplicationUser> _userManager;

  public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
  {
    _context = context;
    _userManager = userManager;
  }

  // Student Dashboard (Default)
  public async Task<IActionResult> Index()
  {
    var user = await _userManager.GetUserAsync(User);
    var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
    
    // Add null check
    if (user == null)
    {
        return RedirectToAction("Login", "Account");
    }

    // Get statistics for all users
    ViewBag.TotalLostItems = await _context.Items
        .Where(i => i.Type == ItemType.Lost)
        .CountAsync();

    ViewBag.TotalFoundItems = await _context.Items
        .Where(i => i.Type == ItemType.Found)
        .CountAsync();

    ViewBag.ReunitedItems = await _context.Items
        .Where(i => i.Status == ItemStatus.Resolved)
        .CountAsync();

    ViewBag.MyLostCount = await _context.Items
        .Where(i => i.UserId == user.Id && i.Type == ItemType.Lost)
        .CountAsync();

    ViewBag.MyFoundCount = await _context.Items
        .Where(i => i.UserId == user.Id && i.Type == ItemType.Found)
        .CountAsync();

    // Get recent items
    ViewBag.RecentLostItems = await _context.Items
        .Where(i => i.Type == ItemType.Lost && i.Status == ItemStatus.Active)
        .OrderByDescending(i => i.DateReported)
        .Take(5)
        .ToListAsync();

    ViewBag.RecentFoundItems = await _context.Items
        .Where(i => i.Type == ItemType.Found && i.Status == ItemStatus.Active)
        .OrderByDescending(i => i.DateReported)
        .Take(5)
        .ToListAsync();

    // Admin-specific data
    if (isAdmin)
    {
        ViewBag.PendingAdminRequests = await _context.AdminRequests
            .Where(r => r.Status == AdminRequestStatus.Pending)
            .CountAsync();

        ViewBag.EscalatedItems = await _context.Items
            .Where(i => i.IsEscalatedToAdmin && string.IsNullOrEmpty(i.AdminDecisionUserId))
            .CountAsync();

        ViewBag.TotalUsers = await _context.Users.CountAsync();

        ViewBag.ResolvedToday = await _context.Items
            .Where(i => i.Status == ItemStatus.Resolved && 
                       i.DateReported.Date == DateTime.Today)
            .CountAsync();

        ViewBag.RecentAdminRequests = await _context.AdminRequests
            .Include(r => r.Item)
            .Include(r => r.Requester)
            .OrderByDescending(r => r.RequestedAt)
            .Take(5)
            .ToListAsync();
    }

    return View();
  }

  // Manager Dashboard
  [Authorize(Roles = "Manager,Admin")]
  public async Task<IActionResult> Manager()
  {
    // Manager-specific stats
    var pendingItems = await _context.Items
      .Where(i => i.Status == ItemStatus.Active)
      .CountAsync();

    ViewBag.PendingItems = pendingItems;
    return View();
  }

  // Admin Dashboard
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Admin()
  {
    // Admin-specific stats
    var totalUsers = await _context.Users.CountAsync();
    var totalItems = await _context.Items.CountAsync();
    
    ViewBag.TotalUsers = totalUsers;
    ViewBag.TotalItems = totalItems;
    return View();
  }
}
