using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMatch.Data;
using AutoMatch.Models;
using AutoMatch.Models.ViewModels;
using AutoMatch.Services;
using System.Linq;

namespace AutoMatch.Controllers
{
    public class VehicleController : Controller
    {
        private readonly AutoMatchContext _db;
        private readonly IEmailService _emailService;

        public VehicleController(AutoMatchContext db, IEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public IActionResult Results(string? brand, string? model, int? year, decimal? maxPrice, int? maxMileage, string? fuelType, string? transmission, string? bodyType, string? sort)
        {
            IQueryable<Anuncio> baseQuery = _db.Anuncios
                .Include(a => a.Modelo)
                .Include(a => a.Imagens)
                .Where(a => a.Estado)
                .Where(a => !_db.Compras.Any(c => c.Id_Anuncio == a.Id_Anuncio && c.Estado));

            var availableBrands = baseQuery
                .Select(a => a.Modelo.Marca)
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            var brandModels = baseQuery
                .GroupBy(a => a.Modelo.Marca)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(a => a.Modelo.NomeModelo)
                          .Distinct()
                          .OrderBy(n => n)
                          .ToList()
                );

            IQueryable<Anuncio> query = baseQuery;

            if (!string.IsNullOrEmpty(brand))
                query = query.Where(a => a.Modelo.Marca == brand);

            if (!string.IsNullOrEmpty(model))
                query = query.Where(a => a.Modelo.NomeModelo == model);

            if (year.HasValue)
                query = query.Where(a => a.Ano.Year == year.Value);

            if (maxPrice.HasValue)
                query = query.Where(a => a.Preco <= maxPrice.Value);

            if (maxMileage.HasValue)
                query = query.Where(a => a.Kilometros <= maxMileage.Value);

            if (!string.IsNullOrEmpty(fuelType))
                query = query.Where(a => a.Modelo.Combustivel == fuelType);

            if (!string.IsNullOrEmpty(transmission))
            {
                bool isAutomatic = transmission.Equals("Automatic", StringComparison.OrdinalIgnoreCase);
                query = query.Where(a => a.Modelo.Transmissao == isAutomatic);
            }

            if (!string.IsNullOrEmpty(bodyType))
                query = query.Where(a => a.Modelo.Categoria == bodyType);

            query = sort switch
            {
                "price-low-high" => query.OrderBy(a => a.Preco),
                "price-high-low" => query.OrderByDescending(a => a.Preco),
                "year-new-old" => query.OrderByDescending(a => a.Ano),
                "year-old-new" => query.OrderBy(a => a.Ano),
                "mileage-low-high" => query.OrderBy(a => a.Kilometros),
                _ => query.OrderBy(a => a.Preco)
            };

            var anunciosLista = query
                .Include(a => a.Imagens)
                .ToList();

            var vehicles = anunciosLista
                .Select(a => new Vehicle
                {
                    Id = a.Id_Anuncio,
                    Brand = a.Modelo.Marca,
                    Model = a.Modelo.NomeModelo,
                    Year = a.Ano.Year,
                    Price = a.Preco,
                    Mileage = a.Kilometros,
                    FuelType = a.Modelo.Combustivel,
                    Transmission = a.Modelo.Transmissao ? "Automatic" : "Manual",
                    BodyType = a.Modelo.Categoria,
      
                    ImageUrl = GetCoverImagePath(a.Imagens),
                    Description = a.Descricao,
                    SellerId = a.Id_Vendedor
                })
                .ToList();

            var viewModel = new VehicleResultsViewModel
            {
                Vehicles = vehicles,
                AvailableBrands = availableBrands,
                BrandModels = brandModels,
                SelectedBrand = brand,
                SelectedModel = model,
                SelectedYear = year,
                SelectedMaxPrice = maxPrice,
                SelectedMaxMileage = maxMileage,
                SelectedFuelType = fuelType,
                SelectedTransmission = transmission,
                SelectedBodyType = bodyType,
                SelectedSort = sort
            };

            return View(viewModel);
        }

