using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AutoMatch.Data;
using AutoMatch.Models;

namespace AutoMatch.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AutoMatchContext _db;

        public HomeController(ILogger<HomeController> logger, AutoMatchContext db)
        {
            _logger = logger;
            _db = db;
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
                    HttpContext.Session.SetString("UserProfileImageUrl", user.ProfileImageUrl ?? string.Empty);
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
