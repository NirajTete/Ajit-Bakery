using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ajit_Bakery.Data;
using Ajit_Bakery.Models;
using AspNetCoreHero.ToastNotification.Notyf.Models;
using System.ComponentModel;
using System.Data;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Ajit_Bakery.Controllers
{
    public class ProductionCapturesController : Controller
    {
        private readonly DataDBContext _context;
        private IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _config;

        public ProductionCapturesController(DataDBContext context, IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            _context = context;
            _config = config;
            _webHostEnvironment = webHostEnvironment;
        }


        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file uploaded!" });
            }

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set license
                var Production_Id = GetProductionId(); // Generate a Production ID

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // 🔹 Fix for EPPlus License Issue

                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Read first sheet
                        int rowCount = worksheet.Dimension.Rows;
                        int colCount = worksheet.Dimension.Columns;

                        List<ProductionCapture> productionList = new List<ProductionCapture>();
                        var maxid = _context.ProductionCapture.Any() ? _context.ProductionCapture.Max(e => e.Id) + 0 : 0 ;

                        // Extract outlet names from the second row
                        List<string> outletNames = new List<string>();
                        for (int col = 3; col < colCount ; col++) // Outlet names start from column index 3
                        {
                            outletNames.Add(worksheet.Cells[2, col].Text.Trim()); // Read outlet names
                        }

                        //check outlet exist or not 
                        var found = _context.OutletMaster.Select(a => a.OutletName.Trim()).ToList();

                        // Find outlets that are missing
                        var missingOutlets = outletNames.Where(name => !found.Contains(name)).ToList();

                        if (missingOutlets.Any())
                        {
                            string missingOutletNames = string.Join(", ", missingOutlets);
                            return Json(new { success = false, message = $"The following outlets do not exist in the Outlet Master: {missingOutletNames}" });
                        }

                        //end

                        // Read data from the third row onwards
                        for (int row = 3; row <= rowCount; row++)
                        {
                            string productName = worksheet.Cells[row, 1].Text.Trim();
                            string unit = worksheet.Cells[row, 2].Text.Trim();
                            int totalQty = Convert.ToInt32(worksheet.Cells[row, colCount].Text.Trim()); // Last column is total qty
                            // Iterate through outlets and add records where quantity > 0
                            for (int col = 3; col < colCount; col++)
                            {
                                if (int.TryParse(worksheet.Cells[row, col].Text.Trim(), out int quantity) && quantity > 0)
                                {
                                     maxid = maxid + 1;
                                    ProductionCapture production = new ProductionCapture
                                    {
                                        Id = maxid,
                                        Production_Id = Production_Id,
                                        ProductName = productName,
                                        Unit = unit,
                                        OutletName = outletNames[col - 3], // Match column index with outlet name
                                        TotalQty = quantity,
                                        Production_Date = DateTime.Now.ToString("dd-MM-yyyy"),
                                        Production_Time = DateTime.Now.ToString("HH:mm"),
                                        Status = "Pending",
                                    };

                                    productionList.Add(production);
                                }
                            }
                        }

                        // Save to database
                        _context.ProductionCapture.AddRange(productionList);
                        await _context.SaveChangesAsync();
                    }
                }

                return Json(new { success = true, message = "Data uploaded successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error processing file: " + ex.Message });
            }
        }
        public IActionResult ExportExcel()
        {
            // Path to the existing file
            string templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "Doc", "ExcelExport.xlsx");

            if (!System.IO.File.Exists(templatePath))
            {
                return NotFound("The requested file was not found.");
            }

            // Read file bytes
            byte[] fileBytes = System.IO.File.ReadAllBytes(templatePath);

            // Return the file for download
            return File(fileBytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ExcelExport.xlsx");
        }
        public async Task<IActionResult> Index()
        {
            List<ProductionCapture> ProductionCapture = new List<ProductionCapture>() ;
            var list = await _context.ProductionCapture.OrderByDescending(a=>a.Id).ToListAsync();
            var groupedData = _context.ProductionCapture
                            .GroupBy(p => new { p.Production_Id, p.OutletName,p.Production_Date,p.ProductName ,p.Status})
                            .Select(g => new
                            {
                                ProductName = g.Key.ProductName,
                                ProductionOrderId = g.Key.Production_Id,
                                OutletName = g.Key.OutletName,
                                TotalProductionQty = g.Sum(x => x.TotalQty),
                                DateTime = g.Key.Production_Date,
                                Status = g.Key.Status, 
                            })
                            .ToList();

            if (list.Count > 0)
            {
                foreach(var item in groupedData)
                {
                    var founddata = list.Where(a => a.Production_Date.Trim() == item.DateTime.Trim()).FirstOrDefault();
                    var date = founddata.Production_Date + " - " + founddata.Production_Time;

                    //var checkstore = _context.SaveProduction.ToList();

                    ProductionCapture ProductionCapturenew = new ProductionCapture()
                    {
                        ProductName = item.ProductName,
                        Production_Id = item.ProductionOrderId,
                        OutletName = item.OutletName,
                        TotalQty = item.TotalProductionQty,
                        Production_Date = date,
                        Status = item.Status,
                    };
                    ProductionCapture.Add(ProductionCapturenew);
                }
            }
            ProductionCapture= ProductionCapture.OrderByDescending(a=>a.Production_Id).ToList();
            return View(ProductionCapture);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productionCapture = await _context.ProductionCapture
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productionCapture == null)
            {
                return NotFound();
            }

            return View(productionCapture);
        }
        
        public string GetProductionId()
        {
            // Step 1: Get current year and month in "YYMM" format
            var currentYearMonth = DateTime.Now.ToString("yyMM"); // Example: "2412" for Dec 2024
            string newBoxId;
            int newCounter;

            // Step 2: Retrieve the last BoxID
            var lastBox = _context.ProductionIds.Where(a => a.ProductionId.Trim().StartsWith("PID"))
                .OrderByDescending(b => b.id)
                .FirstOrDefault();

            if (lastBox != null)
            {
                // Extract the year and month (YYMM) from the last BoxID
                string lastYearMonth = lastBox.ProductionId.Substring(3, 4); // Extract characters 4-7 ("YYMM")
                int lastCounter = int.Parse(lastBox.ProductionId.Substring(7)); // Extract counter part

                if (lastYearMonth == currentYearMonth)
                {
                    // If the month matches, continue the counter
                    newCounter = lastCounter + 1;
                }
                else
                {
                    // If the month doesn't match, reset the counter to 1
                    newCounter = 1;
                }
            }
            else
            {
                // If no BoxID exists, start the counter at 1
                newCounter = 1;
            }

            // Step 3: Generate the new BoxID
            newBoxId = $"PID{currentYearMonth}{newCounter:D2}"; // Format: STBYYMMCC

            // Step 4: Save the new BoxID to the database
            var maxId = _context.ProductionIds.Any() ? _context.ProductionIds.Max(e => e.id) + 1 : 1;

            var newBoxEntry = new ProductionIds
            {
                id = maxId,
                ProductionId = newBoxId,
                date = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
            };
            _context.ProductionIds.Add(newBoxEntry);
            _context.SaveChanges();

            return newBoxId;
        }

        public string GetProductionId1()
        {
            // Step 1: Get current year and month in "YYMM" format
            var currentYearMonth = DateTime.Now.ToString("yyMM"); // Example: "2412" for Dec 2024
            string newBoxId;
            int newCounter;

            // Step 2: Retrieve the last BoxID
            var lastBox = _context.ProductionIds.Where(a => a.ProductionId.Trim().StartsWith("PID"))
                .OrderByDescending(b => b.id)
                .FirstOrDefault();

            if (lastBox != null)
            {
                // Extract the year and month (YYMM) from the last BoxID
                string lastYearMonth = lastBox.ProductionId.Substring(3, 4); // Extract characters 4-7 ("YYMM")
                int lastCounter = int.Parse(lastBox.ProductionId.Substring(7)); // Extract counter part

                if (lastYearMonth == currentYearMonth)
                {
                    // If the month matches, continue the counter
                    newCounter = lastCounter + 1;
                }
                else
                {
                    // If the month doesn't match, reset the counter to 1
                    newCounter = 1;
                }
            }
            else
            {
                // If no BoxID exists, start the counter at 1
                newCounter = 1;
            }

            // Step 3: Generate the new BoxID
            newBoxId = $"PID{currentYearMonth}{newCounter:D2}"; // Format: STBYYMMCC

            // Step 4: Save the new BoxID to the database
            var maxId = _context.ProductionIds.Any() ? _context.ProductionIds.Max(e => e.id) + 1 : 1;

            var newBoxEntry = new ProductionIds
            {
                id = maxId,
                ProductionId = newBoxId,
                date = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
            };
            //_context.ProductionIds.Add(newBoxEntry);
            //_context.SaveChanges();

            return newBoxId;
        }

        public IActionResult Create()
        {
            ViewBag.ProductionId = GetProductionId1();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( ProductionCapture productionCapture)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productionCapture);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(productionCapture);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productionCapture = await _context.ProductionCapture.FindAsync(id);
            if (productionCapture == null)
            {
                return NotFound();
            }
            return View(productionCapture);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  ProductionCapture productionCapture)
        {
            if (id != productionCapture.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productionCapture);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductionCaptureExists(productionCapture.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(productionCapture);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productionCapture = await _context.ProductionCapture
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productionCapture == null)
            {
                return NotFound();
            }

            return View(productionCapture);
        }

        private bool ProductionCaptureExists(int id)
        {
            return _context.ProductionCapture.Any(e => e.Id == id);
        }
    }
}
