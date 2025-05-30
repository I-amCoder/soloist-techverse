using AspnetCoreMvcFull.Models.Identity;

namespace AspnetCoreMvcFull.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalItems { get; set; }
        public int TotalClaims { get; set; }
        public int ActiveItems { get; set; }
        public int PendingClaims { get; set; }
        public int ResolvedItems { get; set; }
        public int LostItemsCount { get; set; }
        public int FoundItemsCount { get; set; }

        public List<Item> RecentItems { get; set; } = new();
        public List<ClaimRequest> RecentClaims { get; set; } = new();
        public Dictionary<ItemCategory, int> ItemsByCategory { get; set; } = new();
    }

    public class UserManagementViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
        public int ItemsPosted { get; set; }
        public int ClaimsSubmitted { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SystemStatisticsViewModel
    {
        public Dictionary<string, int> UserRegistrationsByMonth { get; set; } = new();
        public Dictionary<string, int> ItemPostsByMonth { get; set; } = new();
        public double ClaimSuccessRate { get; set; }
        public Dictionary<string, int> PopularLocations { get; set; } = new();
        public Dictionary<ItemCategory, int> CategoryDistribution { get; set; } = new();
        
        // Additional metrics
        public int TotalActiveUsers { get; set; }
        public int AverageClaimsPerItem { get; set; }
        public string MostActiveLocation { get; set; } = string.Empty;
        public ItemCategory MostLostCategory { get; set; }
        public TimeSpan AverageResolutionTime { get; set; }
    }

    public class AdminItemDetailsViewModel
    {
        public Item Item { get; set; } = null!;
        public List<ClaimRequest> Claims { get; set; } = new();
        public bool CanArchive { get; set; }
        public bool CanDelete { get; set; }
        public string AdminNotes { get; set; } = string.Empty;
    }

    public class ReportViewModel
    {
        public DateTime StartDate { get; set; } = DateTime.UtcNow.AddDays(-30);
        public DateTime EndDate { get; set; } = DateTime.UtcNow;
        public ReportType Type { get; set; }
        
        // Report data
        public List<ReportDataPoint> Data { get; set; } = new();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class ReportDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public enum ReportType
    {
        UserActivity,
        ItemPosts,
        ClaimActivity,
        CategoryTrends,
        LocationAnalysis,
        ResolutionMetrics
    }
} 