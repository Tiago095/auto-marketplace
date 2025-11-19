using AutoMatch.Data;
using AutoMatch.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoMatch.Controllers
{
    public class DashBoardController : Controller
    {
        private readonly AutoMatchContext _context;

        public DashBoardController(AutoMatchContext context)
        {
            _context = context;
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

        public async Task<IActionResult> Messages()
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

            var notificacoes = await _context.Notificacoes
                .Where(n => n.Id_Comprador == userId && n.Tipo == "Mensagem")
                .OrderByDescending(n => n.Data_Envio)
                .ToListAsync();

            var grupos = notificacoes
                .GroupBy(n => n.Id_Vendedor)
                .ToList();

            foreach (var g in grupos)
            {
                var ultima = g.First();
                vm.Conversas.Add(new ConversationItemViewModel
                {
                    Id = g.Key,
                    Nome = "Vendedor #" + g.Key,
                    UltimaMensagem = ultima.Mensagem,
                    DataUltima = ultima.Data_Envio,
                    Online = false
                });
            }

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
            }

            return View(vm);
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

            var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.Id_User == userId);
            if (vendedor != null)
            {
                var listingDocs = await _context.Documentos
                    .Include(d => d.Anuncio)
                    .Where(d => d.Anuncio.Id_Vendedor == vendedor.Id_User)
                    .ToListAsync();

                foreach (var d in listingDocs)
                {
                    vm.ListingDocuments.Add(new DocumentItemViewModel
                    {
                        Id = d.Id_Doc,
                        CarTitle = d.Anuncio?.Titulo ?? "Anuncio",
                        Tipo = d.Tipo,
                        Caminho = d.CaminhoDocumento,
                        IsListing = true
                    });
                }
            }

            var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.Id_User == userId);
            if (comprador != null)
            {
                var compras = await _context.Compras
                    .Include(c => c.Anuncio)
                    .Where(c => c.Id_Comprador == comprador.Id_User)
                    .ToListAsync();

                var anuncioIds = compras.Select(c => c.Id_Anuncio).ToList();

                var purchaseDocs = await _context.Documentos
                    .Include(d => d.Anuncio)
                    .Where(d => anuncioIds.Contains(d.Id_Anuncio))
                    .ToListAsync();

                foreach (var d in purchaseDocs)
                {
                    vm.PurchaseDocuments.Add(new DocumentItemViewModel
                    {
                        Id = d.Id_Doc,
                        CarTitle = d.Anuncio?.Titulo ?? "Anuncio",
                        Tipo = d.Tipo,
                        Caminho = d.CaminhoDocumento,
                        IsListing = false
                    });
                }
            }

            return View(vm);
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
