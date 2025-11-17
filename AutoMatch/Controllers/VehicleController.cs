using Microsoft.AspNetCore.Mvc;
using AutoMatch.Models;

namespace AutoMatch.Controllers
{
    public class VehicleController : Controller
    {
        // Dados de exemplo - mais tarde podes substituir por base de dados
        private static List<Vehicle> GetSampleVehicles()
        {
            return new List<Vehicle>
            {
                new Vehicle { Id = 1, Brand = "Tesla", Model = "Model 3", Year = 2020, Price = 30000, Mileage = 20000, FuelType = "Electric", Transmission = "Automatic", BodyType = "Sedan", ImageUrl = "~/images/eletric.png", Description = "Beautiful Tesla Model 3, in perfect condition and ready to turn heads. Sleek design, metallic finish, and a powerful sound that makes every drive unforgettable. Always well maintained and kept in a garage. If you're looking for a mix of style, comfort, and performance — this is the one. Serious buyers only." },
                new Vehicle { Id = 2, Brand = "Hyundai", Model = "Creta", Year = 2019, Price = 20000, Mileage = 70000, FuelType = "Petrol", Transmission = "Automatic", BodyType = "SUV", ImageUrl = "~/images/SUV_e.png", Description = "Reliable Hyundai Creta in excellent condition. Perfect for families with spacious interior and modern features. Well maintained with full service history." },
                new Vehicle { Id = 3, Brand = "Audi", Model = "A4", Year = 2018, Price = 25000, Mileage = 110000, FuelType = "Diesel", Transmission = "Manual", BodyType = "Sedan", ImageUrl = "~/images/sedan.png", Description = "Elegant Audi A4 with premium interior. Powerful diesel engine and smooth manual transmission. Great fuel efficiency for long distance driving." },
                new Vehicle { Id = 4, Brand = "BMW", Model = "Series 3", Year = 2021, Price = 35000, Mileage = 15000, FuelType = "Petrol", Transmission = "Automatic", BodyType = "Sedan", ImageUrl = "~/images/sports.png", Description = "Sporty BMW Series 3 with low mileage. Dynamic driving experience with luxury comfort. All advanced safety features included." },
                new Vehicle { Id = 5, Brand = "Tesla", Model = "Model Y", Year = 2022, Price = 45000, Mileage = 5000, FuelType = "Electric", Transmission = "Automatic", BodyType = "SUV", ImageUrl = "~/images/SUV.png", Description = "Nearly new Tesla Model Y with cutting-edge technology. Autopilot, premium sound system, and exceptional range. Like new condition." },
                new Vehicle { Id = 6, Brand = "Audi", Model = "Q5", Year = 2020, Price = 40000, Mileage = 30000, FuelType = "Diesel", Transmission = "Automatic", BodyType = "SUV", ImageUrl = "~/images/SUV_e.png", Description = "Luxurious Audi Q5 with quattro all-wheel drive. Perfect combination of performance and comfort. Meticulously maintained with premium package." }
            };
        }

        public IActionResult Results(string? brand, string? model, int? year, decimal? maxPrice, int? maxMileage, string? fuelType, string? transmission, string? bodyType, string? sort)
        {
            var vehicles = GetSampleVehicles();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(brand))
                vehicles = vehicles.Where(v => v.Brand.Equals(brand, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrEmpty(model))
                vehicles = vehicles.Where(v => v.Model.Equals(model, StringComparison.OrdinalIgnoreCase)).ToList();

            if (year.HasValue)
                vehicles = vehicles.Where(v => v.Year == year.Value).ToList();

            if (maxPrice.HasValue)
                vehicles = vehicles.Where(v => v.Price <= maxPrice.Value).ToList();

            if (maxMileage.HasValue)
                vehicles = vehicles.Where(v => v.Mileage <= maxMileage.Value).ToList();

            if (!string.IsNullOrEmpty(fuelType))
                vehicles = vehicles.Where(v => v.FuelType.Equals(fuelType, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrEmpty(transmission))
                vehicles = vehicles.Where(v => v.Transmission.Equals(transmission, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrEmpty(bodyType))
                vehicles = vehicles.Where(v => v.BodyType.Equals(bodyType, StringComparison.OrdinalIgnoreCase)).ToList();

            // Aplicar ordenação
            vehicles = sort switch
            {
                "price-low-high" => vehicles.OrderBy(v => v.Price).ToList(),
                "price-high-low" => vehicles.OrderByDescending(v => v.Price).ToList(),
                "year-new-old" => vehicles.OrderByDescending(v => v.Year).ToList(),
                "year-old-new" => vehicles.OrderBy(v => v.Year).ToList(),
                "mileage-low-high" => vehicles.OrderBy(v => v.Mileage).ToList(),
                _ => vehicles.OrderBy(v => v.Price).ToList()
            };

            return View(vehicles);
        }

        public IActionResult Details(int id)
        {
            var vehicles = GetSampleVehicles();
            var vehicle = vehicles.FirstOrDefault(v => v.Id == id);

            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }
    }
}
