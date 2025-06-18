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
                Text = "-- Select Outlets --",
                Selected = true,
                Disabled = true
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
                Text = "-- Select ProductName --",
                Selected = true,
                Disabled = true
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
                Text = "-- Production Id --",
                Selected = true,
                Disabled = true

            };

            lstProducts.Insert(0, defItem);
            return lstProducts;
        }

        private static List<ProductionCapture> ProductionCapture_list = new List<ProductionCapture>();

        //Accept both xls and xlsx
        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file uploaded!" });
            }

            try
            {
                var extension = Path.GetExtension(file.FileName).ToLower();
                var Production_Id = GetProductionId();
                var productionList = new List<ProductionCapture>();
                var maxid = _context.ProductionCapture.Any() ? _context.ProductionCapture.Max(e => e.Id) : 0;

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                var outletLookup = _context.OutletMaster
                    .AsEnumerable()
                    .ToDictionary(o => o.OutletName.Trim().ToLowerInvariant(), o => o.OutletName.Trim());

                var productLookup = _context.ProductMaster
                    .AsEnumerable()
                    .ToDictionary(p => p.ProductName.Trim().ToLowerInvariant(), p => p.ProductName.Trim());

                List<string> outletNames = new List<string>();
                HashSet<string> skippedProducts = new HashSet<string>();

                if (extension == ".xlsx")
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using var package = new ExcelPackage(stream);
                    var worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    // Find the index of the "TOTAL" column
                    int totalColIndex = -1;
                    for (int col = 1; col <= colCount; col++)
                    {
                        var headerText = worksheet.Cells[1, col].Text.Trim().ToUpper().Replace("\n", " ");
                        if (headerText == "TOTAL")
                        {
                            totalColIndex = col;
                            break;
                        }
                    }
                    int loopUntil = totalColIndex > 0 ? totalColIndex : colCount;

                    for (int col = 3; col < loopUntil; col++)
                    {
                        var outletName = worksheet.Cells[1, col].Text.Trim().Replace("\n", " ").Trim();
                        if (!string.IsNullOrEmpty(outletName))
                        {
                            outletNames.Add(outletName);
                        }
                    }

                    var missingOutlets = outletNames
                        .Where(name => !outletLookup.ContainsKey(name.ToLowerInvariant()))
                        .ToList();

                    if (missingOutlets.Any())
                        return Json(new { success = false, message = $"The following outlets do not exist in the Outlet Master: {string.Join(", ", missingOutlets)}" });

                    for (int row = 3; row <= rowCount; row++)
                    {
                        string productNameRaw = worksheet.Cells[row, 1].Text.Trim();
                        string unit = worksheet.Cells[row, 2].Text.Trim();
                        string productKey = productNameRaw.ToLowerInvariant();

                        if (!productLookup.ContainsKey(productKey))
                        {
                            if (!string.Equals(productNameRaw, "FANCY CAKE", StringComparison.OrdinalIgnoreCase))
                            {
                                skippedProducts.Add(productNameRaw);
                            }
                            continue;
                        }

                        string canonicalProductName = productLookup[productKey];

                        for (int col = 3; col < loopUntil; col++)
                        {
                            int outletIndex = col - 3;
                            if (outletIndex >= outletNames.Count)
                                continue;

                            var cellValue = worksheet.Cells[row, col]?.Text;
                            if (!string.IsNullOrWhiteSpace(cellValue) && int.TryParse(cellValue.Trim(), out int quantity) && quantity > 0)
                            {
                                var outletKey = outletNames[outletIndex].ToLowerInvariant();
                                var outletName = outletLookup[outletKey];
                                var username = HttpContext.User.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name)?.Value;
                                maxid++;

                                productionList.Add(new ProductionCapture
                                {
                                    Id = maxid,
                                    Production_Id = Production_Id,
                                    ProductName = canonicalProductName,
                                    Unit = unit,
                                    OutletName = outletName,
                                    TotalQty = quantity,
                                    Production_Date = DateTime.Now.ToString("dd-MM-yyyy"),
                                    Production_Time = DateTime.Now.ToString("HH:mm"),
                                    Status = "Pending",
                                    User = username
                                });
                            }
                        }
                    }
                }
                else if (extension == ".xls")
                {
                    using var workbook = new NPOI.HSSF.UserModel.HSSFWorkbook(stream);
                    var sheet = workbook.GetSheetAt(0);
                    int rowCount = sheet.LastRowNum;
                    var headerRow = sheet.GetRow(0);
                    int colCount = headerRow.LastCellNum;

                    int totalColIndex = -1;
                    for (int col = 0; col < colCount; col++)
                    {
                        var cellValue = headerRow.GetCell(col)?.ToString()?.Trim().ToUpper().Replace("\n", " ");
                        if (cellValue == "TOTAL")
                        {
                            totalColIndex = col;
                            break;
                        }
                    }
                    int loopUntil = totalColIndex > 0 ? totalColIndex : colCount;

                    for (int col = 2; col < loopUntil; col++)
                    {
                        var cellValue = headerRow.GetCell(col)?.ToString()?.Trim().Replace("\n", " ");
                        if (!string.IsNullOrEmpty(cellValue))
                        {
                            outletNames.Add(cellValue);
                        }
                    }

                    var missingOutlets = outletNames
                        .Where(name => !outletLookup.ContainsKey(name.ToLowerInvariant()))
                        .ToList();

                    if (missingOutlets.Any())
                        return Json(new { success = false, message = $"The following outlets do not exist in the Outlet Master: {string.Join(", ", missingOutlets)}" });

                    for (int row = 1; row <= rowCount; row++)
                    {
                        var currentRow = sheet.GetRow(row);
                        if (currentRow == null) continue;

                        string productNameRaw = currentRow.GetCell(0)?.ToString().Trim();
                        string unit = currentRow.GetCell(1)?.ToString().Trim();
                        string productKey = productNameRaw.ToLowerInvariant();

                        if (!productLookup.ContainsKey(productKey))
                        {
                            if (!string.Equals(productNameRaw, "FANCY CAKE", StringComparison.OrdinalIgnoreCase))
                            {
                                skippedProducts.Add(productNameRaw);
                            }
                            continue;
                        }

                        string canonicalProductName = productLookup[productKey];

                        for (int col = 2; col < loopUntil; col++)
                        {
                            int outletIndex = col - 2;
                            if (outletIndex >= outletNames.Count)
                                continue;

                            var cellValue = currentRow.GetCell(col)?.ToString();
                            if (!string.IsNullOrWhiteSpace(cellValue) && int.TryParse(cellValue.Trim(), out int quantity) && quantity > 0)
                            {
                                var outletKey = outletNames[outletIndex].ToLowerInvariant();
                                var outletName = outletLookup[outletKey];
                                var username = HttpContext.User.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name)?.Value;
                                maxid++;

                                productionList.Add(new ProductionCapture
                                {
                                    Id = maxid,
                                    Production_Id = Production_Id,
                                    ProductName = canonicalProductName,
                                    Unit = unit,
                                    OutletName = outletName,
                                    TotalQty = quantity,
                                    Production_Date = DateTime.Now.ToString("dd-MM-yyyy"),
                                    Production_Time = DateTime.Now.ToString("HH:mm"),
                                    Status = "Pending",
                                    User = username
                                });
                            }
                        }
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Unsupported file format. Please upload .xls or .xlsx files only." });
                }

                _context.ProductionCapture.AddRange(productionList);
                await _context.SaveChangesAsync();

                string successMessage = "Data uploaded successfully!";
                if (skippedProducts.Any())
                {
                    successMessage += $" Skipped products: {string.Join(", ", skippedProducts)}";
                    return Json(new { success = false, message = successMessage });
                }

                return Json(new { success = true, message = successMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error processing file: " + ex.Message });
            }
        }



        /*  [HttpPost]
          public async Task<IActionResult> UploadExcel(IFormFile file)
          {
              if (file == null || file.Length == 0)
              {
                  return Json(new { success = false, message = "No file uploaded!" });
              }

              try
              {
                  ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                  var Production_Id = GetProductionId();

                  using (var stream = new MemoryStream())
                  {
                      await file.CopyToAsync(stream);

                      using (var package = new ExcelPackage(stream))
                      {
                          ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                          int rowCount = worksheet.Dimension.Rows;
                          int colCount = worksheet.Dimension.Columns;

                          List<ProductionCapture> productionList = new List<ProductionCapture>();
                          var maxid = _context.ProductionCapture.Any() ? _context.ProductionCapture.Max(e => e.Id) : 0;

                          // ✅ Extract outlet names from header row
                          List<string> outletNames = new List<string>();
                          for (int col = 3; col < colCount; col++)
                          {
                              outletNames.Add(worksheet.Cells[1, col].Text.Trim());
                          }

                          // ✅ Validate outlet names
                          var foundOutlets = _context.OutletMaster.Select(a => a.OutletName.Trim()).ToList();
                          var missingOutlets = outletNames.Where(name => !foundOutlets.Contains(name)).ToList();

                          if (missingOutlets.Any())
                          {
                              string missingOutletNames = string.Join(", ", missingOutlets);
                              return Json(new { success = false, message = $"The following outlets do not exist in the Outlet Master: {missingOutletNames}" });
                          }

                          // ✅ Get all valid products from ProductMaster
                          var validProductNames = _context.ProductMaster.Select(p => p.ProductName.Trim()).ToHashSet();

                          // ✅ Keep track of skipped products
                          var skippedProducts = new HashSet<string>();

                          for (int row = 3; row <= rowCount; row++)
                          {
                              string productName = worksheet.Cells[row, 1].Text.Trim();
                              string unit = worksheet.Cells[row, 2].Text.Trim();

                              if (!validProductNames.Contains(productName))
                              {
                                  skippedProducts.Add(productName); // Track and skip invalid product
                                  continue;
                              }

                              int totalQty = Convert.ToInt32(worksheet.Cells[row, colCount].Text.Trim());

                              for (int col = 3; col < colCount; col++)
                              {
                                  if (int.TryParse(worksheet.Cells[row, col].Text.Trim(), out int quantity) && quantity > 0)
                                  {
                                      var currentuser = HttpContext.User;
                                      string username = currentuser.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name).Value;

                                      maxid++;

                                      ProductionCapture production = new ProductionCapture
                                      {
                                          Id = maxid,
                                          Production_Id = Production_Id,
                                          ProductName = productName,
                                          Unit = unit,
                                          OutletName = outletNames[col - 3],
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

                          // ✅ Save valid data
                          _context.ProductionCapture.AddRange(productionList);
                          await _context.SaveChangesAsync();

                          // ✅ Prepare response
                          string message = "Data uploaded successfully!";
                          if (skippedProducts.Any())
                          {
                              message += $" Skipped products: {string.Join(", ", skippedProducts)}";
                              return Json(new { success = false, message });

                          }

                          return Json(new { success = true, message });
                      }
                  }
              }
              catch (Exception ex)
              {
                  return Json(new { success = false, message = "Error processing file: " + ex.Message });
              }
          }*/



        public IActionResult ExportExcel()
        {
            try
            {
                var products = _context.ProductMaster.Select(p => new { p.ProductName, p.Uom }).Distinct().ToList();
                var outlets = _context.OutletMaster.Select(p => p.OutletName).Distinct().Where(o => !string.IsNullOrEmpty(o)).ToList();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Production Template");

                    int totalColumns = outlets.Count + 3; // 2 fixed columns + outlet columns + total

                    // ✅ Row 1: Column Headers
                    worksheet.Cells[1, 1].Value = "PRODUCT";
                    worksheet.Cells[1, 2].Value = "UNIT";

                    int colIndex = 3;
                    foreach (var outlet in outlets)
                    {
                        worksheet.Cells[1, colIndex].Value = outlet;
                        colIndex++;
                    }

                    worksheet.Cells[1, colIndex].Value = "TOTAL";

                    // ✅ Style the header row
                    using (var range = worksheet.Cells[1, 1, 1, totalColumns])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    // ✅ Row 2: Title (merged and centered below headers)
                    worksheet.Cells[2, 1, 2, totalColumns].Merge = true;
                    worksheet.Cells[2, 1].Value = "Category";
                    //worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    // ✅ Data rows start from row 3
                    int rowIndex = 3;
                    foreach (var product in products)
                    {
                        worksheet.Cells[rowIndex, 1].Value = product.ProductName;
                        worksheet.Cells[rowIndex, 2].Value = product.Uom;

                        using (var range = worksheet.Cells[rowIndex, 2, rowIndex, totalColumns])
                        {
                            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        }

                        string startColumn = "C";
                        string endColumn = GetExcelColumnName(totalColumns - 1);
                        worksheet.Cells[rowIndex, totalColumns].Formula = $"SUM({startColumn}{rowIndex}:{endColumn}{rowIndex})";

                        rowIndex++;
                    }

                    // ✅ Apply borders to all content (headers + data)
                    using (var borderRange = worksheet.Cells[1, 1, rowIndex - 1, totalColumns])
                    {
                        borderRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        borderRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        borderRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        borderRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    }

                    //worksheet.Cells.AutoFitColumns();

                    // Set manual width for fixed columns
                    worksheet.Column(1).Width = 30; // Product
                    worksheet.Column(2).Width = 12; // Unit

                    // Set manual width for outlet columns (columns 3 to colIndex - 1)
                    for (int col = 3; col < colIndex; col++)
                    {
                        worksheet.Column(col).Width = 25; // Adjust width as needed
                    }

                    worksheet.Column(totalColumns).Width = 15; // Total column

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
            List<ProductionCapture> productionCaptures = new List<ProductionCapture>();

            var yesdate = DateTime.Now.AddDays(-1).ToString("dd-MM-yyyy");
            var todaydate = DateTime.Now.ToString("dd-MM-yyyy");
            //Fetch ordered data from database
            var list = await _context.ProductionCapture.Where(a => a.Production_Date.Trim() == todaydate.Trim() || a.Production_Date.Trim() == yesdate.Trim()).OrderByDescending(a => a.Id).ToListAsync();

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
                var foundData = list.FirstOrDefault(a => a.Production_Date.Trim() == group.ProductionDate.Trim() && a.Production_Id.Trim() == group.ProductionOrderId.Trim());
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
                Text = "-- Select Design --",
                Selected = true,
                Disabled = true
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
            var checkiffound = _context.ProductionCapture.OrderByDescending(a => a.Id).Select(a => a.Production_Date).FirstOrDefault();
            var proid = _context.ProductionCapture.OrderByDescending(a => a.Id).Select(a => a.Production_Id).FirstOrDefault();
            //if(checkiffound != null)
            //{
            //    if (currentdate.Trim() == checkiffound.Trim())
            //    {
            //        return RedirectToAction(nameof(Index));
            //    }
            //}

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
