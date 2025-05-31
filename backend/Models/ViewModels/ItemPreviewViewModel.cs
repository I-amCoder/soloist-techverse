namespace AspnetCoreMvcFull.Models.ViewModels
{
    public class ItemPreviewViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ItemCategory Category { get; set; }
        public string Location { get; set; } = string.Empty;
        public ItemType Type { get; set; }
        public DateTime DateReported { get; set; }
        public string? ImagePath { get; set; }
        public string ReporterName { get; set; } = string.Empty;
        public string ContactMethod { get; set; } = string.Empty;
        public bool IsOwnItem { get; set; }
        
        // Computed properties
        public string TypeBadgeClass => Type == ItemType.Lost ? "danger" : "success";
        public string TypeIcon => Type == ItemType.Lost ? "bx-search" : "bx-check-circle";
        public string DaysAgo => (DateTime.UtcNow - DateReported).Days.ToString();
    }
} 