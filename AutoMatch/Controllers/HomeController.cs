using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AutoMatch.Models;
using AutoMatch.Services;

namespace AutoMatch.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEmailService _emailService;

    public HomeController(ILogger<HomeController> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public IActionResult Index()
    {
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
