using System;
using System.Collections.Generic;

namespace AutoMatch.Models.ViewModels
{
    public class BookingRowViewModel
    {
        public int ReservaId { get; set; }
        public string Vehicle { get; set; }
        public string Buyer { get; set; }
        public DateTime Date { get; set; }
        public DateTime DataFim { get; set; }
        public string Status { get; set; }
        public bool IsVendedor { get; set; }
        public bool CanAccept { get; set; }
    }

    public class BookingsViewModel
    {
        public string UserName { get; set; }
        public IList<BookingRowViewModel> Bookings { get; set; } = new List<BookingRowViewModel>();
    }
}
