using System;
using System.Collections.Generic;
using AutoMatch.Models;

namespace AutoMatch.Models.ViewModels
{
    public class DashboardBookingInfo
    {
        public int ReservaId { get; set; }
        public int AnuncioId { get; set; }
        public string CarTitle { get; set; }
        public string? CarImageUrl { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
    }

    public class DashboardMessageInfo
    {
        public int? OutroParticipanteId { get; set; }
        public string NomeRemetente { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string Texto { get; set; }
        public DateTime Data { get; set; }
    }

    public class DashboardNotificationInfo
    {
        public int NotificacaoId { get; set; }
        public string? Tipo { get; set; }
        public int? OutroParticipanteId { get; set; }
        public string Texto { get; set; }
        public DateTime Data { get; set; }
    }

    public class DashboardViewModel
    {
        public string UserName { get; set; }

        // Quick stats
        public int PendingBookings { get; set; }
        public int UnreadMessages { get; set; }
        public int NewNotifications { get; set; }
        public int FiltersSaved { get; set; }

        public DashboardBookingInfo LatestBooking { get; set; }

        public List<DashboardMessageInfo> RecentMessages { get; set; } = new();

        public List<DashboardNotificationInfo> Notifications { get; set; } = new();
    }
}
