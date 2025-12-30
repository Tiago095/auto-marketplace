using AutoMatch.Data;
using AutoMatch.Models;
using AutoMatch.Models.ViewModel;
using AutoMatch.Models.ViewModels;
using AutoMatch.Services;
using AutoMatch.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AutoMatch.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly AutoMatchContext _context;

        public AdminController(IAdminService adminService, AutoMatchContext context)
        {
            _adminService = adminService;
            _context = context;
        }

        public async Task<IActionResult> DashAdmin()
        {
            var viewModel = new AdminDashboardViewModel();
            try
            {
                viewModel.Stats = await _adminService.GetDashboardStatsAsync();
                viewModel.RecentReports = await _adminService.GetRecentReportsAsync();
                viewModel.RecentActivities = await _adminService.GetRecentActivityAsync();
                viewModel.ListingsByType = await _adminService.GetListingsByTypeAsync();
                viewModel.UserGrowth = await _adminService.GetUserGrowthAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading admin dashboard: {ex.Message}");
            }

            return View(viewModel);
        }

        public async Task<IActionResult> AdminForms()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Check if admin
            bool isAdmin = await _context.Administradores
                .AnyAsync(a => a.Id_User == userId);

            if (!isAdmin)
                return RedirectToAction("Index", "Dashboard");

            var viewModel = new AdminFormsViewModel
            {
                FormSubmissions = new List<FormSubmissionViewModel>()
            };

            try
            {
                // Fetch pending seller applications from database
                var applications = await _context.SellerApplications
                    .Include(sa => sa.User)
                    .Where(sa => sa.Status == "Pending")
                    .OrderByDescending(sa => sa.SubmissionDate)
                    .ToListAsync();

                foreach (var app in applications)
                {
                    viewModel.FormSubmissions.Add(new FormSubmissionViewModel
                    {
                        RequestId = app.Id.ToString(),
                        Username = app.User?.Nome ?? "Unknown",
                        SubmissionDate = app.SubmissionDate,
                        ApplicationId = app.Id
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading admin forms: {ex.Message}");
            }

            return View(viewModel);
        }

        public async Task<IActionResult> FormDetails(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Check if admin
            bool isAdmin = await _context.Administradores
                .AnyAsync(a => a.Id_User == userId);

            if (!isAdmin)
                return RedirectToAction("Index", "Dashboard");

            var application = await _context.SellerApplications
                .Include(sa => sa.User)
                .FirstOrDefaultAsync(sa => sa.Id == id);

            if (application == null)
                return NotFound();

            var viewModel = new FormDetailsViewModel
            {
                ApplicationId = application.Id,
                Username = application.User?.Nome ?? "Unknown",
                Email = application.User?.Email ?? "N/A",
                SellingType = application.SellingType,
                DocumentNumber = application.DocumentNumber,
                PhoneNumber = application.PhoneNumber,
                PostalCode = application.PostalCode,
                PreferredContactMethod = application.PreferredContactMethod,
                SubmissionDate = application.SubmissionDate,
                Status = application.Status
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveApplication(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Check if admin
            bool isAdmin = await _context.Administradores
                .AnyAsync(a => a.Id_User == userId);

            if (!isAdmin)
                return Unauthorized();

            var application = await _context.SellerApplications
                .Include(sa => sa.User)
                .FirstOrDefaultAsync(sa => sa.Id == id);

            if (application == null)
                return NotFound();

            if (application.Status != "Pending")
            {
                TempData["Error"] = "This application has already been reviewed.";
                return RedirectToAction("AdminForms");
            }

            // Check if user is already a seller
            var existingSeller = await _context.Vendedores
                .FirstOrDefaultAsync(v => v.Id_User == application.UserId);

            if (existingSeller != null)
            {
                TempData["Error"] = "User is already a seller.";
                return RedirectToAction("AdminForms");
            }

            // Create new Vendedor record
            var vendedor = new Vendedor
            {
                Id_User = application.UserId,
                NIF = string.IsNullOrEmpty(application.DocumentNumber) ? 0 : int.Parse(application.DocumentNumber),
                Tipo = application.SellingType == "Professional",
                Contactos = application.PhoneNumber,
                Codigo_Postal = application.PostalCode,
                Rua = application.PostalCode // Set Rua to PostalCode as requested
            };

            _context.Vendedores.Add(vendedor);

            // Update application status
            application.Status = "Approved";
            application.ReviewedDate = DateTime.UtcNow;
            application.ReviewedByAdminId = userId;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Application for {application.User?.Nome} has been approved!";
            return RedirectToAction("AdminForms");
        }

        [HttpPost]
        public async Task<IActionResult> RejectApplication(int id, string reason)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Check if admin
            bool isAdmin = await _context.Administradores
                .AnyAsync(a => a.Id_User == userId);

            if (!isAdmin)
                return Unauthorized();

            var application = await _context.SellerApplications
                .FirstOrDefaultAsync(sa => sa.Id == id);

            if (application == null)
                return NotFound();

            if (application.Status != "Pending")
            {
                TempData["Error"] = "This application has already been reviewed.";
                return RedirectToAction("AdminForms");
            }

            // Update application status
            application.Status = "Rejected";
            application.ReviewedDate = DateTime.UtcNow;
            application.ReviewedByAdminId = userId;
            application.RejectionReason = reason ?? "Not specified";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Application has been rejected.";
            return RedirectToAction("AdminForms");
        }
    }
}