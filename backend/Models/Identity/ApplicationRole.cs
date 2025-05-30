using Microsoft.AspNetCore.Identity;

namespace AspnetCoreMvcFull.Models.Identity
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
} 
