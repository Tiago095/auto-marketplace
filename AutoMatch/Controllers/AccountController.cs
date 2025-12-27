using AutoMatch.Data;
using AutoMatch.Models;
using AutoMatch.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AutoMatch.Controllers
{
    public class AccountController : Controller
    {
        private readonly AutoMatchContext _db;

        public AccountController(AutoMatchContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // ===== POST LOGIN =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.Clear();
                TempData["ShowRegister"] = null;
                return View(new LoginViewModel());
            }

            var user = await _db.Utilizadores.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.Clear();
                ModelState.AddModelError("", "Email ou password incorretos.");
                TempData["ShowRegister"] = null;
                return View(new LoginViewModel());
            }

            string hashed = HashPassword(model.Password);
            if (user.Senha != hashed)
            {
                ModelState.Clear();
                ModelState.AddModelError("", "Email ou password incorretos.");
                TempData["ShowRegister"] = null;
                return View(new LoginViewModel());
            }

            // Sessão
            HttpContext.Session.SetInt32("UserId", user.Id_User);
            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("UserInitial", user.UserName.Substring(0, 1).ToUpper());

            // Cookie Remember Me
            if (model.RememberMe)
            {
                CookieOptions options = new()
                {
                    Expires = DateTime.Now.AddDays(7),
                    HttpOnly = true
                };
                Response.Cookies.Append("AutoMatch_UserId", user.Id_User.ToString(), options);
            }

            return RedirectToAction("Index", "Home");
        }



        // ===== POST REGISTER =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Força a view a abrir o painel de registro
            TempData["ShowRegister"] = "true";

            // Limpa quaisquer erros do login
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(model.FullName) ||
                string.IsNullOrWhiteSpace(model.UserName) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Password) ||
                string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                TempData["RegisterError"] = "Preencha todos os campos.";
                return View("Login");
            }

            if (await _db.Utilizadores.AnyAsync(u => u.Email == model.Email))
            {
                TempData["RegisterError"] = "Este email já está em uso.";
                return View("Login");
            }

            if (await _db.Utilizadores.AnyAsync(u => u.UserName == model.UserName))
            {
                TempData["RegisterError"] = "Este nome de utilizador já está em uso.";
                return View("Login");
            }

            if (model.Password.Length < 6 || model.Password.Length > 12)
            {
                TempData["RegisterError"] = "A password deve ter entre 6 e 12 caracteres.";
                return View("Login");
            }

            if (model.Password != model.ConfirmPassword)
            {
                TempData["RegisterError"] = "As passwords não coincidem.";
                return View("Login");
            }

            // Cria novo utilizador
            string hashed = HashPassword(model.Password);

            var user = new Utilizador
            {
                Nome = model.FullName,
                UserName = model.UserName,
                Email = model.Email,
                Senha = hashed,
                Estado = true
            };

            _db.Utilizadores.Add(user);
            await _db.SaveChangesAsync();

            var comprador = new Comprador
            {
                Id_User = user.Id_User,
                Contactos = "N/A",
                Rua = "Desconhecida",
                Codigo_Postal = "0000-000"
            };
            _db.Compradores.Add(comprador);
            await _db.SaveChangesAsync();

            // Sessão
            HttpContext.Session.SetInt32("UserId", user.Id_User);
            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("UserInitial", user.UserName.Substring(0, 1).ToUpper());

            return RedirectToAction("Index", "Home");
        }

        // ===== LOGOUT =====
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("AutoMatch_UserId");
            return RedirectToAction("Index", "Home");
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var hashed = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashed);
        }

        // PROFILE PAGE
        public IActionResult Profile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _db.Utilizadores.FirstOrDefault(u => u.Id_User == userId);
            if (user == null) return RedirectToAction("Login");

            var comprador = _db.Compradores.FirstOrDefault(c => c.Id_User == user.Id_User);
            bool isSeller = _db.Vendedores.Any(v => v.Id_User == userId);
            bool isBuyer = comprador != null;

            ViewBag.PostalList = _db.CodigoPostais.OrderBy(c => c.Localidade).ToList();

            var vm = new ProfileViewModel
            {
                Id_User = user.Id_User,
                UserName = user.UserName,
                FullName = user.Nome,
                Email = user.Email,
                ProfileImageUrl = !string.IsNullOrEmpty(user.ProfileImageUrl)
                    ? user.ProfileImageUrl
                    : $"https://ui-avatars.com/api/?name={user.UserName}&background=111&color=fff",
                IsSeller = isSeller,
                IsBuyer = isBuyer,

                // From comprador table
                Address = comprador?.Rua ?? "Not defined",
                PostalCode = comprador?.Codigo_Postal ?? "0000-000",
                Phone = comprador?.Contactos ?? "Not defined",

                //Orders = _db.Encomendas
                //    .Where(e => e.Id_User == user.Id_User)
                //    .Select(e => new CarOrderViewModel
                //    {
                //        Name = e.Titulo,
                //        ImageUrl = e.Imagem,
                //        Date = e.Data.ToString("dd/MM/yyyy"),
                //        Status = e.Estado
                //    })
                //    .ToList(),

                //Listings = _db.Listings
                //    .Where(a => a.Id_User == user.Id_User)
                //    .Select(a => new CarListingViewModel
                //    {
                //        Name = a.Titulo,
                //        ImageUrl = a.Imagem,
                //        CreatedAt = a.DataCriacao.ToString("dd/MM/yyyy"),
                //        State = a.Estado
                //    })
                //    .ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = await _db.Utilizadores.FirstOrDefaultAsync(u => u.Id_User == userId);
            if (user == null)
                return RedirectToAction("Login");

            // Apagar comprador associado
            var comprador = await _db.Compradores.FirstOrDefaultAsync(c => c.Id_User == userId);
            if (comprador != null)
                _db.Compradores.Remove(comprador);

            // Apagar vendedor se existir
            var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.Id_User == userId);
            if (vendedor != null)
                _db.Vendedores.Remove(vendedor);

            // Apagar conta do utilizador
            _db.Utilizadores.Remove(user);
            await _db.SaveChangesAsync();

            // Limpa sessão e cookies
            HttpContext.Session.Clear();
            Response.Cookies.Delete("AutoMatch_UserId");

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = await _db.Utilizadores.FirstOrDefaultAsync(u => u.Id_User == userId);
            var comprador = await _db.Compradores.FirstOrDefaultAsync(c => c.Id_User == userId);

            if (user == null) return RedirectToAction("Login");


            // -------- VALIDAR USERNAME --------
            if (!string.IsNullOrWhiteSpace(model.UserName) && model.UserName != user.UserName)
            {
                bool usernameExists = await _db.Utilizadores
                    .AnyAsync(u => u.UserName == model.UserName);

                if (usernameExists)
                {
                    TempData["EditError"] = "Este nome de utilizador já está em uso.";
                    return RedirectToAction("Profile");
                }

                user.UserName = model.UserName;
            }


            // -------- VALIDAR PASSWORD --------
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (model.Password.Length < 6 || model.Password.Length > 12)
                {
                    TempData["EditError"] = "A password deve ter entre 6 e 12 caracteres.";
                    return RedirectToAction("Profile");
                }

                user.Senha = HashPassword(model.Password);
            }


            // -------- ATUALIZAR COMPRADOR --------
            if (comprador != null)
            {
                comprador.Contactos = model.Phone ?? comprador.Contactos;
                comprador.Rua = model.SelectedLocalidade;
                comprador.Codigo_Postal = model.SelectedCodigoPostal;
            }

            // -------- VALIDAR TELEFONE --------
            if (!string.IsNullOrWhiteSpace(model.Phone))
            {
                if (model.Phone.Length != 9 || !model.Phone.All(char.IsDigit))
                {
                    TempData["EditError"] = "O número de telefone deve conter exatamente 9 dígitos.";
                    return RedirectToAction("Profile");
                }

                comprador.Contactos = model.Phone;
            }


            await _db.SaveChangesAsync();

            TempData["EditSuccess"] = "Perfil atualizado com sucesso.";
            return RedirectToAction("Profile");
        }

    }
}
