using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AutoMatch.Data;
using AutoMatch.Models;
using AutoMatch.Models.ViewModels;
using AutoMatch.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;


public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEmailService _emailService;
     private readonly AutoMatchContext _db;

        public HomeController(ILogger<HomeController> logger, AutoMatchContext db, IEmailService emailService)
        {
            _logger = logger;
            _db = db;
            _emailService = emailService;
    }
   

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null &&
                Request.Cookies.ContainsKey("AutoMatch_UserId"))
            {
                int userId = int.Parse(Request.Cookies["AutoMatch_UserId"]);
                var user = _db.Utilizadores.FirstOrDefault(u => u.Id_User == userId);

                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.Id_User);
                    HttpContext.Session.SetString("UserName", user.UserName);
                    HttpContext.Session.SetString("UserInitial", user.UserName.Substring(0, 1).ToUpper());
                    HttpContext.Session.SetString("UserProfileImageUrl", user.ProfileImageUrl ?? string.Empty);
                }
            }

            // Buscar 3 anúncios aleatórios que estão ativos e não foram comprados
            var anunciosCompradosIds = await _db.Compras
                .Where(c => c.Estado)
                .Select(c => c.Id_Anuncio)
                .ToListAsync();

            var anunciosAleatorios = await _db.Anuncios
                .Include(a => a.Imagens)
                .Include(a => a.Modelo)
                .Where(a => a.Estado && !anunciosCompradosIds.Contains(a.Id_Anuncio))
                .OrderBy(x => Guid.NewGuid())
                .Take(3)
                .ToListAsync();

            // Buscar imagens dos anúncios
            var anuncioIds = anunciosAleatorios.Select(a => a.Id_Anuncio).ToList();
            var todasImagens = await _db.Imagens
                .Where(i => anuncioIds.Contains(i.Id_Anuncio))
                .OrderBy(i => i.Id_Anuncio)
                .ThenBy(i => i.Id_Imagem)
                .ToListAsync();

            var primeiraImagemPorAnuncio = todasImagens
                .GroupBy(i => i.Id_Anuncio)
                .ToDictionary(g => g.Key, g => g.First().CaminhoImagem);

            var featuredCars = anunciosAleatorios.Select(a => new FeaturedCarViewModel
            {
                Id = a.Id_Anuncio,
                Titulo = a.Titulo,
                Preco = a.Preco,
                ImageUrl = primeiraImagemPorAnuncio.ContainsKey(a.Id_Anuncio)
                    ? primeiraImagemPorAnuncio[a.Id_Anuncio]
                    : "/images/placeholder-car.jpg"
            }).ToList();

            ViewBag.FeaturedCars = featuredCars;

            // Buscar brands e modelos disponíveis da base de dados
            var baseQuery = _db.Anuncios
                .Include(a => a.Modelo)
                .Where(a => a.Estado)
                .Where(a => !_db.Compras.Any(c => c.Id_Anuncio == a.Id_Anuncio && c.Estado));

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

            ViewBag.AvailableBrands = availableBrands;
            ViewBag.BrandModels = brandModels;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        string? fullName = null;
        string? email = null;

        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId.HasValue)
        {
            var user = _db.Utilizadores.FirstOrDefault(u => u.Id_User == userId.Value);
            if (user != null)
            {
                fullName = user.Nome;
                email = user.Email;
            }
        }

        ViewBag.ContactFullName = fullName;
        ViewBag.ContactEmail = email;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SubmitContactForm([FromBody] ContactFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Please fill in all fields correctly." });
        }

        var emailSent = await _emailService.SendContactEmailAsync(
            model.FullName,
            model.Email,
            model.Topic,
            model.Message
        );

        if (emailSent)
        {
            return Ok(new { success = true, message = "Message sent successfully! We'll reply within 24 hours." });
        }
        else
        {
            return StatusCode(500, new { success = false, message = "Failed to send message. Please try again later." });
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
