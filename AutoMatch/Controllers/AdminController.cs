using Microsoft.AspNetCore.Mvc;
using AutoMatch.Services;
using AutoMatch.ViewModels;
using System.Threading.Tasks;

namespace AutoMatch.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> DashAdmin()
        {
            var viewModel = new AdminDashboardViewModel();

            try
            {
                // Busca todas as estatísticas da BD
                viewModel.Stats = await _adminService.GetDashboardStatsAsync();
                viewModel.RecentReports = await _adminService.GetRecentReportsAsync();
                viewModel.RecentActivities = await _adminService.GetRecentActivityAsync();
                viewModel.ListingsByType = await _adminService.GetListingsByTypeAsync();
                viewModel.UserGrowth = await _adminService.GetUserGrowthAsync();
            }
            catch (Exception ex)
            {
                // Log do erro
                System.Diagnostics.Debug.WriteLine($"Error loading admin dashboard: {ex.Message}");
            }

            return View(viewModel);
        }
    }
}