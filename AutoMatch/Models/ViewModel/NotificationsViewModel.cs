using System;
using System.Collections.Generic;

namespace AutoMatch.Models.ViewModels
{
    public class NotificationItemViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Texto { get; set; }
        public DateTime Data { get; set; }
        public bool Lida { get; set; }
        public int? OutroParticipanteId { get; set; } // Para linkar mensagens
    }

    public class NotificationsViewModel
    {
        public string UserName { get; set; }
        public IList<NotificationItemViewModel> Items { get; set; } = new List<NotificationItemViewModel>();
    }
}
