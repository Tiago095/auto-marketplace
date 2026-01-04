using System;

namespace AutoMatch.Models.ViewModels
{
    public class CreateBookingRequest
    {
        public int anuncioId { get; set; }
        public DateTime dataInicio { get; set; }
        public DateTime dataFim { get; set; }
        public string? comentario { get; set; }
    }
}

