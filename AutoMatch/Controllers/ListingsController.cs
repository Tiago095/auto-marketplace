using AutoMatch.Data;
using AutoMatch.Models;
using AutoMatch.Models.ViewModel;
using AutoMatch.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace AutoMatch.Controllers
{
    public class ListingsController : Controller
    {
        private readonly AutoMatchContext _db;
        private readonly IWebHostEnvironment _env;

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
                return RedirectToAction("Login", "Account");
            }

            var vendedor = _db.Vendedores.FirstOrDefault(v => v.Id_User == userId.Value);

            ViewBag.IsSeller = vendedor != null;

            List<Anuncio> anuncios;

            if (vendedor == null)
            {
                anuncios = new List<Anuncio>();
            }
            else
            {
                var compradosIds = _db.Compras
                    .Where(c => c.Estado == true)
                    .Select(c => c.Id_Anuncio)
                    .ToList();

                anuncios = _db.Anuncios
                    .Include(a => a.Imagens)
                    .Where(a => a.Id_Vendedor == vendedor.Id_User && a.Estado && !compradosIds.Contains(a.Id_Anuncio))
                    .OrderByDescending(a => a.Id_Anuncio)
                    .ToList();
            }

            return View(anuncios);
        }

        // GET: /Listings/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var vendedor = _db.Vendedores.FirstOrDefault(v => v.Id_User == userId);
            if (vendedor == null) return Unauthorized();

            // Marcas únicas com opção vazia no início
            var marcas = _db.Modelos.Select(m => m.Marca).Distinct().ToList();
            var marcasList = new List<string> { "" };
            marcasList.AddRange(marcas);
            ViewBag.Marcas = new SelectList(marcasList);

            // Modelos com todas as informações necessárias
            var modelos = _db.Modelos.Select(m => new {
                m.Id_Modelo,
                m.Marca,
                m.NomeModelo,
                m.Transmissao,
                m.Combustivel,
                m.Categoria
            }).ToList();
            ViewBag.ModelosJson = JsonSerializer.Serialize(modelos);

            return View();
        }

        // POST: /Listings/Create
        // POST: /Listings/Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateListingViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var vendedor = _db.Vendedores.FirstOrDefault(v => v.Id_User == userId);
            if (vendedor == null)
                return Unauthorized();

            // Validação dos campos obrigatórios
            if (string.IsNullOrWhiteSpace(model.Descricao) ||
                string.IsNullOrWhiteSpace(model.Localizacao) ||
                string.IsNullOrWhiteSpace(model.Matricula) ||
                model.Ano <= 0 ||
                model.Preco <= 0 ||
                model.Kilometros < 0 ||
                model.IdModelo <= 0)
            {
                ModelState.AddModelError("", "Fill in all required fields.");
                ReloadViewBag();
                return View(model);
            }

            // Validação do Ano
            int anoAtual = DateTime.Now.Year;
            if (model.Ano < 1990 || model.Ano > anoAtual)
            {
                ModelState.AddModelError("Ano", $"The year must be between 1990 and {anoAtual}.");
                ReloadViewBag();
                return View(model);
            }

            // Validação da Matrícula
            var matriculaSemHifen = model.Matricula.ToUpper().Replace("-", "");
            var matriculaRegex = new System.Text.RegularExpressions.Regex(@"^[A-Z0-9]{2}-[A-Z0-9]{2}-[A-Z0-9]{2}$");
            var allNumbers = new System.Text.RegularExpressions.Regex(@"^[0-9]{6}$").IsMatch(matriculaSemHifen);
            var allLetters = new System.Text.RegularExpressions.Regex(@"^[A-Z]{6}$").IsMatch(matriculaSemHifen);
            
            if (!matriculaRegex.IsMatch(model.Matricula.ToUpper()) || matriculaSemHifen.Length != 6 || allNumbers || allLetters)
            {
                ModelState.AddModelError("Matricula", "The license plate must follow the format: XX-XX-XX (6 characters: letters and numbers). Cannot be all numbers or all letters.");
                ReloadViewBag();
                return View(model);
            }

            model.Matricula = model.Matricula.ToUpper();

            // Validar 5 imagens
            if (model.Imagens == null || model.Imagens.Count != 5)
            {
                ModelState.AddModelError("Imagens", "You must upload exactly 5 images.");
                ReloadViewBag();
                return View(model);
            }

            // Validar ficheiros de imagem
            foreach (var img in model.Imagens)
            {
                if (img.Length == 0)
                {
                    ModelState.AddModelError("Imagens", "One or more images are empty.");
                    ReloadViewBag();
                    return View(model);
                }
            }

            var modeloInfo = _db.Modelos.FirstOrDefault(m => m.Id_Modelo == model.IdModelo);
            if (modeloInfo == null)
            {
                ModelState.AddModelError("", "Invalid model selected.");
                ReloadViewBag();
                return View(model);
            }

            string tituloAuto = $"{modeloInfo.Marca} {modeloInfo.NomeModelo}";

            // Criar o anúncio
            var anuncio = new Anuncio
            {
                Id_Modelo = model.IdModelo,
                Titulo = tituloAuto,
                Descricao = model.Descricao,
                Ano = new DateTime(model.Ano, 1, 1),
                Preco = model.Preco,
                Kilometros = model.Kilometros,
                Localizacao = model.Localizacao,
                Matricula = model.Matricula,
                Estado = true,
                Id_Vendedor = vendedor.Id_User,
                Id_Admin = 4
            };

            _db.Anuncios.Add(anuncio);

            try
            {
                _db.SaveChanges();

                // Criar estrutura de pastas
                string basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Anuncios", $"Anuncio{anuncio.Id_Anuncio}");
                string imgPath = Path.Combine(basePath, "Imagens");
                string docsPath = Path.Combine(basePath, "Docs");

                // Criar diretórios
                Directory.CreateDirectory(imgPath);
                Directory.CreateDirectory(docsPath);

                // Salvar as 5 imagens
                int ordem = 1;
                foreach (var img in model.Imagens)
                {
                    string fileName = $"{ordem}_{Guid.NewGuid()}{Path.GetExtension(img.FileName)}";
                    string fullPath = Path.Combine(imgPath, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await img.CopyToAsync(stream);
                    }

                    _db.Imagens.Add(new Imagens
                    {
                        Id_Anuncio = anuncio.Id_Anuncio,
                        CaminhoImagem = $"/Anuncios/Anuncio{anuncio.Id_Anuncio}/Imagens/{fileName}"
                    });

                    ordem++;
                }

                // Salvar documentos opcionais
                if (model.Documentos != null && model.Documentos.Count > 0)
                {
                    foreach (var doc in model.Documentos)
                    {
                        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(doc.FileName)}";
                        string fullPath = Path.Combine(docsPath, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await doc.CopyToAsync(stream);
                        }

                        string tipo = Path.GetExtension(doc.FileName).ToLower() switch
                        {
                            ".pdf" => "PDF",
                            ".png" => "Imagem",
                            ".jpg" => "Imagem",
                            ".jpeg" => "Imagem",
                            _ => "Documento"
                        };

                        _db.Documentos.Add(new Documento
                        {
                            Id_Anuncio = anuncio.Id_Anuncio,
                            Tipo = tipo,
                            CaminhoDocumento = $"/Anuncios/Anuncio{anuncio.Id_Anuncio}/Docs/{fileName}"
                        });
                    }
                }

                _db.SaveChanges();

                TempData["Success"] = "Listing created successfully!";
                return RedirectToAction("MyListings");
            }
            catch (Exception ex)
            {
                _db.Anuncios.Remove(anuncio);
                _db.SaveChanges();

                ModelState.AddModelError("", $"Error saving files: {ex.Message}");
                ReloadViewBag();
                return View(model);
            }
        }

        // Método auxiliar para recarregar ViewBag
        private void ReloadViewBag()
        {
            var marcas = _db.Modelos.Select(m => m.Marca).Distinct().ToList();
            var marcasList = new List<string> { "" };
            marcasList.AddRange(marcas);
            ViewBag.Marcas = new SelectList(marcasList);

            var modelos = _db.Modelos.Select(m => new {
                m.Id_Modelo,
                m.Marca,
                m.NomeModelo,
                m.Transmissao,
                m.Combustivel,
                m.Categoria
            }).ToList();
            ViewBag.ModelosJson = JsonSerializer.Serialize(modelos);
        }

        // GET: /Listings/Edit
        public IActionResult Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var anuncio = _db.Anuncios
                .Include(a => a.Modelo)
                .Include(a => a.Imagens)
                .FirstOrDefault(a => a.Id_Anuncio == id && a.Id_Vendedor == userId);

            if (anuncio == null)
                return NotFound();

            var imagens = anuncio.Imagens.ToList();
            var imagensOrdenadas = new List<string>();
            for (int i = 1; i <= 5; i++)
            {
                var img = imagens.FirstOrDefault(x => x.CaminhoImagem.Contains($"/{i}_"));
                imagensOrdenadas.Add(img?.CaminhoImagem ?? "");
            }

            var model = new EditListingViewModel
            {
                Id_Anuncio = anuncio.Id_Anuncio,

                Marca = anuncio.Modelo?.Marca ?? "",
                NomeModelo = anuncio.Modelo?.NomeModelo ?? "",
                Transmissao = anuncio.Modelo?.Transmissao ?? false,
                Combustivel = anuncio.Modelo?.Combustivel ?? "N/A",
                Categoria = anuncio.Modelo?.Categoria ?? "",

                Matricula = anuncio.Matricula,
                Ano = anuncio.Ano.Year,

                // Campos editáveis
                Preco = anuncio.Preco,
                Kilometros = anuncio.Kilometros,
                Descricao = anuncio.Descricao,
                Localizacao = anuncio.Localizacao,

                ImagensExistentes = imagensOrdenadas
            };

            return View(model);
        }

        // POST: /Listings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditListingViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var anuncio = _db.Anuncios
                .Include(a => a.Imagens)
                .Include(a => a.Modelo)
                .FirstOrDefault(a => a.Id_Anuncio == model.Id_Anuncio && a.Id_Vendedor == userId);

            if (anuncio == null)
                return NotFound();

            // Validações
            if (model.Preco <= 0 || model.Kilometros < 0 ||
                string.IsNullOrWhiteSpace(model.Descricao) ||
                string.IsNullOrWhiteSpace(model.Localizacao))
            {
                ModelState.AddModelError("", "Preencha todos os campos obrigatórios.");
                RecarregarDadosView(model, anuncio);
                return View(model);
            }


            // Atualizar campos editáveis
            anuncio.Preco = model.Preco;
            anuncio.Kilometros = model.Kilometros;
            anuncio.Descricao = model.Descricao;
            anuncio.Localizacao = model.Localizacao;

            try
            {
                string imgPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "Anuncios",
                    $"Anuncio{anuncio.Id_Anuncio}",
                    "Imagens");

                if (!Directory.Exists(imgPath))
                    Directory.CreateDirectory(imgPath);

                var imagensExistentesDict = new Dictionary<int, Imagens>();
                foreach (var img in anuncio.Imagens)
                {
                    var fileName = Path.GetFileName(img.CaminhoImagem);
                    var partes = fileName.Split('_');
                    if (partes.Length > 0 && int.TryParse(partes[0], out int pos))
                    {
                        imagensExistentesDict[pos] = img;
                    }
                }

                // Processar imagens deletadas
                if (model.ImagensParaDeletar != null)
                {
                    for (int i = 0; i < model.ImagensParaDeletar.Count && i < 5; i++)
                    {
                        if (model.ImagensParaDeletar[i] == "true")
                        {
                            int posicao = i + 1;
                            if (imagensExistentesDict.ContainsKey(posicao))
                            {
                                var imagemParaDeletar = imagensExistentesDict[posicao];
                                string oldPath = Path.Combine(
                                    Directory.GetCurrentDirectory(),
                                    "wwwroot",
                                    imagemParaDeletar.CaminhoImagem.TrimStart('/'));

                                if (System.IO.File.Exists(oldPath))
                                    System.IO.File.Delete(oldPath);

                                _db.Imagens.Remove(imagemParaDeletar);
                                imagensExistentesDict.Remove(posicao);
                            }
                        }
                    }
                }

                // Processar novas imagens
                for (int i = 0; i < 5; i++)
                {
                    int posicao = i + 1;
                    IFormFile novaImagem = null;
                    
                    // Tentar obter o ficheiro pelo nome indexado
                    string indexedName = $"NovasImagens[{i}]";
                    var filesWithIndexedName = Request.Form.Files.GetFiles(indexedName);
                    if (filesWithIndexedName != null && filesWithIndexedName.Count > 0)
                    {
                        var file = filesWithIndexedName.FirstOrDefault();
                        if (file != null && file.Length > 0)
                        {
                            novaImagem = file;
                        }
                    }
                    
                    if (novaImagem == null)
                    {
                        var allFiles = Request.Form.Files.GetFiles("NovasImagens");
                        var nonIndexedFiles = allFiles.Where(f => f.Name == "NovasImagens" || !f.Name.StartsWith("NovasImagens[")).ToList();
                        if (nonIndexedFiles != null && i < nonIndexedFiles.Count)
                        {
                            var file = nonIndexedFiles[i];
                            if (file != null && file.Length > 0)
                            {
                                novaImagem = file;
                            }
                        }
                    }

                    // Verificar se há flag de delete para esta posição
                    bool deveDeletar = false;
                    if (model.ImagensParaDeletar != null && i < model.ImagensParaDeletar.Count)
                    {
                        deveDeletar = model.ImagensParaDeletar[i] == "true";
                    }

                    if (novaImagem != null && novaImagem.Length > 0)
                    {
                        if (imagensExistentesDict.ContainsKey(posicao))
                        {
                            var imagemAntiga = imagensExistentesDict[posicao];
                            string oldPath = Path.Combine(
                                Directory.GetCurrentDirectory(),
                                "wwwroot",
                                imagemAntiga.CaminhoImagem.TrimStart('/'));

                            if (System.IO.File.Exists(oldPath))
                                System.IO.File.Delete(oldPath);

                            _db.Imagens.Remove(imagemAntiga);
                            imagensExistentesDict.Remove(posicao);
                        }

                        // Salvar nova imagem
                        string fileName = $"{posicao}_{Guid.NewGuid()}{Path.GetExtension(novaImagem.FileName)}";
                        string fullPath = Path.Combine(imgPath, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await novaImagem.CopyToAsync(stream);
                        }

                        _db.Imagens.Add(new Imagens
                        {
                            Id_Anuncio = anuncio.Id_Anuncio,
                            CaminhoImagem = $"/Anuncios/Anuncio{anuncio.Id_Anuncio}/Imagens/{fileName}"
                        });
                    }
                    else if (deveDeletar)
                    {
                        if (imagensExistentesDict.ContainsKey(posicao))
                        {
                            var imagemParaDeletar = imagensExistentesDict[posicao];
                            string oldPath = Path.Combine(
                                Directory.GetCurrentDirectory(),
                                "wwwroot",
                                imagemParaDeletar.CaminhoImagem.TrimStart('/'));

                            if (System.IO.File.Exists(oldPath))
                                System.IO.File.Delete(oldPath);

                            _db.Imagens.Remove(imagemParaDeletar);
                            imagensExistentesDict.Remove(posicao);
                        }
                    }
                }

                _db.Entry(anuncio).Collection(a => a.Imagens).Load();
                var imagensFinais = anuncio.Imagens.Count;
                
                if (imagensFinais < 5)
                {
                    ModelState.AddModelError("", $"You need exactly 5 images to save the listing. Currently you have {imagensFinais} image(s). Please add {5 - imagensFinais} more image(s).");
                    RecarregarDadosView(model, anuncio);
                    return View(model);
                }

                _db.SaveChanges();

                TempData["Success"] = "Anúncio atualizado com sucesso!";
                return RedirectToAction("MyListings");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erro ao atualizar: {ex.Message}");

                // Recarregar em caso de erro
                var anuncioReload = _db.Anuncios
                    .Include(a => a.Modelo)
                    .Include(a => a.Imagens)
                    .FirstOrDefault(a => a.Id_Anuncio == model.Id_Anuncio);

                if (anuncioReload != null)
                    RecarregarDadosView(model, anuncioReload);

                return View(model);
            }
        }

        private void RecarregarDadosView(EditListingViewModel model, Anuncio anuncio)
        {
            model.Marca = anuncio.Modelo?.Marca ?? "";
            model.NomeModelo = anuncio.Modelo?.NomeModelo ?? "";
            model.Transmissao = anuncio.Modelo?.Transmissao ?? false;
            model.Combustivel = anuncio.Modelo?.Combustivel ?? "N/A";
            model.Categoria = anuncio.Modelo?.Categoria ?? "";
            model.Matricula = anuncio.Matricula;
            model.Ano = anuncio.Ano.Year;

            var imagensOrdenadas = new List<string>();
            for (int i = 1; i <= 5; i++)
            {
                var img = anuncio.Imagens.FirstOrDefault(x => x.CaminhoImagem.Contains($"/{i}_"));
                imagensOrdenadas.Add(img?.CaminhoImagem ?? "");
            }
            model.ImagensExistentes = imagensOrdenadas;
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
                anuncio.Estado = false;
                _db.SaveChanges();
            }

            return RedirectToAction("MyListings");
        }
    }
}