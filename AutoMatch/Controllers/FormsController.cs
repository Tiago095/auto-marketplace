using AutoMatch.Data;
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

            // Tem de aceitar os termos para o formulário ser considerado válido
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

            TempData["SellerFormSubmitted"] = "Your application has been sent for review.";
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
