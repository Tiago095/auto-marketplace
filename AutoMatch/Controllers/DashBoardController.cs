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

            // Latest booking
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

            // Recent messages (from notifications of type "Mensagem")
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

            // Notifications (any other type)
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
    }
}
