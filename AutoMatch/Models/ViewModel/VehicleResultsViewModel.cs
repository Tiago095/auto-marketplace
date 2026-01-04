using System.Collections.Generic;

namespace AutoMatch.Models.ViewModels
{
    public class VehicleResultsViewModel
    {
        public List<Vehicle> Vehicles { get; set; } = new();

        public List<string> AvailableBrands { get; set; } = new();
        public Dictionary<string, List<string>> BrandModels { get; set; } = new();

        public string? SelectedBrand { get; set; }
        public string? SelectedModel { get; set; }
        public int? SelectedYear { get; set; }
        public decimal? SelectedMaxPrice { get; set; }
        public int? SelectedMaxMileage { get; set; }
        public string? SelectedFuelType { get; set; }
        public string? SelectedTransmission { get; set; }
        public string? SelectedBodyType { get; set; }
        public string? SelectedSort { get; set; }
    }
}