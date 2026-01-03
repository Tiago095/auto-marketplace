using System;
using System.Collections.Generic;

namespace AutoMatch.Models.ViewModels
{
    public class SaleRowViewModel
    {
        public DateTime Data { get; set; }
        public string Cliente { get; set; }
        public string Veiculo { get; set; }
        public decimal Valor { get; set; }
        public string Estado { get; set; }
    }

    public class SalesViewModel
    {
        public string UserName { get; set; }

        public int TotalVendasConcluidas { get; set; }
        public string TopModel { get; set; }
        public int TopModelUnidades { get; set; }

        public IList<SaleRowViewModel> RecentSales { get; set; } = new List<SaleRowViewModel>();

        public string SelectedRange { get; set; } = "7d";

        public IList<int> ChartPoints { get; set; } = new List<int>();

        public string ChartPointsSvg { get; set; } = string.Empty;
    }
}