        public IActionResult Details(int id)
        {
            var anuncio = _db.Anuncios
                .Include(a => a.Modelo)
                .Include(a => a.Imagens)
                .FirstOrDefault(a => a.Id_Anuncio == id && a.Estado);

            if (anuncio == null)
            {
                return NotFound();
            }

            var orderedImages = (anuncio.Imagens ?? new List<Imagens>())
                .Where(i => !string.IsNullOrEmpty(i.CaminhoImagem))
                .OrderBy(i =>
                {
                    try
                    {
                        var nomeArquivo = System.IO.Path.GetFileName(i.CaminhoImagem);
                        var numeroParte = nomeArquivo.Split('_')[0];
                        return int.Parse(numeroParte);
                    }
                    catch
                    {
                        return 999;
                    }
                })
                .Select(i => i.CaminhoImagem)
                .ToList();

            ViewBag.OrderedImages = orderedImages;

            var vehicle = new Vehicle
            {
                Id = anuncio.Id_Anuncio,
                Brand = anuncio.Modelo.Marca,
                Model = anuncio.Modelo.NomeModelo,
                Year = anuncio.Ano.Year,
                Price = anuncio.Preco,
                Mileage = anuncio.Kilometros,
                FuelType = anuncio.Modelo.Combustivel,
                Transmission = anuncio.Modelo.Transmissao ? "Automatic" : "Manual",
                BodyType = anuncio.Modelo.Categoria,
                ImageUrl = GetCoverImagePath(anuncio.Imagens?.ToList() ?? new List<Imagens>()),
                Description = anuncio.Descricao,
                SellerId = anuncio.Id_Vendedor
            };

            return View(vehicle);
        }

        public IActionResult Buy(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var anuncio = _db.Anuncios
                .Include(a => a.Modelo)
                .Include(a => a.Imagens)
                .FirstOrDefault(a => a.Id_Anuncio == id && a.Estado);

            if (anuncio == null)
            {
                return NotFound();
            }

            var vehicle = new Vehicle
            {
                Id = anuncio.Id_Anuncio,
                Brand = anuncio.Modelo.Marca,
                Model = anuncio.Modelo.NomeModelo,
                Year = anuncio.Ano.Year,
                Price = anuncio.Preco,
                Mileage = anuncio.Kilometros,
                FuelType = anuncio.Modelo.Combustivel,
                Transmission = anuncio.Modelo.Transmissao ? "Automatic" : "Manual",
                BodyType = anuncio.Modelo.Categoria,
                ImageUrl = GetCoverImagePath(anuncio.Imagens?.ToList() ?? new List<Imagens>()),
                Description = anuncio.Descricao,
                SellerId = anuncio.Id_Vendedor
            };

            var tax = Math.Round(vehicle.Price * 0.07m, 2);
            var total = vehicle.Price + tax;

            ViewBag.Tax = tax;
            ViewBag.Total = total;

            return View(vehicle);
        }

