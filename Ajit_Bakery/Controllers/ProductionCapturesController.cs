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
using Microsoft.AspNetCore.Authorization;
using DocumentFormat.OpenXml.Bibliography;
using System.Security.Claims;
using iText.Commons.Actions.Data;
using AspNetCoreHero.ToastNotification.Abstractions;
using DocumentFormat.OpenXml.VariantTypes;
using Newtonsoft.Json;

namespace Ajit_Bakery.Controllers
{
    [Authorize]
    public class ProductionCapturesController : Controller
    {
        private readonly DataDBContext _context;
        private IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _config;
        public INotyfService _notyfyService { get; }
        public ProductionCapturesController(DataDBContext context, IWebHostEnvironment webHostEnvironment, IConfiguration config, INotyfService notyfyService)
        {
            _context = context;
            _config = config;
            _webHostEnvironment = webHostEnvironment;
            _notyfyService = notyfyService;
        }
        private List<SelectListItem> GetOutlets()
        {
            var lstProducts = _context.OutletMaster
                .AsNoTracking()
                .Select(n => new SelectListItem
                {
                    Value = n.OutletName,
                    Text = n.OutletName
                })
                .Distinct()
                .ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "- Select Outlets -"
            };

            lstProducts.Insert(0, defItem);

            return lstProducts;
        }
        private List<SelectListItem> ProductName()
        {
            var lstProducts = _context.ProductMaster
                .AsNoTracking()
                .Select(n => new SelectListItem
                {
                    Value = n.ProductName,
                    Text = n.ProductName
                })
                .Distinct()
                .ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "- Select ProductName -"
            };

            lstProducts.Insert(0, defItem);

