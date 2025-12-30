using System;
using System.Collections.Generic;

namespace AutoMatch.Models.ViewModels
{
    public class ConversationItemViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string UltimaMensagem { get; set; }
        public DateTime DataUltima { get; set; }
        public bool Online { get; set; }
        public int MensagensNaoLidas { get; set; } = 0;
    }

    public class MessageBubbleViewModel
    {
        public bool IsOutgoing { get; set; }
        public string Texto { get; set; }
        public DateTime Data { get; set; }
    }

    public class MessagesViewModel
    {
        public string UserName { get; set; }
        public IList<ConversationItemViewModel> Conversas { get; set; } = new List<ConversationItemViewModel>();
        public IList<MessageBubbleViewModel> Mensagens { get; set; } = new List<MessageBubbleViewModel>();

        public int? VendedorAtualId { get; set; }
    }
}
