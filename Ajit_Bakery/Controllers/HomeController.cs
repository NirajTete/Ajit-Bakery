using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ajit_Bakery.Models;
using AspNetCore;
using Ajit_Bakery.Data;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ajit_Bakery.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DataDBContext _context;

    public HomeController(ILogger<HomeController> logger, DataDBContext context)
    {
        _logger = logger;
        _context = context;

    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult test()
    {
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
