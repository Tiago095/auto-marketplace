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

    public class MonthlySalesData
    {
        public string Month { get; set; }
        public int Count { get; set; }
    }

    public class SalesViewModel
    {
        public string UserName { get; set; }

        public int TotalVendasConcluidas { get; set; }
        public string TopModel { get; set; }
        public int TopModelUnidades { get; set; }

        public IList<SaleRowViewModel> RecentSales { get; set; } = new List<SaleRowViewModel>();
        public IList<MonthlySalesData> MonthlySales { get; set; } = new List<MonthlySalesData>();

        // Filtro selecionado: "7d", "1m", "1y"
        public string SelectedRange { get; set; } = "7d";
    }
}