        [HttpPost]
        public IActionResult SimulateCheckout(int vehicleId, int months)
        {
            var anuncio = _db.Anuncios
                .Include(a => a.Modelo)
                .FirstOrDefault(a => a.Id_Anuncio == vehicleId && a.Estado);

            if (anuncio == null)
            {
                return NotFound();
            }

            decimal basePrice = anuncio.Preco;
            decimal tax = Math.Round(basePrice * 0.07m, 2);
            decimal deliveryFee = 250m;
            decimal insurance = 300m;
            decimal totalEstimate = basePrice + tax + deliveryFee + insurance;

            decimal monthly = months > 0 ? Math.Round(totalEstimate / months, 2) : totalEstimate;

            return Json(new
            {
                basePrice = basePrice,
                tax,
                deliveryFee,
                insurance,
                totalEstimate,
                months,
                monthly
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPurchase(int vehicleId, string FullName, string Address, string City, string Country)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var anuncio = _db.Anuncios
                .Include(a => a.Modelo)
                .FirstOrDefault(a => a.Id_Anuncio == vehicleId && a.Estado);

            if (anuncio == null)
            {
                return NotFound();
            }

            var user = _db.Utilizadores.FirstOrDefault(u => u.Id_User == userId.Value);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var compra = new Compra
            {
                Id_Anuncio = anuncio.Id_Anuncio,
                Id_Comprador = user.Id_User,
                Estado = true
            };

            _db.Compras.Add(compra);

            anuncio.Estado = false;

            _db.SaveChanges();

            var subject = "AutoMatch - Purchase Confirmation";
            var body = $"Hello {FullName},\n\n" +
                       $"Thank you for your purchase on AutoMatch.\n" +
                       $"Vehicle: {anuncio.Modelo.Marca} {anuncio.Modelo.NomeModelo} ({anuncio.Ano.Year})\n" +
                       $"Price: {anuncio.Preco:N0}€\n" +
                       $"Billing address: {Address}, {City}, {Country}\n\n" +
                       "Best regards,\nAutoMatch";

            await _emailService.SendPurchaseConfirmationAsync(user.Email, subject, body);

            TempData["Success"] = "Purchase completed successfully. A confirmation email was sent.";
            return RedirectToAction("Results");
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            if (request == null)
            {
                return Json(new { success = false, message = "Dados inválidos" });
            }

            var anuncio = await _db.Anuncios
                .FirstOrDefaultAsync(a => a.Id_Anuncio == request.anuncioId && a.Estado);

            if (anuncio == null)
            {
                return Json(new { success = false, message = "Anúncio não encontrado" });
            }

            var reservaExistente = await _db.Reservas
                .AnyAsync(r => r.Id_Anuncio == request.anuncioId &&
                              r.Data_Inicio <= request.dataFim &&
                              r.Data_Fim >= request.dataInicio &&
                              (r.Estado == true || r.Estado == false));

            if (reservaExistente)
            {
                return Json(new { success = false, message = "Este horário já está reservado" });
            }

            var comprador = await _db.Compradores
                .FirstOrDefaultAsync(c => c.Id_User == userId);

            if (comprador == null)
            {
                comprador = new Comprador
                {
                    Id_User = userId.Value,
                    Contactos = "N/A",
                    Rua = "Desconhecida",
                    Codigo_Postal = "0000-000"
                };
                _db.Compradores.Add(comprador);
                await _db.SaveChangesAsync();
            }

            var reserva = new Reserva
            {
                Id_Anuncio = request.anuncioId,
                Id_Comprador = comprador.Id_User,
                Data_Inicio = request.dataInicio,
                Data_Fim = request.dataFim,
                Estado = false
            };

            _db.Reservas.Add(reserva);
            await _db.SaveChangesAsync();

            var compradorInfo = await _db.Utilizadores
                .FirstOrDefaultAsync(u => u.Id_User == comprador.Id_User);

            var compradorNome = compradorInfo?.Nome ?? compradorInfo?.UserName ?? "Um comprador";

            var vendedorId = anuncio.Id_Vendedor;
            
            var vendedorExiste = await _db.Vendedores.AnyAsync(v => v.Id_User == vendedorId);
            if (!vendedorExiste)
            {
                var codigoPostalExiste = await _db.CodigoPostais.AnyAsync(cp => cp.Codigo_Postal == "0000-000");
                if (!codigoPostalExiste)
                {
                    var novoCodigoPostal = new CodigoPostal
                    {
                        Codigo_Postal = "0000-000",
                        Localidade = "Desconhecida"
                    };
                    _db.CodigoPostais.Add(novoCodigoPostal);
                    await _db.SaveChangesAsync();
                }

                var novoVendedor = new Vendedor
                {
                    Id_User = vendedorId,
                    Tipo = false,
                    Contactos = "N/A",
                    Rua = "Desconhecida",
                    Codigo_Postal = "0000-000"
                };
                _db.Vendedores.Add(novoVendedor);
                await _db.SaveChangesAsync();
            }

            var notificacao = new Notificacoes
            {
                Id_Comprador = comprador.Id_User,
                Id_Vendedor = vendedorId,
                Tipo = "Booking",
                Mensagem = $"Nova reserva de test drive de {compradorNome} para {anuncio.Titulo} em {request.dataInicio:dd/MM/yyyy} das {request.dataInicio:HH:mm} às {request.dataFim:HH:mm}",
                Data_Envio = DateTime.Now,
                Estado = false
            };

            _db.Notificacoes.Add(notificacao);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Reserva criada com sucesso. Aguardando aprovação do vendedor." });
        }

        [HttpGet]
        public async Task<IActionResult> GetBookedSlots(int anuncioId)
        {
            var reservas = await _db.Reservas
                .Where(r => r.Id_Anuncio == anuncioId &&
                           (r.Estado == true || r.Estado == false))
                .Select(r => new
                {
                    inicio = r.Data_Inicio,
                    fim = r.Data_Fim
                })
                .ToListAsync();

            return Json(reservas);
        }

        private static string GetCoverImagePath(IEnumerable<Imagens> imagens)
        {
            if (imagens == null)
                return string.Empty;

            var ordered = imagens
                .Where(i => !string.IsNullOrEmpty(i.CaminhoImagem))
                .OrderBy(i =>
                {
                    try
                    {
                        var nomeArquivo = System.IO.Path.GetFileName(i.CaminhoImagem);
                        var numeroParte = nomeArquivo.Split('_')[0];
                        return int.Parse(numeroParte);
                    }
                    catch
                    {
                        return 999;
                    }
                })
                .FirstOrDefault();

            return ordered?.CaminhoImagem ?? string.Empty;
        }
    }
}
