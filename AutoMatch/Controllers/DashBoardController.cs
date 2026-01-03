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

            // Verificar se é vendedor
            var isVendedor = await _context.Vendedores.AnyAsync(v => v.Id_User == userId);

            // Quick stats - ajustar para vendedor ou comprador
            int pendingBookings;
            if (isVendedor)
            {
                // Para vendedor: contar reservas pendentes dos seus anúncios
                var anunciosIds = await _context.Anuncios
                    .Where(a => a.Id_Vendedor == userId)
                    .Select(a => a.Id_Anuncio)
                    .ToListAsync();
                pendingBookings = await _context.Reservas
                    .CountAsync(r => anunciosIds.Contains(r.Id_Anuncio) && !r.Estado);
            }
            else
            {
                // Para comprador: contar suas reservas pendentes
                pendingBookings = await _context.Reservas
                .CountAsync(r => r.Id_Comprador == userId && !r.Estado);
            }

            // Contar mensagens não lidas: mensagens onde o outro participante enviou e Estado = false
            var unreadMessages = await _context.Notificacoes
                .CountAsync(n => n.Tipo == "Mensagem" && 
                                !n.Estado && 
                                ((n.Id_Comprador == userId && n.Id_Vendedor != userId) || 
                                 (n.Id_Vendedor == userId && n.Id_Comprador != userId)));

            // Notificações não lidas - ajustar para vendedor ou comprador
            int newNotifications;
            if (isVendedor)
            {
                // Para vendedor: notificações recebidas (Id_Vendedor == userId)
                newNotifications = await _context.Notificacoes
                    .CountAsync(n => n.Id_Vendedor == userId && !n.Estado);
            }
            else
            {
                // Para comprador: notificações recebidas (Id_Comprador == userId)
                newNotifications = await _context.Notificacoes
                .CountAsync(n => n.Id_Comprador == userId && !n.Estado);
            }

            var comprador = await _context.Compradores.FirstOrDefaultAsync(c => c.Id_User == userId);
            var filtersSaved = 0;
            if (comprador != null)
            {
                filtersSaved = await _context.Preferencias
                    .CountAsync(p => p.Id_Comprador == comprador.Id_User);
            }

            // Latest Reserva - ajustar para vendedor ou comprador
            Reserva latestReserva = null;
            if (isVendedor)
            {
                // Para vendedor: última reserva dos seus anúncios
                var anunciosIds = await _context.Anuncios
                    .Where(a => a.Id_Vendedor == userId)
                    .Select(a => a.Id_Anuncio)
                    .ToListAsync();
                latestReserva = await _context.Reservas
                .Include(r => r.Anuncio)
                        .ThenInclude(a => a.Imagens)
                .OrderByDescending(r => r.Data_Inicio)
                    .FirstOrDefaultAsync(r => anunciosIds.Contains(r.Id_Anuncio));
            }
            else
            {
                // Para comprador: última reserva dele
                latestReserva = await _context.Reservas
                    .Include(r => r.Anuncio)
                        .ThenInclude(a => a.Imagens)
                    .OrderByDescending(r => r.Data_Inicio)
                .FirstOrDefaultAsync(r => r.Id_Comprador == userId);
            }

            DashboardBookingInfo latestBookingVm = null;
            if (latestReserva != null && latestReserva.Anuncio != null)
            {
                // Buscar imagem de capa
                string? carImageUrl = null;
                if (latestReserva.Anuncio.Imagens != null && latestReserva.Anuncio.Imagens.Any())
                {
                    var orderedImages = latestReserva.Anuncio.Imagens
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
                        .FirstOrDefault();
                    carImageUrl = orderedImages;
                }

                latestBookingVm = new DashboardBookingInfo
                {
                    ReservaId = latestReserva.Id_Reserva,
                    AnuncioId = latestReserva.Id_Anuncio,
                    CarTitle = latestReserva.Anuncio.Titulo ?? "Reserva",
                    CarImageUrl = carImageUrl,
                    DataInicio = latestReserva.Data_Inicio,
                    DataFim = latestReserva.Data_Fim
                };
            }

            // Recent messages - ajustar para vendedor ou comprador
            var recentMessagesQuery = _context.Notificacoes
                .Where(n => n.Tipo == "Mensagem" &&
                            ((isVendedor && n.Id_Vendedor == userId) || 
                             (!isVendedor && n.Id_Comprador == userId)))
                .OrderByDescending(n => n.Data_Envio)
                .Take(2);

            var recentMessagesList = await recentMessagesQuery
                .Include(n => n.Comprador)
                    .ThenInclude(c => c.Utilizador)
                .Include(n => n.Vendedor)
                    .ThenInclude(v => v.Utilizador)
                .ToListAsync();

            var recentMessages = recentMessagesList.Select(n =>
            {
                // Determinar o outro participante
                int? outroParticipanteId = null;
                string nomeRemetente = "Unknown";
                string? profileImageUrl = null;

                if (isVendedor)
                {
                    // Vendedor recebe mensagem de comprador
                    outroParticipanteId = n.Id_Comprador;
                    var comprador = n.Comprador?.Utilizador;
                    nomeRemetente = comprador?.Nome ?? comprador?.UserName ?? $"User #{n.Id_Comprador}";
                    profileImageUrl = comprador?.ProfileImageUrl;
                }
                else
                {
                    // Comprador recebe mensagem de vendedor
                    outroParticipanteId = n.Id_Vendedor;
                    var vendedor = n.Vendedor?.Utilizador;
                    nomeRemetente = vendedor?.Nome ?? vendedor?.UserName ?? $"User #{n.Id_Vendedor}";
                    profileImageUrl = vendedor?.ProfileImageUrl;
                }

                return new DashboardMessageInfo
                {
                    OutroParticipanteId = outroParticipanteId,
                    NomeRemetente = nomeRemetente,
                    ProfileImageUrl = profileImageUrl,
                    Texto = n.Mensagem,
                    Data = n.Data_Envio
                };
            }).ToList();

            // Notifications - ajustar para vendedor ou comprador
            var notificationsQuery = _context.Notificacoes
                .Where(n => (isVendedor && n.Id_Vendedor == userId) || 
                           (!isVendedor && n.Id_Comprador == userId))
                .OrderByDescending(n => n.Data_Envio)
                .Take(5);

            var notificationsList = await notificationsQuery
                .Include(n => n.Comprador)
                    .ThenInclude(c => c.Utilizador)
                .Include(n => n.Vendedor)
                    .ThenInclude(v => v.Utilizador)
                .ToListAsync();

            var notifications = notificationsList.Select(n =>
            {
                // Determinar o outro participante para linkar
                int? outroParticipanteId = null;
                if (n.Tipo == "Mensagem")
                {
                    outroParticipanteId = isVendedor ? n.Id_Comprador : n.Id_Vendedor;
                }
                else if (n.Tipo == "Booking")
                {
                    outroParticipanteId = isVendedor ? n.Id_Comprador : n.Id_Vendedor;
                }

                return new DashboardNotificationInfo
                {
                    NotificacaoId = n.Id_notificacao,
                    Tipo = n.Tipo,
                    OutroParticipanteId = outroParticipanteId,
                    Texto = n.Mensagem,
                    Data = n.Data_Envio
                };
            }).ToList();

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

            // Verificar se é vendedor
            var isVendedor = await _context.Vendedores.AnyAsync(v => v.Id_User == userId);
            ViewBag.IsVendedor = isVendedor;

            var vm = new BookingsViewModel
            {
                UserName = userName
            };

            if (isVendedor)
            {
                // Buscar reservas dos anúncios do vendedor
                var anunciosIds = await _context.Anuncios
                    .Where(a => a.Id_Vendedor == userId)
                    .Select(a => a.Id_Anuncio)
                    .ToListAsync();

                var reservas = await _context.Reservas
                    .Include(r => r.Anuncio)
                    .Include(r => r.Comprador)
                        .ThenInclude(c => c.Utilizador)
                    .Where(r => anunciosIds.Contains(r.Id_Anuncio))
                    .OrderByDescending(r => r.Data_Inicio)
                    .ToListAsync();

                foreach (var r in reservas)
                {
                    var status = r.Estado ? "Accepted" : "Pending";
                    vm.Bookings.Add(new BookingRowViewModel
                    {
                        ReservaId = r.Id_Reserva,
                        Vehicle = r.Anuncio?.Titulo ?? "(sem título)",
                        Buyer = r.Comprador?.Utilizador?.Nome ?? r.Comprador?.Utilizador?.UserName ?? "Unknown",
                        Date = r.Data_Inicio,
                        DataFim = r.Data_Fim,
                        Status = status,
                        IsVendedor = true,
                        CanAccept = !r.Estado // Pode aceitar se ainda estiver pendente
                    });
                }
            }
            // Se não for vendedor, não carregar reservas (mostrar apenas mensagem para se tornar vendedor)

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptBooking(int reservaId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            var reserva = await _context.Reservas
                .Include(r => r.Anuncio)
                .FirstOrDefaultAsync(r => r.Id_Reserva == reservaId);

            if (reserva == null)
            {
                return Json(new { success = false, message = "Reserva não encontrada" });
            }

            // Verificar se o utilizador é o vendedor do anúncio
            if (reserva.Anuncio.Id_Vendedor != userId)
            {
                return Json(new { success = false, message = "Não autorizado" });
            }

            // Aceitar a reserva
            reserva.Estado = true;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Reserva aceite com sucesso" });
        }

        [HttpPost]
        public async Task<IActionResult> RejectBooking(int reservaId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            var reserva = await _context.Reservas
                .Include(r => r.Anuncio)
                .FirstOrDefaultAsync(r => r.Id_Reserva == reservaId);

            if (reserva == null)
            {
                return Json(new { success = false, message = "Reserva não encontrada" });
            }

            // Verificar se o utilizador é o vendedor do anúncio
            if (reserva.Anuncio.Id_Vendedor != userId)
            {
                return Json(new { success = false, message = "Não autorizado" });
            }

            // Rejeitar a reserva (apagar)
            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Reserva rejeitada" });
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
                .Select(u => new { u.Id_User, u.Nome, u.UserName, u.ProfileImageUrl })
                .ToListAsync();

            var outrosParticipantesDict = outrosParticipantes.ToDictionary(u => u.Id_User);

            // Contar mensagens não lidas por conversa
            foreach (var kvp in conversasDict)
            {
                var outroId = kvp.Key;
                var mensagens = kvp.Value.mensagens;
                var ultimaMensagem = mensagens.OrderByDescending(m => m.Data_Envio).First();

                var outroParticipante = outrosParticipantesDict.ContainsKey(outroId)
                    ? outrosParticipantesDict[outroId]
                    : null;

                var nomeOutro = outroParticipante?.Nome ?? $"User #{outroId}";
                var userNameOutro = outroParticipante?.UserName ?? $"User{outroId}";
                var profileImageUrlOutro = outroParticipante?.ProfileImageUrl;

                // Contar mensagens não lidas: mensagens enviadas pelo outro participante que ainda não foram lidas (Estado = false)
                var mensagensNaoLidas = mensagens.Count(m =>
                    ((m.Id_Comprador == outroId && m.Id_Vendedor == userId) ||
                     (m.Id_Vendedor == outroId && m.Id_Comprador == userId)) &&
                    !m.Estado);

                vm.Conversas.Add(new ConversationItemViewModel
                {
                    Id = outroId,
                    Nome = nomeOutro,
                    UserName = userNameOutro,
                    ProfileImageUrl = profileImageUrlOutro,
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
                    var userNameOutro = outro?.UserName ?? $"User{conversaId.Value}";
                    var profileImageUrlOutro = outro?.ProfileImageUrl;

                    vm.Conversas.Insert(0, new ConversationItemViewModel
                    {
                        Id = conversaId.Value,
                        Nome = nomeOutro,
                        UserName = userNameOutro,
                        ProfileImageUrl = profileImageUrlOutro,
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
                    // IsOutgoing = true se a mensagem foi enviada pelo utilizador atual
                    // Como Id_Comprador é sempre o remetente, verificar se é o userId
                    vm.Mensagens.Add(new MessageBubbleViewModel
                    {
                        IsOutgoing = n.Id_Comprador == userId,
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

            // Determinar o destinatário (aceita vendedorId ou compradorId, ambos tratados como qualquer utilizador)
            int? recipientId = vendedorId ?? compradorId;

            if (!recipientId.HasValue || recipientId.Value == userId)
            {
                return Json(new { success = false, message = "Invalid recipient" });
            }

            try
            {
                // Para a tabela Notificacoes, precisamos de:
                // - Id_Comprador: sempre o remetente (userId)
                // - Id_Vendedor: sempre o destinatário (recipientId)
                // Isto é apenas uma convenção da estrutura da tabela, não uma limitação real

                int idComprador = userId.Value;
                int idVendedor = recipientId.Value;

                // Garantir que o remetente existe como comprador
                var compradorExiste = await _context.Compradores.AnyAsync(c => c.Id_User == idComprador);
                if (!compradorExiste)
                {
                    var novoComprador = new Comprador
                    {
                        Id_User = idComprador,
                        Contactos = "N/A",
                        Rua = "Desconhecida",
                        Codigo_Postal = "0000-000"
                    };
                    _context.Compradores.Add(novoComprador);
                    await _context.SaveChangesAsync();
                }

                // Garantir que o destinatário existe como vendedor (criar se necessário)
                var vendedorExiste = await _context.Vendedores.AnyAsync(v => v.Id_User == idVendedor);
                if (!vendedorExiste)
                {
                    // Garantir que o código postal padrão existe
                    var codigoPostalExiste = await _context.CodigoPostais.AnyAsync(cp => cp.Codigo_Postal == "0000-000");
                    if (!codigoPostalExiste)
                    {
                        var novoCodigoPostal = new CodigoPostal
                        {
                            Codigo_Postal = "0000-000",
                            Localidade = "Desconhecida"
                        };
                        _context.CodigoPostais.Add(novoCodigoPostal);
                        await _context.SaveChangesAsync();
                    }

                    // Criar registo temporário de vendedor para permitir mensagens entre qualquer utilizador
                    var novoVendedor = new Vendedor
                    {
                        Id_User = idVendedor,
                        Tipo = false, // false = pessoa física
                        Contactos = "N/A",
                        Rua = "Desconhecida",
                        Codigo_Postal = "0000-000"
                    };
                    _context.Vendedores.Add(novoVendedor);
                    await _context.SaveChangesAsync();
                }

                var notificacao = new Notificacoes
                {
                    Id_Comprador = idComprador,
                    Id_Vendedor = idVendedor,
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
                // Capturar a exceção interna para mais detalhes
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " | Inner: " + ex.InnerException.Message;
                }
                return Json(new { success = false, message = errorMessage });
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

            // Buscar apenas notificações recebidas pelo utilizador (não as que ele enviou)
            // Para mensagens: mostrar apenas quando Id_Vendedor == userId (destinatário)
            // Para bookings: mostrar apenas quando Id_Vendedor == userId (vendedor recebe a notificação)
            // Para outras notificações: mostrar quando o utilizador é destinatário
            var list = await _context.Notificacoes
                .Where(n => n.Tipo == "Mensagem" 
                    ? (n.Id_Vendedor == userId) // Apenas mensagens recebidas
                    : n.Tipo == "Booking"
                    ? (n.Id_Vendedor == userId) // Apenas bookings recebidos pelo vendedor
                    : ((n.Id_Comprador == userId) || (n.Id_Vendedor == userId))) // Outras notificações
                .OrderByDescending(n => n.Data_Envio)
                .ToListAsync();

            // Buscar informações dos remetentes para melhorar os títulos
            var remetentesIds = list
                .Where(n => n.Tipo == "Mensagem" || n.Tipo == "Booking")
                .Select(n => n.Id_Comprador) // Remetente da mensagem/booking
                .Distinct()
                .ToList();

            var remetentes = await _context.Utilizadores
                .Where(u => remetentesIds.Contains(u.Id_User))
                .ToDictionaryAsync(u => u.Id_User, u => u.UserName);

            foreach (var n in list)
            {
                // Determinar o outro participante para linkar mensagens/bookings
                int? outroParticipanteId = null;
                string titulo = n.Tipo;

                if (n.Tipo == "Mensagem")
                {
                    // Para mensagens, o remetente é sempre Id_Comprador
                    outroParticipanteId = n.Id_Comprador;
                    var remetenteNome = remetentes.ContainsKey(n.Id_Comprador) 
                        ? remetentes[n.Id_Comprador] 
                        : $"User #{n.Id_Comprador}";
                    titulo = $"Nova mensagem de {remetenteNome}";
                }
                else if (n.Tipo == "Booking")
                {
                    // Para bookings, o remetente é sempre Id_Comprador (quem fez a reserva)
                    outroParticipanteId = n.Id_Comprador;
                    var remetenteNome = remetentes.ContainsKey(n.Id_Comprador) 
                        ? remetentes[n.Id_Comprador] 
                        : $"User #{n.Id_Comprador}";
                    titulo = $"Nova reserva de test drive de {remetenteNome}";
                }
                else
                {
                    // Para outras notificações, determinar o outro participante
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
                    Titulo = titulo,
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
