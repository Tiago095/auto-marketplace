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

        private async Task<int> GetUnreadMessagesCount(int userId)
        {
            // Contar mensagens não lidas: mensagens onde o outro participante enviou (não o usuário atual) e Estado = false
            return await _context.Notificacoes
                .CountAsync(n => n.Tipo == "Mensagem" && 
                                !n.Estado && 
                                ((n.Id_Comprador == userId && n.Id_Vendedor != userId) || 
                                 (n.Id_Vendedor == userId && n.Id_Comprador != userId)));
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName") ?? "Utilizador";

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UnreadMessagesCount = await GetUnreadMessagesCount(userId.Value);

            // Quick stats
            var pendingBookings = await _context.Reservas
                .CountAsync(r => r.Id_Comprador == userId && !r.Estado);

            // Contar mensagens não lidas: mensagens onde o outro participante enviou e Estado = false
            var unreadMessages = await _context.Notificacoes
                .CountAsync(n => n.Tipo == "Mensagem" && 
                                !n.Estado && 
                                ((n.Id_Comprador == userId && n.Id_Vendedor != userId) || 
                                 (n.Id_Vendedor == userId && n.Id_Comprador != userId)));

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

            ViewBag.UnreadMessagesCount = await GetUnreadMessagesCount(userId.Value);

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
        public async Task<IActionResult> Messages(int? vendedorId, int? compradorId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName") ?? "Utilizador";

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UnreadMessagesCount = await GetUnreadMessagesCount(userId.Value);

            // Verificar se o usuário é vendedor ou comprador
            var isVendedor = await _context.Vendedores.AnyAsync(v => v.Id_User == userId);
            var isComprador = await _context.Compradores.AnyAsync(c => c.Id_User == userId);

            var vm = new MessagesViewModel
            {
                UserName = userName
            };

            // Buscar todas as mensagens onde o usuário participa (como comprador OU como vendedor)
            var todasNotificacoes = await _context.Notificacoes
                .Where(n => n.Tipo == "Mensagem" && 
                           ((n.Id_Comprador == userId) || (n.Id_Vendedor == userId)))
                .OrderByDescending(n => n.Data_Envio)
                .ToListAsync();

            // Agrupar conversas: identificar o outro participante de cada conversa
            var conversasDict = new Dictionary<int, (List<Notificacoes> mensagens, DateTime ultimaData)>();
            
            foreach (var n in todasNotificacoes)
            {
                int outroParticipanteId;
                if (n.Id_Comprador == userId)
                {
                    outroParticipanteId = n.Id_Vendedor;
                }
                else
                {
                    outroParticipanteId = n.Id_Comprador;
                }

                if (!conversasDict.ContainsKey(outroParticipanteId))
                {
                    conversasDict[outroParticipanteId] = (new List<Notificacoes>(), n.Data_Envio);
                }
                
                conversasDict[outroParticipanteId].mensagens.Add(n);
                if (n.Data_Envio > conversasDict[outroParticipanteId].ultimaData)
                {
                    conversasDict[outroParticipanteId] = (conversasDict[outroParticipanteId].mensagens, n.Data_Envio);
                }
            }

            // Buscar informações dos outros participantes
            var outrosParticipantesIds = conversasDict.Keys.ToList();
            var outrosParticipantes = await _context.Utilizadores
                .Where(u => outrosParticipantesIds.Contains(u.Id_User))
                .ToDictionaryAsync(u => u.Id_User, u => u.Nome);

            // Contar mensagens não lidas por conversa
            foreach (var kvp in conversasDict)
            {
                var outroId = kvp.Key;
                var mensagens = kvp.Value.mensagens;
                var ultimaMensagem = mensagens.OrderByDescending(m => m.Data_Envio).First();
                var nomeOutro = outrosParticipantes.ContainsKey(outroId) ? outrosParticipantes[outroId] : $"User #{outroId}";
                
                // Contar mensagens não lidas: mensagens enviadas pelo outro participante que ainda não foram lidas (Estado = false)
                var mensagensNaoLidas = mensagens.Count(m => 
                    ((m.Id_Comprador == outroId && m.Id_Vendedor == userId) || 
                     (m.Id_Vendedor == outroId && m.Id_Comprador == userId)) &&
                    !m.Estado);

                vm.Conversas.Add(new ConversationItemViewModel
                {
                    Id = outroId,
                    Nome = nomeOutro,
                    UltimaMensagem = ultimaMensagem.Mensagem.Length > 50 ? ultimaMensagem.Mensagem.Substring(0, 50) + "..." : ultimaMensagem.Mensagem,
                    DataUltima = ultimaMensagem.Data_Envio,
                    Online = false,
                    MensagensNaoLidas = mensagensNaoLidas
                });
            }

            // Ordenar conversas por data da última mensagem
            vm.Conversas = vm.Conversas.OrderByDescending(c => c.DataUltima).ToList();

            // Determinar qual conversa carregar
            int? conversaId = null;
            if (vendedorId.HasValue && vendedorId.Value != userId)
            {
                conversaId = vendedorId.Value;
            }
            else if (compradorId.HasValue && compradorId.Value != userId)
            {
                conversaId = compradorId.Value;
            }
            else if (vm.Conversas.Any())
            {
                conversaId = vm.Conversas.First().Id;
            }

            if (conversaId.HasValue)
            {
                // Verificar se já existe conversa
                if (!vm.Conversas.Any(c => c.Id == conversaId.Value))
                {
                    var outro = await _context.Utilizadores.FirstOrDefaultAsync(u => u.Id_User == conversaId.Value);
                    var nomeOutro = outro?.Nome ?? $"User #{conversaId.Value}";

                    vm.Conversas.Insert(0, new ConversationItemViewModel
                    {
                        Id = conversaId.Value,
                        Nome = nomeOutro,
                        UltimaMensagem = "Start a conversation...",
                        DataUltima = DateTime.Now,
                        Online = false,
                        MensagensNaoLidas = 0
                    });
                }

                // Carregar mensagens desta conversa
                var mensagensConversa = await _context.Notificacoes
                    .Where(n => n.Tipo == "Mensagem" &&
                               ((n.Id_Comprador == userId && n.Id_Vendedor == conversaId.Value) ||
                                (n.Id_Vendedor == userId && n.Id_Comprador == conversaId.Value)))
                    .OrderBy(n => n.Data_Envio)
                    .ToListAsync();

                foreach (var n in mensagensConversa)
                {
                    vm.Mensagens.Add(new MessageBubbleViewModel
                    {
                        IsOutgoing = n.Id_Comprador == userId || (n.Id_Vendedor == userId && n.Id_Comprador != userId),
                        Texto = n.Mensagem,
                        Data = n.Data_Envio
                    });
                }

                vm.VendedorAtualId = conversaId.Value;

                // Marcar mensagens como lidas quando a conversa é aberta
                var mensagensParaMarcar = mensagensConversa
                    .Where(n => ((n.Id_Comprador == conversaId.Value && n.Id_Vendedor == userId) ||
                                (n.Id_Vendedor == conversaId.Value && n.Id_Comprador == userId)) &&
                               !n.Estado)
                    .ToList();

                foreach (var msg in mensagensParaMarcar)
                {
                    msg.Estado = true;
                }

                await _context.SaveChangesAsync();
            }

            return View(vm);
        }

        // POST: Enviar mensagem
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromQuery] int? vendedorId, [FromQuery] int? compradorId, [FromQuery] string mensagem)
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

            // Determinar o outro participante
            int outroParticipanteId;
            bool usuarioEhComprador;

            var isVendedor = await _context.Vendedores.AnyAsync(v => v.Id_User == userId);
            
            if (vendedorId.HasValue && vendedorId.Value != userId)
            {
                outroParticipanteId = vendedorId.Value;
                usuarioEhComprador = true;
            }
            else if (compradorId.HasValue && compradorId.Value != userId)
            {
                outroParticipanteId = compradorId.Value;
                usuarioEhComprador = false;
            }
            else
            {
                return Json(new { success = false, message = "Invalid recipient" });
            }

            try
            {
                var notificacao = new Notificacoes
                {
                    Id_Comprador = usuarioEhComprador ? userId.Value : outroParticipanteId,
                    Id_Vendedor = usuarioEhComprador ? outroParticipanteId : userId.Value,
                    Tipo = "Mensagem",
                    Mensagem = mensagem,
                    Data_Envio = DateTime.Now,
                    Estado = false // Não lida pelo destinatário
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
        public async Task<IActionResult> GetConversation(int? vendedorId, int? compradorId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            int? outroId = vendedorId ?? compradorId;
            if (!outroId.HasValue)
            {
                return Json(new { success = false, message = "Invalid conversation" });
            }

            var mensagens = await _context.Notificacoes
                .Where(n => n.Tipo == "Mensagem" &&
                           ((n.Id_Comprador == userId && n.Id_Vendedor == outroId.Value) ||
                            (n.Id_Vendedor == userId && n.Id_Comprador == outroId.Value)))
                .OrderBy(n => n.Data_Envio)
                .Select(n => new {
                    texto = n.Mensagem,
                    data = n.Data_Envio.ToString("HH:mm"),
                    isOutgoing = (n.Id_Comprador == userId) || (n.Id_Vendedor == userId && n.Id_Comprador != userId)
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

            ViewBag.UnreadMessagesCount = await GetUnreadMessagesCount(userId.Value);

            var vm = new NotificationsViewModel
            {
                UserName = userName
            };

            // Buscar todas as notificações onde o usuário participa (como comprador OU como vendedor)
            var list = await _context.Notificacoes
                .Where(n => (n.Id_Comprador == userId) || (n.Id_Vendedor == userId))
                .OrderByDescending(n => n.Data_Envio)
                .ToListAsync();

            foreach (var n in list)
            {
                // Determinar o outro participante para linkar mensagens
                int? outroParticipanteId = null;
                if (n.Tipo == "Mensagem")
                {
                    if (n.Id_Comprador == userId)
                    {
                        outroParticipanteId = n.Id_Vendedor;
                    }
                    else if (n.Id_Vendedor == userId)
                    {
                        outroParticipanteId = n.Id_Comprador;
                    }
                }

                vm.Items.Add(new NotificationItemViewModel
                {
                    Id = n.Id_notificacao,
                    Titulo = n.Tipo,
                    Texto = n.Mensagem,
                    Data = n.Data_Envio,
                    Lida = n.Estado,
                    OutroParticipanteId = outroParticipanteId
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

            ViewBag.UnreadMessagesCount = await GetUnreadMessagesCount(userId.Value);

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

            ViewBag.UnreadMessagesCount = await GetUnreadMessagesCount(userId.Value);

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
        public async Task<IActionResult> Admin()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            // check if admin
            bool isAdmin = await _context.Administradores
                .AnyAsync(a => a.Id_User == userId);

            if (!isAdmin)
                return RedirectToAction("Index"); // Normal user dashboard

            // Admin encontrado — redireciona para o controlador Admin
            return RedirectToAction("DashAdmin", "Admin");
        }
    }
}
