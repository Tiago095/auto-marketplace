using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMatch.Data;
using AutoMatch.Models;
using AutoMatch.Models.ViewModels;
using System.Linq;

namespace AutoMatch.Controllers
{
    public class VehicleController : Controller
    {
        private readonly AutoMatchContext _db;

        public VehicleController(AutoMatchContext db)
        {
            _db = db;
        }

        public IActionResult Results(string? brand, string? model, int? year, decimal? maxPrice, int? maxMileage, string? fuelType, string? transmission, string? bodyType, string? sort)
        {
            // Base query with active ads and their models
            IQueryable<Anuncio> baseQuery = _db.Anuncios
                .Include(a => a.Modelo)
                .Where(a => a.Estado);

            // Data for filters (all available brands/models on the site)
            var availableBrands = baseQuery
                .Select(a => a.Modelo.Marca)
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            var brandModels = baseQuery
                .GroupBy(a => a.Modelo.Marca)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(a => a.Modelo.NomeModelo)
                          .Distinct()
                          .OrderBy(n => n)
                          .ToList()
                );

            // Apply filters to a copy of the query
            IQueryable<Anuncio> query = baseQuery;

            if (!string.IsNullOrEmpty(brand))
                query = query.Where(a => a.Modelo.Marca == brand);

            if (!string.IsNullOrEmpty(model))
                query = query.Where(a => a.Modelo.NomeModelo == model);

            if (year.HasValue)
                query = query.Where(a => a.Ano.Year == year.Value);

            if (maxPrice.HasValue)
                query = query.Where(a => a.Preco <= maxPrice.Value);

            if (maxMileage.HasValue)
                query = query.Where(a => a.Kilometros <= maxMileage.Value);

            if (!string.IsNullOrEmpty(fuelType))
                query = query.Where(a => a.Modelo.Combustivel == fuelType);

            if (!string.IsNullOrEmpty(transmission))
            {
                bool isAutomatic = transmission.Equals("Automatic", StringComparison.OrdinalIgnoreCase);
                query = query.Where(a => a.Modelo.Transmissao == isAutomatic);
            }

            if (!string.IsNullOrEmpty(bodyType))
                query = query.Where(a => a.Modelo.Categoria == bodyType);

            // Sorting
            query = sort switch
            {
                "price-low-high" => query.OrderBy(a => a.Preco),
                "price-high-low" => query.OrderByDescending(a => a.Preco),
                "year-new-old" => query.OrderByDescending(a => a.Ano),
                "year-old-new" => query.OrderBy(a => a.Ano),
                "mileage-low-high" => query.OrderBy(a => a.Kilometros),
                _ => query.OrderBy(a => a.Preco)
            };

            var vehicles = query
                .Select(a => new Vehicle
                {
                    Id = a.Id_Anuncio,
                    Brand = a.Modelo.Marca,
                    Model = a.Modelo.NomeModelo,
                    Year = a.Ano.Year,
                    Price = a.Preco,
                    Mileage = a.Kilometros,
                    FuelType = a.Modelo.Combustivel,
                    Transmission = a.Modelo.Transmissao ? "Automatic" : "Manual",
                    BodyType = a.Modelo.Categoria,
                    ImageUrl = string.Empty,
                    Description = a.Descricao
                })
                .ToList();

            var viewModel = new VehicleResultsViewModel
            {
                Vehicles = vehicles,
                AvailableBrands = availableBrands,
                BrandModels = brandModels,
                SelectedBrand = brand,
                SelectedModel = model,
                SelectedYear = year,
                SelectedMaxPrice = maxPrice,
                SelectedMaxMileage = maxMileage,
                SelectedFuelType = fuelType,
                SelectedTransmission = transmission,
                SelectedBodyType = bodyType,
                SelectedSort = sort
            };

            return View(viewModel);
        }

        public IActionResult Details(int id)
        {
            var anuncio = _db.Anuncios
                .Include(a => a.Modelo)
                .FirstOrDefault(a => a.Id_Anuncio == id && a.Estado);

            if (anuncio == null)
            {
                return NotFound();
            }

            var vehicle = new Vehicle
            {
                Id = anuncio.Id_Anuncio,
                Brand = anuncio.Modelo.Marca,
                Model = anuncio.Modelo.NomeModelo,
                Year = anuncio.Ano.Year,
                Price = anuncio.Preco,
                Mileage = anuncio.Kilometros,
                FuelType = anuncio.Modelo.Combustivel,
                Transmission = anuncio.Modelo.Transmissao ? "Automatic" : "Manual",
                BodyType = anuncio.Modelo.Categoria,
                ImageUrl = string.Empty,
                Description = anuncio.Descricao
            };

            return View(vehicle);
        }
    }
}
