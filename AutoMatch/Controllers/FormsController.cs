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

            // Recarrega opções do dropdown de cidades/códigos postais
            LoadPostalCodeOptions();

            // Tem de aceitar os termos para o formulário ser considerado válido
            if (!model.AcceptTerms)
            {
                ModelState.AddModelError(nameof(model.AcceptTerms), "You must accept the Seller Terms & Conditions.");
            }

            if (!ModelState.IsValid)
            {
                // Repreenche dados de leitura (nome/email/username) caso venham a null
                var baseModel = BuildSellerFormViewModel(userId.Value);
                model.FullName = baseModel.FullName;
                model.Email = baseModel.Email;
                model.UserName = baseModel.UserName;

                return View(model);
            }

            // Neste ponto todos os campos obrigatórios (menos ID) estão preenchidos e os termos aceites.
            // Aqui poderás enviar o email para o administrador e, após aprovação, criar o registo Vendedor.

            TempData["SellerFormSubmitted"] = "Your application has been sent for review.";
            return RedirectToAction("SellerForms");
        }

        private SellerFormViewModel BuildSellerFormViewModel(int userId)
        {
            // Dados básicos do utilizador
            var user = _db.Utilizadores.FirstOrDefault(u => u.Id_User == userId);

            // Dados de comprador e vendedor (se existirem)
            var comprador = _db.Compradores.FirstOrDefault(c => c.Id_User == userId);
            var vendedor = _db.Vendedores.FirstOrDefault(v => v.Id_User == userId);

            // Código postal: prioriza Vendedor, senão Comprador
            string? rawPostalCode = vendedor?.Codigo_Postal ?? comprador?.Codigo_Postal;
            // Se for o valor default "0000-000" não conta como preenchido
            string? postalCode = rawPostalCode == "0000-000" ? null : rawPostalCode;

            // Contactos: prioriza Vendedor, senão Comprador
            string? rawContactos = vendedor?.Contactos ?? comprador?.Contactos;
            // Se for "N/A" ou vazio, também não conta como preenchido
            string? contactos = string.IsNullOrWhiteSpace(rawContactos) || rawContactos == "N/A" ? null : rawContactos;

            // Opções de cidade/código postal para o dropdown
            LoadPostalCodeOptions();

            return new SellerFormViewModel
            {
                SellingType = vendedor != null && vendedor.Tipo ? "Professional" : "Individual",
                DocumentNumber = vendedor?.NIF?.ToString(),
                PostalCode = postalCode,
                PreferredContactMethod = "Email", // por agora fixo
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

            // Primeiro item é placeholder - obriga o utilizador a escolher
            postalOptions.Insert(0, new SelectListItem
            {
                Value = string.Empty,
                Text = "Select city"
            });

            ViewBag.PostalCodeOptions = postalOptions;
        }
    }
}
