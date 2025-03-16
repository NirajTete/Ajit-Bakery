using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ajit_Bakery.Models;
using AspNetCore;
using Ajit_Bakery.Data;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;

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
        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        // Fetch only Production_Date from the database
        var productionList = _context.ProductionCapture
            .Select(x => x.Production_Date) // Fetch only necessary column
            .ToList();  // Load into memory

        // Filter records that match the current month and year
        var list1 = productionList
            .Where(dateString =>
                !string.IsNullOrEmpty(dateString) && // Avoid null values
                dateString.Length == 10 && // Ensure valid format (dd-MM-yyyy)
                dateString[2] == '-' && dateString[5] == '-' // Validate correct format
            )
            .Select(dateString => dateString.Split('-')) // Split into [dd, MM, yyyy]
            .Where(parts =>
                int.TryParse(parts[1], out int month) && month == currentMonth && // Match month
                int.TryParse(parts[2], out int year) && year == currentYear // Match year
            )
            .Count(); // Get the count
                      
        var list = _context.ProductionCapture.Where(a=>a.Status == "Pending").ToList().Count();
        ViewBag.PendingOrders = list;
        ViewBag.TotalOrders = list1;
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
