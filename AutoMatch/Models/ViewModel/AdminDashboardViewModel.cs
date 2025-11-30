using AutoMatch.Services;
using System.Collections.Generic;

namespace AutoMatch.ViewModels
{
    public class AdminDashboardViewModel
    {
        public AdminDashboardStats Stats { get; set; }
        public List<RecentReportDto> RecentReports { get; set; }
        public List<RecentActivityDto> RecentActivities { get; set; }
        public List<ListingsByTypeDto> ListingsByType { get; set; }
        public List<UserGrowthDto> UserGrowth { get; set; }

        public AdminDashboardViewModel()
        {
            Stats = new AdminDashboardStats();
            RecentReports = new List<RecentReportDto>();
            RecentActivities = new List<RecentActivityDto>();
            ListingsByType = new List<ListingsByTypeDto>();
            UserGrowth = new List<UserGrowthDto>();
        }
    }
}