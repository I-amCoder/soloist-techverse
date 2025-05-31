namespace AspnetCoreMvcFull.Models.ViewModels
{
    public class PublicItemsViewModel
    {
        public List<PublicItemViewModel> Items { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public string? CurrentType { get; set; }
        public string? CurrentCategory { get; set; }
        public string? CurrentSearch { get; set; }
        
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class PublicItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ItemCategory Category { get; set; }
        public ItemType Type { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime DateReported { get; set; }
        public string? ImagePath { get; set; }
        public string ReporterName { get; set; } = string.Empty;
        
        // Computed properties
        public string TypeBadgeClass => Type == ItemType.Lost ? "danger" : "success";
        public string TypeIcon => Type == ItemType.Lost ? "bx-search" : "bx-check-circle";
        public string DaysAgo => (DateTime.UtcNow - DateReported).Days.ToString();
        public string ShortDescription => Description.Length > 100 ? Description.Substring(0, 100) + "..." : Description;
    }
} 