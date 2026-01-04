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

    public class ListingWithDocumentsViewModel
    {
        public int Id_Anuncio { get; set; }
        public string Titulo { get; set; }
        public List<DocumentItemViewModel> Documents { get; set; } = new List<DocumentItemViewModel>();
    }

    public class PurchaseWithDocumentsViewModel
    {
        public int Id_Compra { get; set; }
        public string CarTitle { get; set; }
        public List<DocumentItemViewModel> Documents { get; set; } = new List<DocumentItemViewModel>();
    }

    public class DocumentsViewModel
    {
        public string UserName { get; set; }
        public IList<ListingWithDocumentsViewModel> Listings { get; set; } = new List<ListingWithDocumentsViewModel>();
        public IList<PurchaseWithDocumentsViewModel> Purchases { get; set; } = new List<PurchaseWithDocumentsViewModel>();
    }
}
