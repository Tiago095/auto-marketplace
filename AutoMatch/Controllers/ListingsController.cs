using AutoMatch.Data;
using AutoMatch.Models;
using AutoMatch.Models.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace AutoMatch.Controllers
{
    public class ListingsController : Controller
    {
        private readonly AutoMatchContext _db;
        private readonly IWebHostEnvironment _env;

        // Preenche ViewBags para Brand / Model / Type na Create/Edit.
        private void PopulateBrandModelTypeDropdowns()
        {
            var modelos = _db.Modelos.ToList();

            foreach (var extra in ExtraModelos)
            {
                if (!modelos.Any(m => m.Marca == extra.Marca && m.NomeModelo == extra.NomeModelo))
                {
                    modelos.Add(extra);
                }
            }

            ViewBag.Marcas = modelos.Select(m => m.Marca).Distinct().OrderBy(m => m).ToList();
            ViewBag.TodosModelos = modelos;
            ViewBag.Tipos = modelos.Select(m => m.Categoria).Distinct().OrderBy(c => c).ToList();
        }

        // Marcas/modelos extra para aparecerem sempre nos dropdowns da Create/Edit,
        // mesmo que ainda não existam na base de dados.
        private static readonly List<Modelo> ExtraModelos = new()
        {
            new Modelo { Marca = "Tesla",   NomeModelo = "Model 3",      Transmissao = true,  Combustivel = "Electric", Categoria = "Sedan" },
            new Modelo { Marca = "Tesla",   NomeModelo = "Model Y",      Transmissao = true,  Combustivel = "Electric", Categoria = "SUV" },
            new Modelo { Marca = "BMW",     NomeModelo = "3 Series",     Transmissao = true,  Combustivel = "Gasoline", Categoria = "Sedan" },
            new Modelo { Marca = "BMW",     NomeModelo = "X5",           Transmissao = true,  Combustivel = "Diesel",   Categoria = "SUV" },
            new Modelo { Marca = "Mercedes",NomeModelo = "C-Class",      Transmissao = true,  Combustivel = "Gasoline", Categoria = "Sedan" },
            new Modelo { Marca = "Mercedes",NomeModelo = "GLA",          Transmissao = true,  Combustivel = "Gasoline", Categoria = "SUV" },
            new Modelo { Marca = "Audi",    NomeModelo = "A3",           Transmissao = true,  Combustivel = "Gasoline", Categoria = "Hatchback" },
            new Modelo { Marca = "Audi",    NomeModelo = "Q5",           Transmissao = true,  Combustivel = "Diesel",   Categoria = "SUV" },
            new Modelo { Marca = "Volkswagen", NomeModelo = "Golf",      Transmissao = true,  Combustivel = "Gasoline", Categoria = "Hatchback" },
            new Modelo { Marca = "Volkswagen", NomeModelo = "Tiguan",    Transmissao = true,  Combustivel = "Diesel",   Categoria = "SUV" },
            new Modelo { Marca = "Toyota",  NomeModelo = "Corolla",      Transmissao = true,  Combustivel = "Hybrid",   Categoria = "Sedan" },
            new Modelo { Marca = "Toyota",  NomeModelo = "Yaris",        Transmissao = true,  Combustivel = "Hybrid",   Categoria = "Hatchback" },
            new Modelo { Marca = "Honda",   NomeModelo = "Civic",        Transmissao = true,  Combustivel = "Gasoline", Categoria = "Sedan" },
            new Modelo { Marca = "Ford",    NomeModelo = "Focus",        Transmissao = true,  Combustivel = "Gasoline", Categoria = "Hatchback" },
            new Modelo { Marca = "Hyundai", NomeModelo = "Tucson",       Transmissao = true,  Combustivel = "Diesel",   Categoria = "SUV" },
            new Modelo { Marca = "Kia",     NomeModelo = "Sportage",     Transmissao = true,  Combustivel = "Diesel",   Categoria = "SUV" }
        };

        public ListingsController(AutoMatchContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
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
                    .Include(a => a.Imagens)
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

            PopulateBrandModelTypeDropdowns();

            var model = new CreateListingViewModel();
            return View(model);
        }

        // POST: /Listings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateListingViewModel model, List<IFormFile> Photos)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                PopulateBrandModelTypeDropdowns();
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

            // Garante que existe um modelo correspondente à marca e modelo indicados
            var modeloEnt = _db.Modelos.FirstOrDefault(m => m.Marca == model.Marca && m.NomeModelo == model.Modelo);
            if (modeloEnt == null)
            {
                modeloEnt = new Modelo
                {
                    Marca = model.Marca,
                    NomeModelo = model.Modelo,
                    Transmissao = true,
                    Combustivel = "N/A",
                    Categoria = string.IsNullOrWhiteSpace(model.Tipo) ? "Other" : model.Tipo
                };
                _db.Modelos.Add(modeloEnt);
                _db.SaveChanges();
            }
            else if (!string.IsNullOrWhiteSpace(model.Tipo))
            {
                modeloEnt.Categoria = model.Tipo;
            }

            var anuncio = new Anuncio
            {
                Id_Vendedor = vendedor.Id_User,
                Id_Admin = admin.Id_Admin,
                Id_Modelo = modeloEnt.Id_Modelo,
                Titulo = $"{model.Marca} {model.Modelo}",
                Descricao = model.Descricao,
                Ano = new DateTime(model.Year, 1, 1),
                Preco = model.Price,
                Kilometros = model.Kilometros ?? 0,
                Localizacao = model.Localizacao,
                Estado = true,
                Matricula = "0000-000",
                Administrador = admin,
                Modelo = modeloEnt
            };

            _db.Anuncios.Add(anuncio);
            _db.SaveChanges();

            // Guarda imagens associadas ao anúncio (se tiver sido feito upload)
            SavePhotosForListing(anuncio.Id_Anuncio, Photos);

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

            var anuncio = _db.Anuncios
                .Include(a => a.Imagens)
                .FirstOrDefault(a => a.Id_Anuncio == id);
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

            var modeloEnt = _db.Modelos.FirstOrDefault(m => m.Id_Modelo == anuncio.Id_Modelo);

            PopulateBrandModelTypeDropdowns();

            // Foto atual (se existir) para pré-visualização na página de edição
            var firstImage = anuncio.Imagens?.FirstOrDefault();
            ViewBag.CurrentImageFile = firstImage?.CaminhoImagem;

            var model = new CreateListingViewModel
            {
                Id = anuncio.Id_Anuncio,
                Marca = modeloEnt?.Marca ?? string.Empty,
                Modelo = modeloEnt?.NomeModelo ?? string.Empty,
                Tipo = modeloEnt?.Categoria ?? string.Empty,
                Price = anuncio.Preco,
                Year = anuncio.Ano.Year,
                Kilometros = anuncio.Kilometros,
                Localizacao = anuncio.Localizacao,
                Descricao = anuncio.Descricao
            };

            return View(model);
        }

        // POST: /Listings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CreateListingViewModel model, List<IFormFile> Photos)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                PopulateBrandModelTypeDropdowns();
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

            // Atualiza/associa modelo com base na marca e modelo inseridos
            var modeloEnt = _db.Modelos.FirstOrDefault(m => m.Marca == model.Marca && m.NomeModelo == model.Modelo);
            if (modeloEnt == null)
            {
                modeloEnt = new Modelo
                {
                    Marca = model.Marca,
                    NomeModelo = model.Modelo,
                    Transmissao = true,
                    Combustivel = "N/A",
                    Categoria = string.IsNullOrWhiteSpace(model.Tipo) ? "Other" : model.Tipo
                };
                _db.Modelos.Add(modeloEnt);
                _db.SaveChanges();
            }
            else if (!string.IsNullOrWhiteSpace(model.Tipo))
            {
                modeloEnt.Categoria = model.Tipo;
            }

            anuncio.Id_Modelo = modeloEnt.Id_Modelo;

            anuncio.Titulo = $"{model.Marca} {model.Modelo}";
            anuncio.Preco = model.Price;
            anuncio.Ano = new DateTime(model.Year, 1, 1);
            anuncio.Kilometros = model.Kilometros ?? anuncio.Kilometros;
            anuncio.Localizacao = model.Localizacao;
            anuncio.Descricao = model.Descricao;

            _db.SaveChanges();

            // Se forem adicionadas novas imagens em modo de edição, guardamos também.
            SavePhotosForListing(anuncio.Id_Anuncio, Photos);

            return RedirectToAction("MyListings");
        }

        private void SavePhotosForListing(int anuncioId, List<IFormFile> photos)
        {
            if (photos == null || photos.Count == 0)
            {
                return;
            }

            var uploadsRoot = Path.Combine(_env.WebRootPath, "images", "listings");
            Directory.CreateDirectory(uploadsRoot);

            foreach (var photo in photos.Where(p => p != null && p.Length > 0))
            {
                var ext = Path.GetExtension(photo.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsRoot, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    photo.CopyTo(stream);
                }

                // Guardamos apenas o nome do ficheiro na BD (CaminhoImagem tem max 50 chars)
                // e construímos o caminho completo na view.
                var img = new Imagens
                {
                    Id_Anuncio = anuncioId,
                    CaminhoImagem = fileName
                };

                _db.Imagens.Add(img);
            }

            _db.SaveChanges();
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
