using AutoMatch.Data;
using AutoMatch.Models;
using AutoMatch.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace AutoMatch.Controllers
{
    public class ListingsController : Controller
    {
        private readonly AutoMatchContext _db;

        public ListingsController(AutoMatchContext db)
        {
            _db = db;
        }

        // GET: /Listings/MyListings
        public IActionResult MyListings()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                // Se não estiver autenticado, redireciona para o login
                return RedirectToAction("Login", "Account");
            }

            // Encontrar o vendedor associado a este utilizador (se existir)
            var vendedor = _db.Vendedores.FirstOrDefault(v => v.Id_User == userId.Value);

            List<Anuncio> anuncios;

            if (vendedor == null)
            {
                // Utilizador ainda não é vendedor ou não tem anúncios
                anuncios = new List<Anuncio>();
            }
            else
            {
                anuncios = _db.Anuncios
                    .Where(a => a.Id_Vendedor == vendedor.Id_User && a.Estado)
                    .OrderByDescending(a => a.Id_Anuncio)
                    .ToList();
            }

            return View(anuncios);
        }

        // GET: /Listings/Create
        [HttpGet]
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new CreateListingViewModel();
            return View(model);
        }

        // POST: /Listings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateListingViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Garante que existe um vendedor associado a este utilizador
            var vendedor = _db.Vendedores.FirstOrDefault(v => v.Id_User == userId.Value);
            if (vendedor == null)
            {
                vendedor = new Vendedor
                {
                    Id_User = userId.Value,
                    Tipo = true,
                    NIF = null,
                    Contactos = "N/A",
                    Rua = "Desconhecida",
                    Codigo_Postal = "0000-000"
                };
                _db.Vendedores.Add(vendedor);
                _db.SaveChanges();
            }

            // Garante que existe um administrador associado a este utilizador
            var admin = _db.Administradores.FirstOrDefault(a => a.Id_User == userId.Value);
            if (admin == null)
            {
                admin = new Administrador
                {
                    Id_User = userId.Value,
                    Id_Admin = userId.Value // usa o mesmo id só para referência interna
                };
                _db.Administradores.Add(admin);
                _db.SaveChanges();
            }

            // Garante que existe pelo menos um modelo
            var modeloEnt = _db.Modelos.FirstOrDefault();
            if (modeloEnt == null)
            {
                modeloEnt = new Modelo
                {
                    Marca = "Generic",
                    NomeModelo = "Generic",
                    Transmissao = true,
                    Combustivel = "N/A",
                    Categoria = "Other"
                };
                _db.Modelos.Add(modeloEnt);
                _db.SaveChanges();
            }

            var anuncio = new Anuncio
            {
                Id_Vendedor = vendedor.Id_User,
                Id_Admin = admin.Id_Admin,
                Id_Modelo = modeloEnt.Id_Modelo,
                Titulo = model.Title,
                Descricao = model.Descricao,
                Ano = new DateTime(model.Year, 1, 1),
                Preco = model.Price,
                Kilometros = model.Kilometros,
                Localizacao = model.Localizacao,
                Estado = true,
                Matricula = model.Matricula,
                Administrador = admin,
                Modelo = modeloEnt
            };

            _db.Anuncios.Add(anuncio);
            _db.SaveChanges();

            return RedirectToAction("MyListings");
        }

        // GET: /Listings/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var anuncio = _db.Anuncios.FirstOrDefault(a => a.Id_Anuncio == id);
            if (anuncio == null)
            {
                return RedirectToAction("MyListings");
            }

            // Garante que só o vendedor dono do anúncio pode editar
            var vendedor = _db.Vendedores.FirstOrDefault(v => v.Id_User == userId.Value);
            if (vendedor == null || anuncio.Id_Vendedor != vendedor.Id_User)
            {
                return RedirectToAction("MyListings");
            }

            var model = new CreateListingViewModel
            {
                Id = anuncio.Id_Anuncio,
                Title = anuncio.Titulo,
                Price = anuncio.Preco,
                Year = anuncio.Ano.Year,
                Kilometros = anuncio.Kilometros,
                Matricula = anuncio.Matricula,
                Localizacao = anuncio.Localizacao,
                Descricao = anuncio.Descricao
            };

            return View(model);
        }

        // POST: /Listings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CreateListingViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var vendedor = _db.Vendedores.FirstOrDefault(v => v.Id_User == userId.Value);
            if (vendedor == null)
            {
                return RedirectToAction("MyListings");
            }

            var anuncio = _db.Anuncios.FirstOrDefault(a => a.Id_Anuncio == id && a.Id_Vendedor == vendedor.Id_User);
            if (anuncio == null)
            {
                return RedirectToAction("MyListings");
            }

            anuncio.Titulo = model.Title;
            anuncio.Preco = model.Price;
            anuncio.Ano = new DateTime(model.Year, 1, 1);
            anuncio.Kilometros = model.Kilometros;
            anuncio.Matricula = model.Matricula;
            anuncio.Localizacao = model.Localizacao;
            anuncio.Descricao = model.Descricao;

            _db.SaveChanges();

            return RedirectToAction("MyListings");
        }

        // POST: /Listings/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var vendedor = _db.Vendedores.FirstOrDefault(v => v.Id_User == userId.Value);
            if (vendedor == null)
            {
                return RedirectToAction("MyListings");
            }

            var anuncio = _db.Anuncios.FirstOrDefault(a => a.Id_Anuncio == id && a.Id_Vendedor == vendedor.Id_User);
            if (anuncio != null)
            {
                // Soft delete: marca como inativo para evitar problemas de FK
                anuncio.Estado = false;
                _db.SaveChanges();
            }

            return RedirectToAction("MyListings");
        }
    }
}
