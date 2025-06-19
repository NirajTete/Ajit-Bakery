using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ajit_Bakery.Models;
using AspNetCore;
using Ajit_Bakery.Data;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
namespace Ajit_Bakery.Controllers;
[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DataDBContext _context;

    public HomeController(ILogger<HomeController> logger, DataDBContext context)
    {
        _logger = logger;
        _context = context;

    }
    public IActionResult clearSession()
    {
        HttpContext.Session.Remove("ProductionCaptureList");
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Index()
    {
        var jsonData = HttpContext.Session.GetString("ProductionCaptureList");
        var productionListget = jsonData != null ? JsonConvert.DeserializeObject<List<ProductionCapture>>(jsonData) : new List<ProductionCapture>();



        var dateTimeNow = DateTime.Now.ToString("dd-MM-yyyy");
        //var list1 = _context.ProductionCapture.Where(a => a.Production_Date.Trim() == dateTimeNow.Trim()).ToList().Sum(a => a.TotalQty);

        var today = DateTime.Now.Date;
        var yesterday = today.AddDays(-1);
        var sixPM = new TimeSpan(18, 0, 0);

        // Convert time strings to TimeSpan safely
        bool TryParseTime(string timeStr, out TimeSpan result)
        {
            return TimeSpan.TryParseExact(timeStr, "hh\\:mm", null, out result);
        }

        // Fetch all production records
        var allProduction = _context.ProductionCapture.ToList();
        var filteredProduction = allProduction.Where(a =>
        {
            if (!TryParseTime(a.Production_Time, out var prodTime))
                return false;

            if (a.Production_Date.Trim() == today.ToString("dd-MM-yyyy") && prodTime < sixPM)
                return true;

            if (a.Production_Date.Trim() == yesterday.ToString("dd-MM-yyyy") && prodTime >= sixPM)
                return true;

            return false;
        }).ToList();

        // Count total planned quantity
        var list1 = filteredProduction.Sum(a => a.TotalQty);


        // Count completed dispatches
        var completedDispatchCount = _context.Dispatch.Where(s => /*s.Dispatch_Date == dateTimeNow &&*/ s.Status == "Completed").ToList().Count();

        // Count pending production orders
        var pendingProductionCount = _context.ProductionCapture/*.Where(s=> s.Production_Date == dateTimeNow)*/
            .Count(a => a.Status == "Pending");

        // Count completed production orders
        var completedProductionCount = _context.SaveProduction/*.Where(s => s.SaveProduction_Date == dateTimeNow)*/
            .Count(); /*x => x.Status == "Completed"*/

        // Assign values to ViewBag
        ViewBag.PendingOrders = pendingProductionCount;
        ViewBag.productionListgetBadge = productionListget.Count;
        ViewBag.TotalOrders = list1;
        ViewBag.CompletedDispatches = completedDispatchCount;
        ViewBag.CompletedProductions = completedProductionCount; // Added this

        return View();
    }



    public IActionResult StatusFound()
    {
        //public int Id { get; set; }
        //public string productionid { get; set; }
        //public string datetime { get; set; }
        //public string outletname { get; set; }
        //public string productname { get; set; }
        //public string qty { get; set; }
        //public string status { get; set; }
        //public string process { get; set; }
        List<StatusFound> statusFounds = new List<StatusFound>();
        var list = _context.ProductionCapture.ToList();
        foreach(var item in list)
        {
            StatusFound StatusFound = new StatusFound()
            {
                productionid = item.Production_Id,
                datetime = item.Production_Date + " " + item.Production_Time,
                outletname = item.OutletName,
                productname = item.ProductName,
                qty = (item.TotalQty).ToString(),
                process = "Production Capture",
                status = item.Status,
                //status = "Pending",
            };
            statusFounds.Add(StatusFound);   
        }
        return View(statusFounds);
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
