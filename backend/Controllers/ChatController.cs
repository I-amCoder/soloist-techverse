using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Models.Identity;
using Microsoft.AspNetCore.SignalR;
using AspnetCoreMvcFull.Hubs;

namespace AspnetCoreMvcFull.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
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

            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Content))
            {
                return BadRequest("Message content cannot be empty.");
            }

            var sender = await _userManager.GetUserAsync(User);
            if (sender == null)
            {
                return Unauthorized();
            }

            var message = new Message
            {
                SenderId = sender.Id,
                ReceiverId = model.ReceiverId,
                Content = model.Content,
                SentAt = DateTime.UtcNow
            };

            try
            {
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"Sending message to {model.ReceiverId}: {message.Content}");
                
                // Send to the specific user
                await _hubContext.Clients.User(model.ReceiverId).SendAsync("ReceiveMessage", message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }

            return Ok();
        }
    }
}
