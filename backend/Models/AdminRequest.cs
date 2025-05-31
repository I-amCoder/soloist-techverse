using AspnetCoreMvcFull.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models
{
    public enum AdminRequestType
    {
        ClaimDispute,
        VerifyOwnership,
        ItemResolution,
        Other
    }

    public enum AdminRequestStatus
    {
        Pending,
        Approved,
        Rejected,
        Resolved
    }

    public class AdminRequest
    {
        public int Id { get; set; }
        
        [Required]
        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;
        
        [Required]
        public string RequesterId { get; set; } = string.Empty;
        public ApplicationUser Requester { get; set; } = null!;
        
        public string? ClaimantId { get; set; }
        public ApplicationUser? Claimant { get; set; }
        
        [Required]
        public AdminRequestType Type { get; set; }
        
        [Required]
        [StringLength(500)]
        public string HandoverNotes { get; set; } = string.Empty;
        
        public AdminRequestStatus Status { get; set; } = AdminRequestStatus.Pending;
        
        public string? AdminResponse { get; set; }
        
        public string? ResolvedById { get; set; }
        public ApplicationUser? ResolvedBy { get; set; }
        
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
    }
} 