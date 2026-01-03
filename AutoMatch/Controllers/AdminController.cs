using AutoMatch.Data;
using AutoMatch.Models;
using AutoMatch.Models.ViewModel;
using AutoMatch.Models.ViewModels;
using AutoMatch.Services;
using AutoMatch.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
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

            // Check if user is already a seller (not a temporary one from rejection notification)
            var existingSeller = await _context.Vendedores
                .FirstOrDefaultAsync(v => v.Id_User == application.UserId);

            if (existingSeller != null)
            {
                // Check if it's a temporary seller (created for notification purposes)
                bool isTemporary = existingSeller.Contactos == "N/A" && 
                                   existingSeller.Rua == "Desconhecida" && 
                                   existingSeller.Codigo_Postal == "0000-000";

                if (isTemporary)
                {
                    // Update temporary seller with real data
                    existingSeller.NIF = string.IsNullOrEmpty(application.DocumentNumber) ? 0 : int.Parse(application.DocumentNumber);
                    existingSeller.Tipo = application.SellingType == "Professional";
                    existingSeller.Contactos = application.PhoneNumber;
                    existingSeller.Codigo_Postal = application.PostalCode;
                    existingSeller.Rua = application.PostalCode;
                    _context.Vendedores.Update(existingSeller);
                }
                else
                {
                    TempData["Error"] = "User is already a seller.";
                    return RedirectToAction("AdminForms");
                }
            }
            else
            {
                // Create new Vendedor record
                var vendedor = new Vendedor
                {
                    Id_User = application.UserId,
                    NIF = string.IsNullOrEmpty(application.DocumentNumber) ? 0 : int.Parse(application.DocumentNumber),
                    Tipo = application.SellingType == "Professional",
                    Contactos = application.PhoneNumber,
                    Codigo_Postal = application.PostalCode,
                    Rua = application.PostalCode
                };

                _context.Vendedores.Add(vendedor);
            }

            // Update application status
            application.Status = "Approved";
            application.ReviewedDate = DateTime.UtcNow;
            application.ReviewedByAdminId = userId;

            await _context.SaveChangesAsync();

            // Create notification for the user
            await CreateApplicationNotificationAsync(application.UserId, "Approved", "Your seller application has been approved! You can now create listings.");

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

            // Create notification for the user
            var rejectionMessage = string.IsNullOrWhiteSpace(reason) 
                ? "Your seller application has been rejected." 
                : $"Your seller application has been rejected. Reason: {reason}";
            await CreateApplicationNotificationAsync(application.UserId, "Rejected", rejectionMessage);

            TempData["Success"] = "Application has been rejected.";
            return RedirectToAction("AdminForms");
        }

        private async Task CreateApplicationNotificationAsync(int userId, string status, string message)
        {
            // Ensure user exists as Comprador
            var compradorExiste = await _context.Compradores.AnyAsync(c => c.Id_User == userId);
            if (!compradorExiste)
            {
                // Ensure default postal code exists
                var codigoPostalExiste = await _context.CodigoPostais.AnyAsync(cp => cp.Codigo_Postal == "0000-000");
                if (!codigoPostalExiste)
                {
                    var novoCodigoPostal = new CodigoPostal
                    {
                        Codigo_Postal = "0000-000",
                        Localidade = "Desconhecida"
                    };
                    _context.CodigoPostais.Add(novoCodigoPostal);
                    await _context.SaveChangesAsync();
                }

                var novoComprador = new Comprador
                {
                    Id_User = userId,
                    Contactos = "N/A",
                    Rua = "Desconhecida",
                    Codigo_Postal = "0000-000"
                };
                _context.Compradores.Add(novoComprador);
                await _context.SaveChangesAsync();
            }

            // Ensure user exists as Vendedor (create temporary record if needed for notification structure)
            var vendedorExiste = await _context.Vendedores.AnyAsync(v => v.Id_User == userId);
            if (!vendedorExiste)
            {
                // Ensure default postal code exists
                var codigoPostalExiste = await _context.CodigoPostais.AnyAsync(cp => cp.Codigo_Postal == "0000-000");
                if (!codigoPostalExiste)
                {
                    var novoCodigoPostal = new CodigoPostal
                    {
                        Codigo_Postal = "0000-000",
                        Localidade = "Desconhecida"
                    };
                    _context.CodigoPostais.Add(novoCodigoPostal);
                    await _context.SaveChangesAsync();
                }

                var novoVendedor = new Vendedor
                {
                    Id_User = userId,
                    Tipo = false,
                    Contactos = "N/A",
                    Rua = "Desconhecida",
                    Codigo_Postal = "0000-000"
                };
                _context.Vendedores.Add(novoVendedor);
                await _context.SaveChangesAsync();
            }

            // Get Comprador and Vendedor IDs
            var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.Id_User == userId);
            var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.Id_User == userId);

            if (comprador != null && vendedor != null)
            {
                // Create notification: Id_Comprador = user (recipient), Id_Vendedor = user (recipient)
                // This is a system notification, so we use the same user for both
                var notificacao = new Notificacoes
                {
                    Id_Comprador = comprador.Id_User,
                    Id_Vendedor = vendedor.Id_User,
                    Tipo = "SellerApplication",
                    Mensagem = message,
                    Data_Envio = DateTime.Now,
                    Estado = false // Not read
                };

                _context.Notificacoes.Add(notificacao);
                await _context.SaveChangesAsync();
            }
        }
    }
}