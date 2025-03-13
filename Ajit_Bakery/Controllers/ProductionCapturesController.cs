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

namespace Ajit_Bakery.Controllers
{
    public class ProductionCapturesController : Controller
    {
        private readonly DataDBContext _context;
        private IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _config;

        public ProductionCapturesController(DataDBContext context, IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            _config = config;
            _context = context;
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
                List<ProductionCapture> productionList = new List<ProductionCapture>();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // 🔹 Fix for EPPlus License Issue

                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Read first sheet
                        int rowCount = worksheet.Dimension.Rows;
                        int colCount = worksheet.Dimension.Columns;

                        // Identify dynamic outlet columns
                        Dictionary<int, string> outletColumns = new Dictionary<int, string>();

                        for (int col = 3; col <= colCount - 1; col++) // Start from 3rd column, ignore last "Total" column
                        {
                            string outletName = worksheet.Cells[1, col].Text.Trim(); // Read header row for outlet names
                            if (!string.IsNullOrEmpty(outletName) && outletName.ToLower() != "total")
                            {
                                outletColumns[col] = outletName;
                            }
                        }

                        // Process rows dynamically
                        for (int row = 2; row <= rowCount; row++) // Skip header row
                        {
                            string productName = worksheet.Cells[row, 1].Text.Trim();
                            string unit = worksheet.Cells[row, 2].Text.Trim();

                            foreach (var outlet in outletColumns)
                            {
                                int colIndex = outlet.Key;
                                string outletName = outlet.Value;

                                int qty = worksheet.Cells[row, colIndex].Value != null
                                    ? Convert.ToInt32(worksheet.Cells[row, colIndex].Value)
                                    : 0;

                                if (qty > 0)
                                {
                                    ProductionCapture production = new ProductionCapture
                                    {
                                        Production_Id = Guid.NewGuid().ToString(),
                                        ProductName = productName,
                                        Unit = unit,
                                        OutletName = outletName,
                                        TotalQty = qty,
                                        Production_Date = DateTime.Now.ToString("yyyy-MM-dd"),
                                        Production_Time = DateTime.Now.ToString("HH:mm:ss")
                                    };

                                    productionList.Add(production);
                                }
                            }
                        }
                    }
                }

                // Save to database
                await _context.ProductionCapture.AddRangeAsync(productionList);
                await _context.SaveChangesAsync();

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
            var list = await _context.ProductionCapture.ToListAsync();
            var groupedData = _context.ProductionCapture
                            .GroupBy(p => new { p.Production_Id, p.OutletName,p.Production_Date })
                            .Select(g => new
                            {
                                ProductionOrderId = g.Key.Production_Id,
                                OutletName = g.Key.OutletName,
                                TotalProductionQty = g.Sum(x => x.TotalQty),
                                DateTime = g.Key.Production_Date, 
                            })
                            .ToList();

            if (list.Count > 0)
            {
                foreach(var item in groupedData)
                {
                    var founddata = list.Where(a => a.Production_Date.Trim() == item.DateTime.Trim()).FirstOrDefault();
                    var date = founddata.Production_Date + " " + founddata.Production_Time;
                    ProductionCapture ProductionCapturenew = new ProductionCapture()
                    {
                        Production_Id = item.ProductionOrderId,
                        OutletName = item.OutletName,
                        TotalQty = item.TotalProductionQty,
                        Production_Date = date,
                    };
                    ProductionCapture.Add(ProductionCapturenew);
                }
            }
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
        public IActionResult Create()
        {
            ViewBag.ProductionId = GetProductionId();
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
