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
    
    // Get user's items
    var myLostItems = await _context.Items
      .Where(i => i.UserId == user!.Id && i.Type == ItemType.Lost && i.Status == ItemStatus.Active)
      .CountAsync();
      
    var myFoundItems = await _context.Items
      .Where(i => i.UserId == user!.Id && i.Type == ItemType.Found && i.Status == ItemStatus.Active)
      .CountAsync();

    // Get recent campus activity
    var recentLostItems = await _context.Items
      .Include(i => i.User)
      .Where(i => i.Type == ItemType.Lost && i.Status == ItemStatus.Active)
      .OrderByDescending(i => i.DateReported)
      .Take(5)
      .ToListAsync();
      
    var recentFoundItems = await _context.Items
      .Include(i => i.User)
      .Where(i => i.Type == ItemType.Found && i.Status == ItemStatus.Active)
      .OrderByDescending(i => i.DateReported)
      .Take(5)
      .ToListAsync();

    ViewBag.MyLostCount = myLostItems;
    ViewBag.MyFoundCount = myFoundItems;
    ViewBag.RecentLostItems = recentLostItems;
    ViewBag.RecentFoundItems = recentFoundItems;
    ViewBag.TotalLostItems = await _context.Items.Where(i => i.Type == ItemType.Lost && i.Status == ItemStatus.Active).CountAsync();
    ViewBag.TotalFoundItems = await _context.Items.Where(i => i.Type == ItemType.Found && i.Status == ItemStatus.Active).CountAsync();

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
