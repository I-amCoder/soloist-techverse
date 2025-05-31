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
        
        // Category statistics
        public int LostItemsCount { get; set; }
        public int FoundItemsCount { get; set; }
        public int RecentlyResolvedCount { get; set; }
        
        // Popular categories
        public List<CategoryStatsViewModel> PopularCategories { get; set; } = new();
        
        // Recent items
        public List<RecentItemViewModel> RecentItems { get; set; } = new();
        
        // Success stories
        public List<SuccessStoryViewModel> RecentSuccessStories { get; set; } = new();
        
        // User information
        public bool IsUserLoggedIn { get; set; }
        public string? UserName { get; set; }
        public int UserItemsCount { get; set; }
        public int UserClaimsCount { get; set; }
    }
    
    public class RecentItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ItemCategory Category { get; set; }
        public ItemType Type { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime DateReported { get; set; }
        public string? ImagePath { get; set; }
    }
    
    public class SuccessStoryViewModel
    {
        public string ItemName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public DateTime ResolvedDate { get; set; }
    }
} 