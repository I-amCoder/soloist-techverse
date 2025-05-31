using System.ComponentModel.DataAnnotations;
using AspnetCoreMvcFull.Models.Identity;

namespace AspnetCoreMvcFull.Models
{
    public enum ItemType
    {
        Lost, Found
    }

    public enum ItemCategory
    {
        Electronics, Documents, Clothing, Books, Accessories, IDCard, WaterBottle, USBDrive, Notebook, Other
    }

    public enum ItemStatus
    {
        Active,         // Item is still lost/found and available for claims
        Claimed,        // Someone's claim has been approved, coordination in progress
        HandedOver,     // Owner has given item to claimant (pending confirmation)
        Resolved        // Item successfully returned to rightful owner
    }

    public enum ClaimStatus
    {
        Pending,        // Waiting for owner response
        Approved,       // Owner approved the claim
        Rejected,       // Owner rejected the claim
        HandedOver,     // Item has been given to claimant
        Completed       // Both parties confirmed successful return
    }

    public class Item
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        public ItemType Type { get; set; } // Lost or Found
        public ItemCategory Category { get; set; }
        public ItemStatus Status { get; set; } = ItemStatus.Active;
        
        [Required]
        public string Location { get; set; } = string.Empty;
        
        public DateTime DateReported { get; set; } = DateTime.UtcNow;
        public DateTime? DateLostOrFound { get; set; }
        
        public string? ImagePath { get; set; }
        public string ContactInfo { get; set; } = string.Empty;
        
        // Foreign Keys
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        
        // Navigation
        public List<ClaimRequest> ClaimRequests { get; set; } = new();
        
        public bool IsEscalatedToAdmin { get; set; } = false;
        public DateTime? EscalationDate { get; set; }
        public string? EscalationReason { get; set; }
        public string? AdminNotes { get; set; }
        public string? AdminDecisionUserId { get; set; } // Which admin handled it
        public DateTime? DateResolved { get; set; }
    }

    public class ClaimRequest
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string ClaimantId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedDate { get; set; }
        public DateTime? HandedOverDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? HandoverNotes { get; set; }
        
        // Navigation properties
        public Item Item { get; set; } = null!;
        public ApplicationUser Claimant { get; set; } = null!;
    }
} 