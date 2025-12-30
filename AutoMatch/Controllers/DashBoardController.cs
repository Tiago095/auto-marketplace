using AutoMatch.Data;
using AutoMatch.Models;
using AutoMatch.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace AutoMatch.Controllers
{
    public class DashBoardController : Controller
    {
        private readonly AutoMatchContext _context;
        private readonly IWebHostEnvironment _env;

        public DashBoardController(AutoMatchContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName") ?? "Utilizador";

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Quick stats
            var pendingBookings = await _context.Reservas
                .CountAsync(r => r.Id_Comprador == userId && !r.Estado);

            var unreadMessages = await _context.Notificacoes
                .CountAsync(n => n.Id_Comprador == userId && !n.Estado && n.Tipo == "Mensagem");

            var newNotifications = await _context.Notificacoes
                .CountAsync(n => n.Id_Comprador == userId && !n.Estado);

            var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.Id_User == userId);
            var filtersSaved = 0;
            if (comprador != null)
            {
                filtersSaved = await _context.Preferencias
                    .CountAsync(p => p.Id_Comprador == comprador.Id_User);
            }

            // Latest Reserva
            var latestReserva = await _context.Reservas
                .Include(r => r.Anuncio)
                .OrderByDescending(r => r.Data_Inicio)
                .FirstOrDefaultAsync(r => r.Id_Comprador == userId);

            DashboardBookingInfo latestBookingVm = null;
            if (latestReserva != null)
            {
                latestBookingVm = new DashboardBookingInfo
                {
                    ReservaId = latestReserva.Id_Reserva,
                    CarTitle = latestReserva.Anuncio?.Titulo ?? "Reserva",
                    DataInicio = latestReserva.Data_Inicio,
                    DataFim = latestReserva.Data_Fim
                };
            }

            // Recent messages 
            var recentMessages = await _context.Notificacoes
                .Where(n => n.Id_Comprador == userId && n.Tipo == "Mensagem")
                .OrderByDescending(n => n.Data_Envio)
                .Take(2)
                .Select(n => new DashboardMessageInfo
                {
                    NomeRemetente = "Vendedor #" + n.Id_Vendedor,
                    Texto = n.Mensagem,
                    Data = n.Data_Envio
                })
                .ToListAsync();

            // Notifications 
            var notifications = await _context.Notificacoes
                .Where(n => n.Id_Comprador == userId)
                .OrderByDescending(n => n.Data_Envio)
                .Take(5)
                .Select(n => new DashboardNotificationInfo
                {
                    Texto = n.Mensagem,
                    Data = n.Data_Envio
                })
                .ToListAsync();

            var vm = new DashboardViewModel
            {
                UserName = userName,
                PendingBookings = pendingBookings,
                UnreadMessages = unreadMessages,
                NewNotifications = newNotifications,
                FiltersSaved = filtersSaved,
                LatestBooking = latestBookingVm,
                RecentMessages = recentMessages,
                Notifications = notifications
            };

            return View(vm);
        }
        public async Task<IActionResult> Bookings()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName") ?? "Utilizador";

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var comprador = await _context.Compradores
                .Include(c => c.Utilizador)
                .FirstOrDefaultAsync(c => c.Id_User == userId);

            var vm = new BookingsViewModel
            {
                UserName = userName
            };

            if (comprador != null)
            {
                var reservas = await _context.Reservas
                    .Include(r => r.Anuncio)
                    .Where(r => r.Id_Comprador == comprador.Id_User)
                    .OrderByDescending(r => r.Data_Inicio)
                    .ToListAsync();

                foreach (var r in reservas)
                {
                    vm.Bookings.Add(new BookingRowViewModel
                    {
                        ReservaId = r.Id_Reserva,
                        Vehicle = r.Anuncio?.Titulo ?? "(sem título)",
                        Buyer = comprador.Utilizador?.Nome ?? "",
                        Date = r.Data_Inicio,
                        Status = r.Estado ? "Completed" : "Pending"
                    });
                }
            }

            return View(vm);
        }

        // GET: /Dashboard/Messages
        public async Task<IActionResult> Messages(int? vendedorId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName") ?? "Utilizador";

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var vm = new MessagesViewModel
            {
                UserName = userName
            };

            // Buscar todas as notificações de mensagens do comprador
            var notificacoes = await _context.Notificacoes
                .Where(n => n.Id_Comprador == userId && n.Tipo == "Mensagem")
                .OrderByDescending(n => n.Data_Envio)
                .ToListAsync();

            // Agrupar por vendedor
            var grupos = notificacoes
                .GroupBy(n => n.Id_Vendedor)
                .ToList();

            // Buscar informações dos vendedores
            var vendedorIds = grupos.Select(g => g.Key).ToList();
            var vendedores = await _context.Utilizadores
                .Where(u => vendedorIds.Contains(u.Id_User))
                .ToDictionaryAsync(u => u.Id_User, u => u.Nome);

            foreach (var g in grupos)
            {
                var ultima = g.First();
                var nomeVendedor = vendedores.ContainsKey(g.Key) ? vendedores[g.Key] : $"Vendedor #{g.Key}";

                vm.Conversas.Add(new ConversationItemViewModel
                {
                    Id = g.Key,
                    Nome = nomeVendedor,
                    UltimaMensagem = ultima.Mensagem,
                    DataUltima = ultima.Data_Envio,
                    Online = false
                });
            }

            // Se vendedorId foi passado, iniciar conversa com esse vendedor
            if (vendedorId.HasValue)
            {
                // Verificar se já existe conversa com este vendedor
                if (!vm.Conversas.Any(c => c.Id == vendedorId.Value))
                {
                    // Buscar nome do vendedor
                    var vendedor = await _context.Utilizadores.FirstOrDefaultAsync(u => u.Id_User == vendedorId.Value);
                    var nomeVendedor = vendedor?.Nome ?? $"Vendedor #{vendedorId.Value}";

                    // Adicionar conversa nova (vazia por enquanto)
                    vm.Conversas.Insert(0, new ConversationItemViewModel
                    {
                        Id = vendedorId.Value,
                        Nome = nomeVendedor,
                        UltimaMensagem = "Start a conversation...",
                        DataUltima = DateTime.Now,
                        Online = false
                    });
                }

                // Carregar mensagens desta conversa
                var mensagensConversa = await _context.Notificacoes
                    .Where(n => (n.Id_Comprador == userId && n.Id_Vendedor == vendedorId.Value) ||
                               (n.Id_Vendedor == userId && n.Id_Comprador == vendedorId.Value))
                    .OrderBy(n => n.Data_Envio)
                    .ToListAsync();

                foreach (var n in mensagensConversa)
                {
                    vm.Mensagens.Add(new MessageBubbleViewModel
                    {
                        IsOutgoing = n.Id_Comprador == userId, // Se eu enviei
                        Texto = n.Mensagem,
                        Data = n.Data_Envio
                    });
                }

                vm.VendedorAtualId = vendedorId.Value;
            }
            else
            {
                // Carregar mensagens da primeira conversa (comportamento atual)
                var primeira = grupos.FirstOrDefault();
                if (primeira != null)
                {
                    foreach (var n in primeira.OrderBy(n => n.Data_Envio))
                    {
                        vm.Mensagens.Add(new MessageBubbleViewModel
                        {
                            IsOutgoing = n.Estado,
                            Texto = n.Mensagem,
                            Data = n.Data_Envio
                        });
                    }

                    vm.VendedorAtualId = primeira.Key;
                }
            }

            return View(vm);
        }

        // POST: Enviar mensagem
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromQuery] int vendedorId, [FromQuery] string mensagem)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            if (string.IsNullOrWhiteSpace(mensagem))
            {
                return Json(new { success = false, message = "Message cannot be empty" });
            }

            try
            {
                var notificacao = new Notificacoes
                {
                    Id_Comprador = userId.Value,
                    Id_Vendedor = vendedorId,
                    Tipo = "Mensagem",
                    Mensagem = mensagem,
                    Data_Envio = DateTime.Now,
                    Estado = false 
                };

                _context.Notificacoes.Add(notificacao);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        texto = mensagem,
                        data = DateTime.Now.ToString("HH:mm"),
                        isOutgoing = true
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Carregar mensagens de uma conversa específica
        [HttpGet]
        public async Task<IActionResult> GetConversation(int vendedorId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            var mensagens = await _context.Notificacoes
                .Where(n => (n.Id_Comprador == userId && n.Id_Vendedor == vendedorId) ||
                           (n.Id_Vendedor == userId && n.Id_Comprador == vendedorId))
                .OrderBy(n => n.Data_Envio)
                .Select(n => new {
                    texto = n.Mensagem,
                    data = n.Data_Envio.ToString("HH:mm"),
                    isOutgoing = n.Id_Comprador == userId
                })
                .ToListAsync();

            return Json(new { success = true, data = mensagens });
        }

        public async Task<IActionResult> Notifications()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName") ?? "Utilizador";

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var vm = new NotificationsViewModel
            {
                UserName = userName
            };

            var list = await _context.Notificacoes
                .Where(n => n.Id_Comprador == userId)
                .OrderByDescending(n => n.Data_Envio)
                .ToListAsync();

            foreach (var n in list)
            {
                vm.Items.Add(new NotificationItemViewModel
                {
                    Id = n.Id_notificacao,
                    Titulo = n.Tipo,
                    Texto = n.Mensagem,
                    Data = n.Data_Envio,
                    Lida = n.Estado
                });
            }

            return View(vm);
        }

        public async Task<IActionResult> Documents()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName") ?? "Utilizador";

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var vm = new DocumentsViewModel
            {
                UserName = userName
            };

            // Buscar listings do vendedor com seus documentos
            var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.Id_User == userId);
            if (vendedor != null)
            {
                var listings = await _context.Anuncios
                    .Where(a => a.Id_Vendedor == vendedor.Id_User && a.Estado)
                    .ToListAsync();

                var listingIds = listings.Select(l => l.Id_Anuncio).ToList();
                var allDocs = await _context.Documentos
                    .Where(d => listingIds.Contains(d.Id_Anuncio))
                    .ToListAsync();

                foreach (var listing in listings)
                {
                    var listingVm = new ListingWithDocumentsViewModel
                    {
                        Id_Anuncio = listing.Id_Anuncio,
                        Titulo = listing.Titulo
                    };

                    var listingDocs = allDocs.Where(d => d.Id_Anuncio == listing.Id_Anuncio).ToList();
                    foreach (var doc in listingDocs)
                    {
                        listingVm.Documents.Add(new DocumentItemViewModel
                        {
                            Id = doc.Id_Doc,
                            CarTitle = listing.Titulo,
                            Tipo = doc.Tipo,
                            Caminho = doc.CaminhoDocumento,
                            IsListing = true
                        });
                    }

                    vm.Listings.Add(listingVm);
                }
            }

            // Buscar compras do comprador com seus documentos
            var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.Id_User == userId);
            if (comprador != null)
            {
                var compras = await _context.Compras
                    .Include(c => c.Anuncio)
                    .Where(c => c.Id_Comprador == comprador.Id_User && c.Estado)
                    .ToListAsync();

                var anuncioIds = compras.Select(c => c.Id_Anuncio).ToList();
                var purchaseDocs = await _context.Documentos
                    .Where(d => anuncioIds.Contains(d.Id_Anuncio))
                    .ToListAsync();

                foreach (var compra in compras)
                {
                    var purchaseVm = new PurchaseWithDocumentsViewModel
                    {
                        Id_Compra = compra.Id_Compra,
                        CarTitle = compra.Anuncio?.Titulo ?? "Anuncio"
                    };

                    var compraDocs = purchaseDocs.Where(d => d.Id_Anuncio == compra.Id_Anuncio).ToList();
                    foreach (var doc in compraDocs)
                    {
                        purchaseVm.Documents.Add(new DocumentItemViewModel
                        {
                            Id = doc.Id_Doc,
                            CarTitle = compra.Anuncio?.Titulo ?? "Anuncio",
                            Tipo = doc.Tipo,
                            Caminho = doc.CaminhoDocumento,
                            IsListing = false
                        });
                    }

                    vm.Purchases.Add(purchaseVm);
                }
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddDocument(int anuncioId, IFormFile file, string tipo)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            var anuncio = await _context.Anuncios
                .FirstOrDefaultAsync(a => a.Id_Anuncio == anuncioId && a.Id_Vendedor == userId);

            if (anuncio == null)
                return NotFound();

            if (file == null || file.Length == 0)
                return BadRequest("File is required");

            var basePath = Path.Combine(_env.WebRootPath, "Anuncios", $"Anuncio{anuncioId}", "Docs");
            Directory.CreateDirectory(basePath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(basePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var documento = new Documento
            {
                Id_Anuncio = anuncioId,
                Tipo = tipo ?? "Document",
                CaminhoDocumento = $"/Anuncios/Anuncio{anuncioId}/Docs/{fileName}"
            };

            _context.Documentos.Add(documento);
            await _context.SaveChangesAsync();

            return RedirectToAction("Documents");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDocument(int docId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            var documento = await _context.Documentos
                .Include(d => d.Anuncio)
                .FirstOrDefaultAsync(d => d.Id_Doc == docId);

            if (documento == null || documento.Anuncio.Id_Vendedor != userId)
                return NotFound();

            // Delete file
            var filePath = Path.Combine(_env.WebRootPath, documento.CaminhoDocumento.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.Documentos.Remove(documento);
            await _context.SaveChangesAsync();

            return RedirectToAction("Documents");
        }

        public async Task<IActionResult> DownloadDocument(int docId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            var documento = await _context.Documentos
                .Include(d => d.Anuncio)
                .FirstOrDefaultAsync(d => d.Id_Doc == docId);

            if (documento == null)
                return NotFound();

            // Verificar se é documento de compra do utilizador
            var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.Id_User == userId);
            if (comprador != null)
            {
                var compra = await _context.Compras
                    .FirstOrDefaultAsync(c => c.Id_Comprador == comprador.Id_User && c.Id_Anuncio == documento.Id_Anuncio);
                
                if (compra == null)
                    return Unauthorized();
            }
            else
            {
                // Verificar se é documento de listing do utilizador
                if (documento.Anuncio.Id_Vendedor != userId)
                    return Unauthorized();
            }

            var filePath = Path.Combine(_env.WebRootPath, documento.CaminhoDocumento.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(documento.CaminhoDocumento);

            return File(fileBytes, "application/octet-stream", fileName);
        }

        public async Task<IActionResult> Sales(string range = "7d")
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName") ?? "Utilizador";

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var vm = new SalesViewModel
            {
                UserName = userName,
                SelectedRange = range
            };

            var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.Id_User == userId);
            if (vendedor != null)
            {
                var vendasQuery = _context.Compras
                    .Include(c => c.Anuncio)
                    .Include(c => c.Comprador).ThenInclude(b => b.Utilizador)
                    .Where(c => c.Anuncio.Id_Vendedor == vendedor.Id_User);

                var now = DateTime.UtcNow;
                DateTime fromDate = range switch
                {
                    "1m" => now.AddMonths(-1),
                    "1y" => now.AddYears(-1),
                    _ => now.AddDays(-7)
                };

                vendasQuery = vendasQuery.Where(c => c.Data_Compra >= fromDate);

                var vendas = await vendasQuery
                    .OrderByDescending(c => c.Data_Compra)
                    .ToListAsync();

                vm.TotalVendasConcluidas = vendas.Count(c => c.Estado);

                var topGroup = vendas
                    .GroupBy(c => c.Anuncio.Titulo)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                if (topGroup != null)
                {
                    vm.TopModel = topGroup.Key;
                    vm.TopModelUnidades = topGroup.Count();
                }

                foreach (var c in vendas.Take(10))
                {
                    vm.RecentSales.Add(new SaleRowViewModel
                    {
                        Data = c.Data_Compra,
                        Cliente = c.Comprador?.Utilizador?.Nome ?? "",
                        Veiculo = c.Anuncio?.Titulo ?? "",
                        Valor = c.Anuncio?.Preco ?? 0,
                        Estado = c.Estado ? "Payed" : "Pending"
                    });
                }

                var buckets = vendas
                    .GroupBy(c => c.Data_Compra.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => g.Count())
                    .Take(8)
                    .ToList();

                if (buckets.Count == 0)
                {
                    vm.ChartPoints.Add(0);
                }
                else
                {
                    var max = buckets.Max();
                    foreach (var b in buckets)
                    {
                        var value = max == 0 ? 0 : (int)Math.Round((double)b / max * 10);
                        vm.ChartPoints.Add(value);
                    }
                }

                var svgPoints = new List<string>();
                if (vm.ChartPoints.Count > 0)
                {
                    var step = 100.0 / Math.Max(vm.ChartPoints.Count - 1, 1);
                    for (int i = 0; i < vm.ChartPoints.Count; i++)
                    {
                        var x = step * i;
                        var y = 35 - vm.ChartPoints[i] * 3; 
                        svgPoints.Add($"{x},{y}");
                    }
                }
                else
                {
                    svgPoints.Add("0,35");
                    svgPoints.Add("100,35");
                }
                vm.ChartPointsSvg = string.Join(" ", svgPoints);
            }

            return View(vm);
        }
    }
}
