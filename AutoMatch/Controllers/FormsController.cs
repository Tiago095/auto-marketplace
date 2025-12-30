using AutoMatch.Data;
using AutoMatch.Models;
using AutoMatch.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AutoMatch.Controllers
{
    public class FormsController : Controller
    {
        private readonly AutoMatchContext _db;

        public FormsController(AutoMatchContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult SellerForms()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Optional: show a message if there is already a pending application
            var existingApplication = _db.SellerApplications
                .FirstOrDefault(sa => sa.UserId == userId.Value && sa.Status == "Pending");
            if (existingApplication != null)
            {
                TempData["Info"] = "You already have a pending application under review.";
            }

            // If already a seller, don’t let them apply again
            var isAlreadySeller = _db.Vendedores.Any(v => v.Id_User == userId);
            if (isAlreadySeller)
            {
                TempData["Info"] = "You are already registered as a seller.";
                return RedirectToAction("MyListings", "Listings");
            }

            var model = BuildSellerFormViewModel(userId.Value);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SellerForms(SellerFormViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            LoadPostalCodeOptions();

            // Block multiple pending applications
            var existingApplication = _db.SellerApplications
                .FirstOrDefault(sa => sa.UserId == userId.Value && sa.Status == "Pending");
            if (existingApplication != null)
            {
                TempData["Error"] = "You already have a pending application. Please wait for review.";
                return RedirectToAction("SellerForms");
            }

            // Block if already a seller
            var isAlreadySeller = _db.Vendedores.Any(v => v.Id_User == userId.Value);
            if (isAlreadySeller)
            {
                TempData["Error"] = "You are already registered as a seller.";
                return RedirectToAction("SellerForms");
            }

            // Validate terms acceptance
            if (!model.AcceptTerms)
            {
                ModelState.AddModelError(nameof(model.AcceptTerms), "You must accept the Seller Terms & Conditions.");
            }

            if (!ModelState.IsValid)
            {
                var baseModel = BuildSellerFormViewModel(userId.Value);
                model.FullName = baseModel.FullName;
                model.Email = baseModel.Email;
                model.UserName = baseModel.UserName;
                return View(model);
            }

            // Create new seller application (PENDING)
            var application = new SellerApplication
            {
                UserId = userId.Value,
                SellingType = model.SellingType,
                DocumentNumber = model.DocumentNumber ?? string.Empty,
                PhoneNumber = model.PhoneNumber,
                PostalCode = model.PostalCode,
                PreferredContactMethod = model.PreferredContactMethod,
                AcceptTerms = model.AcceptTerms,
                SubmissionDate = DateTime.UtcNow,
                Status = "Pending",
                RejectionReason = string.Empty // Empty string for pending applications, will be set if rejected
            };

            _db.SellerApplications.Add(application);
            _db.SaveChanges();

            TempData["SellerFormSubmitted"] = "Your seller application has been successfully submitted and is now under review. You will be notified within 24-48 hours.";
            return RedirectToAction("SellerForms");
        }

        private SellerFormViewModel BuildSellerFormViewModel(int userId)
        {
            var user = _db.Utilizadores.FirstOrDefault(u => u.Id_User == userId);
            var comprador = _db.Compradores.FirstOrDefault(c => c.Id_User == userId);
            var vendedor = _db.Vendedores.FirstOrDefault(v => v.Id_User == userId);

            string? rawPostalCode = vendedor?.Codigo_Postal ?? comprador?.Codigo_Postal;
            string? postalCode = rawPostalCode == "0000-000" ? null : rawPostalCode;

            string? rawContactos = vendedor?.Contactos ?? comprador?.Contactos;
            string? contactos = string.IsNullOrWhiteSpace(rawContactos) || rawContactos == "N/A" ? null : rawContactos;

            LoadPostalCodeOptions();

            return new SellerFormViewModel
            {
                SellingType = vendedor != null && vendedor.Tipo ? "Professional" : "Individual",
                DocumentNumber = vendedor?.NIF?.ToString(),
                PostalCode = postalCode,
                PreferredContactMethod = "Email",
                PhoneNumber = contactos,
                FullName = user?.Nome,
                Email = user?.Email,
                UserName = user?.UserName
            };
        }

        private void LoadPostalCodeOptions()
        {
            var postalOptions = _db.CodigoPostais
                .OrderBy(c => c.Localidade)
                .Select(c => new SelectListItem
                {
                    Value = c.Codigo_Postal,
                    Text = c.Localidade + " (" + c.Codigo_Postal + ")"
                })
                .ToList();

            postalOptions.Insert(0, new SelectListItem
            {
                Value = string.Empty,
                Text = "Select city"
            });

            ViewBag.PostalCodeOptions = postalOptions;
        }
    }
}