using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AutoMatch.Data;
using AutoMatch.Models;
using AutoMatch.Services;

namespace AutoMatch.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AutoMatchContext _db;
namespace AutoMatch.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEmailService _emailService;

        public HomeController(ILogger<HomeController> logger, AutoMatchContext db)
        {
            _logger = logger;
            _db = db;
        }
    public HomeController(ILogger<HomeController> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

        public IActionResult Index()
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
                }
            }

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
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
