using AspnetCoreMvcFull.Models.Identity;

namespace AspnetCoreMvcFull.Models.ViewModels
{
    public class HomePageViewModel
    {
        // Statistics for the hero section
        public int TotalItemsReported { get; set; }
        public int TotalItemsReturned { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalSuccessStories { get; set; }
        
        // Recent items for display
        public List<Item> RecentItems { get; set; } = new();
        public List<Item> FeaturedItems { get; set; } = new();
        
        // Category statistics
        public Dictionary<ItemCategory, int> PopularCategories { get; set; } = new();
        
        // User information
        public bool IsUserLoggedIn { get; set; }
        public string? UserName { get; set; }
        public int UserItemsCount { get; set; }
        public int UserClaimsCount { get; set; }
        
        // Quick stats for categories
        public int LostItemsCount { get; set; }
        public int FoundItemsCount { get; set; }
        public int RecentlyResolvedCount { get; set; }
        
        // Recent success stories
        public List<SuccessStoryViewModel> RecentSuccessStories { get; set; } = new();
    }
    
    public class SuccessStoryViewModel
    {
        public string ItemName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public DateTime ResolvedDate { get; set; }
        public string Category { get; set; } = string.Empty;
    }
} 