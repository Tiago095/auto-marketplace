using System.Collections.Generic;

namespace AutoMatch.Models.ViewModels
{
    public class DocumentItemViewModel
    {
        public int Id { get; set; }
        public string CarTitle { get; set; }
        public string Tipo { get; set; }
        public string Caminho { get; set; }
        public bool IsListing { get; set; } // true = anúncios do vendedor, false = compras do comprador
    }

    public class DocumentsViewModel
    {
        public string UserName { get; set; }
        public IList<DocumentItemViewModel> ListingDocuments { get; set; } = new List<DocumentItemViewModel>();
        public IList<DocumentItemViewModel> PurchaseDocuments { get; set; } = new List<DocumentItemViewModel>();
    }
}