            return lstProducts;
        }
        private List<SelectListItem> GetProductionIds()
        {
            var date = DateTime.Now.ToString("dd-MM-yyyy");
            var lstProducts = _context.ProductionCapture
                .Where(a => a.Production_Date.Trim() == date.Trim())
                .AsNoTracking()
                .Select(n => new SelectListItem
                {
                    Value = n.Production_Id,
                    Text = n.Production_Id
                })
                .Distinct()
                .ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "- Select Production Id -"
            };

            lstProducts.Insert(0, defItem);
            return lstProducts;
        }

        private static List<ProductionCapture> ProductionCapture_list = new List<ProductionCapture>();

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
                        var maxid = _context.ProductionCapture.Any() ? _context.ProductionCapture.Max(e => e.Id) + 0 : 0;

                        // Extract outlet names from the second row
                        List<string> outletNames = new List<string>();
                        for (int col = 3; col < colCount; col++) // Outlet names start from column index 3
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
                                    var currentuser = HttpContext.User;
                                    string username = currentuser.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name).Value;
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
                                        User = username,
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


        /* public IActionResult ExportExcel()
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
         }*/

        public IActionResult ExportExcel()
        {
            try
            {
                var products = _context.ProductMaster.Select(p => new { p.ProductName, p.Uom }).Distinct().ToList();

                var outlets = _context.OutletMaster.Select(p => p.OutletName).Distinct().Where(o => !string.IsNullOrEmpty(o)).ToList();

                // **Set EPPlus License**
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Production Template");

                    int totalColumns = outlets.Count + 3; // 2 columns for product & unit + outlet columns + total column

                    // **Set title in the first row (Merged & Centered)**
                    worksheet.Cells[1, 1, 1, totalColumns].Merge = true;
                    worksheet.Cells[1, 1].Value = "DAILY ORDER SHEET";
                    worksheet.Cells[1, 1].Style.Font.Size = 14;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    // **Set column headers in the second row**
                    worksheet.Cells[2, 1].Value = "PRODUCT";
                    worksheet.Cells[2, 2].Value = "UNIT";

                    int colIndex = 3;
                    foreach (var outlet in outlets)
                    {
                        worksheet.Cells[2, colIndex].Value = outlet;
                        colIndex++;
                    }

                    worksheet.Cells[2, colIndex].Value = "TOTAL"; // Last column for total

                    // **Style header row (Bold, Centered, Light Gray Background)**
                    using (var range = worksheet.Cells[2, 1, 2, totalColumns])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray); // Light gray header
                        range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin); // Border for headers
                    }

                    // Fill product names and UOM starting from row 3
                    int rowIndex = 3;
                    foreach (var product in products)
                    {
                        worksheet.Cells[rowIndex, 1].Value = product.ProductName; // Product Name (Left-aligned)
                        worksheet.Cells[rowIndex, 2].Value = product.Uom; // Unit

                        // Center align everything except product name
                        using (var range = worksheet.Cells[rowIndex, 2, rowIndex, totalColumns])
                        {
                            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        }

                        // Insert formula in Total column to auto sum the row values
                        string startColumn = "C"; // First outlet column
                        string endColumn = GetExcelColumnName(totalColumns - 1); // Last outlet column before total
                        worksheet.Cells[rowIndex, totalColumns].Formula = $"SUM({startColumn}{rowIndex}:{endColumn}{rowIndex})";

                        rowIndex++;
                    }

                    // **Apply borders to all filled cells**
                    using (var borderRange = worksheet.Cells[2, 1, rowIndex - 1, totalColumns])
                    {
                        borderRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        borderRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        borderRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        borderRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    }

                    // Auto-fit columns
                    worksheet.Cells.AutoFitColumns();

                    // Convert Excel package to byte array
                    var fileBytes = package.GetAsByteArray();

                    return File(fileBytes,
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "ProductionTemplate.xlsx");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Error generating file: " + ex.Message);
            }
        }

        // Function to get Excel column name based on index (1 = A, 2 = B, ...)
        private string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = string.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }


        //public async Task<IActionResult> Index()
        //{
        //    List<ProductionCapture> ProductionCapture = new List<ProductionCapture>();
        //    var list = await _context.ProductionCapture.OrderByDescending(a => a.Id).ToListAsync();
        //    var groupedData = _context.ProductionCapture
        //                    .GroupBy(p => new { p.Production_Id, p.OutletName, p.Production_Date, p.ProductName, p.Status })
        //                    .Select(g => new
        //                    {
        //                        ProductName = g.Key.ProductName,
        //                        ProductionOrderId = g.Key.Production_Id,
        //                        OutletName = g.Key.OutletName,
        //                        TotalProductionQty = g.Sum(x => x.TotalQty),
        //                        DateTime = g.Key.Production_Date,
        //                        Status = g.Key.Status,
        //                    })
        //                    .ToList();

        //    if (list.Count > 0)
        //    {
        //        foreach (var item in groupedData)
        //        {
        //            var founddata = list.Where(a => a.Production_Date.Trim() == item.DateTime.Trim()).FirstOrDefault();
        //            var date = founddata.Production_Date + " - " + founddata.Production_Time;

        //            //var checkstore = _context.SaveProduction.ToList();

        //            ProductionCapture ProductionCapturenew = new ProductionCapture()
        //            {
        //                ProductName = item.ProductName,
        //                Production_Id = item.ProductionOrderId,
        //                OutletName = item.OutletName,
        //                TotalQty = item.TotalProductionQty,
        //                Production_Date = date,
        //                Status = item.Status,
        //            };
        //            ProductionCapture.Add(ProductionCapturenew);
        //        }
        //    }
        //    ProductionCapture = ProductionCapture.OrderByDescending(a => a.Production_Id).ToList();
        //    return View(ProductionCapture);
        //}

        public async Task<IActionResult> Index()
        {
            
            //if (TempData["NotyfMessage"] != null)
            //{
            //    string message = TempData["NotyfMessage"].ToString();
            //    string type = TempData["NotyfType"].ToString();

            //    if (type == "Warning")
            //        _notyfyService.Warning(message);
            //    else
            //        _notyfyService.Information(message); // You can handle other types if needed
            //}
            List<ProductionCapture> productionCaptures = new List<ProductionCapture>();

            var date = DateTime.Now.ToString("dd-MM-yyyy");
            //Fetch ordered data from database
            var list = await _context.ProductionCapture.Where(a => a.Production_Date.Trim() == date.Trim()).OrderByDescending(a => a.Id ).ToListAsync();

            //Get distinct outlet names dynamically from the data
            var allOutlets = list.Select(x => x.OutletName).Distinct().ToList();

            //Group data properly by Production_Id
            var groupedData = list
                .GroupBy(p => new { p.Production_Id, p.Production_Date, p.Status })
                .Select(g => new
                {
                    ProductionOrderId = g.Key.Production_Id,
                    ProductionDate = g.Key.Production_Date,
                    Status = g.Key.Status,
                    Products = g.GroupBy(x => x.ProductName)
                                .Select(p => new
                                {
                                    ProductName = p.Key,
                                    TotalProductionQty = p.Sum(x => x.TotalQty),
                                    Outlets = p.GroupBy(x => x.OutletName)
                                               .ToDictionary(gn => gn.Key, gn => gn.Sum(x => x.TotalQty))
                                })
                                .ToList()
                })
                .ToList();

            //Process grouped data
            foreach (var group in groupedData)
            {
                //Fetch the matching record to get the correct timestamp
                var foundData = list.FirstOrDefault(a => a.Production_Date == group.ProductionDate);
                string formattedDate = foundData != null
                    ? $"{foundData.Production_Date} - {foundData.Production_Time}"
                    : group.ProductionDate.ToString();

                foreach (var product in group.Products)
                {
                    //Create a new object for the ViewModel

                    ProductionCapture productionCaptureNew = new ProductionCapture
                    {
                        Production_Id = group.ProductionOrderId,
                        ProductName = product.ProductName,
                        TotalQty = product.TotalProductionQty,
                        Status = group.Status,
                        Production_Date = formattedDate, // Date moved to last in the table
                        OutletData = product.Outlets,
                    };

                    productionCaptures.Add(productionCaptureNew);
                }
            }

            //Final sorting
            productionCaptures = productionCaptures.OrderByDescending(a => a.Production_Id).ToList();

            int totat_Qty = _context.ProductionCapture.Sum(pc => pc.TotalQty);

            ViewBag.totat_Qty = totat_Qty;
            ViewBag.AllOutlets = allOutlets; // Send outlet names dynamically to the view
            return View(productionCaptures);
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
            newBoxId = $"PID{currentYearMonth}{newCounter:D3}"; // Format: STBYYMMCC

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
        private List<SelectListItem> GetCakeDesign()
        {
            var lstProducts = _context.CakeDesign
                .AsNoTracking()
                .Select(n => new SelectListItem
                {
                    Value = n.CakeDesign_Name,
                    Text = n.CakeDesign_Name
                })
                .Distinct()
                .ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "-Select Design-"
            };

            lstProducts.Insert(0, defItem);

            return lstProducts;
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


        [HttpGet]
        public IActionResult Create()
        {
            var currentdate = DateTime.Now.ToString("dd-MM-yyyy");
            var checkiffound = _context.ProductionCapture.OrderByDescending(a=>a.Id).Select(a => a.Production_Date).FirstOrDefault();
            var proid = _context.ProductionCapture.OrderByDescending(a => a.Id).Select(a => a.Production_Id).FirstOrDefault();
            if(checkiffound != null)
            {
                if (currentdate.Trim() == checkiffound.Trim())
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            
            ViewBag.ProductionId = GetProductionId1();
            
            return View();
        }
        [HttpGet]
        public IActionResult CreateManually()
        {
            ViewBag.CakeDesign = GetCakeDesign();
            ViewBag.ProductionId = GetProductionIds();
            ViewBag.GetOutlets = GetOutlets();
            ViewBag.ProductName = ProductName();
            return View();
        }
        //public IActionResult SaveData([FromBody] ProductionCapture data)
        //{
        //    return Json(new { success = true, message = "Done" });
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateManually(ProductionCapture productionCapture)
        {
            try
            {
                ProductionCapture_list.Clear();
                var currentuser = HttpContext.User;
                string username = currentuser.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name).Value;

                var maxid = _context.ProductionCapture.Any() ? _context.ProductionCapture.Max(e => e.Id) + 0 : 0;
                maxid = maxid + 1;
                ProductionCapture production = new ProductionCapture
                {
                    Id = maxid,
                    Production_Id = productionCapture.Production_Id,
                    ProductName = productionCapture.ProductName,
                    Unit = "GMS",
                    OutletName = productionCapture.OutletName, // Match column index with outlet name
                    TotalQty = productionCapture.TotalQty,
                    Production_Date = DateTime.Now.ToString("dd-MM-yyyy"),
                    Production_Time = DateTime.Now.ToString("HH:mm"),
                    Status = "Pending",
                    User = username ?? "User",
                };
                ProductionCapture_list.Add(production);

                // Store data in Session (Convert List to JSON and Save)
                HttpContext.Session.SetString("ProductionCaptureList", JsonConvert.SerializeObject(ProductionCapture_list));
                // Retrieve count and pass it to View
                ViewBag.ProductionCount = ProductionCapture_list.Count;

                _context.ProductionCapture.Add(production);
                _context.SaveChanges();
                return Json(new { success = true, message = "Successfully Done !" });
            }
            catch (Exception EX)
            {
                return Json(new { success = false, message = "Error :" + EX.Message });
            }
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
        public async Task<IActionResult> Edit(int id, ProductionCapture productionCapture)
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

        [HttpPost]
        public IActionResult UpdateProduction([FromBody] List<ProductionUpdateModel> updatedData)
        {
            foreach (var item in updatedData)
            {
                // Find the record and update the quantity
                var record = _context.ProductionCapture
                    .FirstOrDefault(x => x.Production_Id == item.ProductId && x.ProductName == item.ProductName && x.OutletName == item.Outlet);

                if (record != null)
                {
                    record.Status = "Pending";
                    record.TotalQty = Convert.ToInt32(item.NewQty);
                    _context.ProductionCapture.Update(record);
                    _context.SaveChanges();
                }
                else
                {
                    //ADDON
                    var currentuser = HttpContext.User;
                    string username = currentuser.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name).Value;

                    var maxid = _context.ProductionCapture.Any() ? _context.ProductionCapture.Max(e => e.Id) + 0 : 0;
                    maxid = maxid + 1;
                    ProductionCapture production = new ProductionCapture
                    {
                        Id = maxid,
                        Production_Id = item.ProductId,
                        ProductName = item.ProductName,
                        Unit = "GMS",
                        OutletName = item.Outlet, // Match column index with outlet name
                        TotalQty = Convert.ToInt32(item.NewQty),
                        Production_Date = DateTime.Now.ToString("dd-MM-yyyy"),
                        Production_Time = DateTime.Now.ToString("HH:mm"),
                        Status = "Pending",
                        User = username ?? "User",
                    };
                    //ProductionCapture_list.Add(production);
                    _context.ProductionCapture.Add(production);
                    _context.SaveChanges();
                    //ENDED
                }
            }

            return Json(new { status = "success" });
        }
        public class ProductionUpdateModel
        {
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string Outlet { get; set; }
            public decimal OldQty { get; set; }
            public decimal NewQty { get; set; }
        }

    }
}
