using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required] 
        public string LastName { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
} 