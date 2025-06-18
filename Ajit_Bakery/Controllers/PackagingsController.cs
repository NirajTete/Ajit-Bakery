using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ajit_Bakery.Data;
using Ajit_Bakery.Models;
using Microsoft.Reporting.NETCore;
using Microsoft.AspNetCore.Hosting;
using QRCoder;
using System.Drawing;
using ZXing.QrCode.Internal;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Data;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using iTextSharp.text.pdf.qrcode;
using System.Net.Sockets;
using System.Text;
using System.Net;
using Microsoft.CodeAnalysis.Elfie.Extensions;
//using AspNetCore.Reporting.ReportExecutionService;

namespace Ajit_Bakery.Controllers
{
    [Authorize]
    public class PackagingsController : Controller
    {
        private readonly DataDBContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _config;
        public PackagingsController(DataDBContext context, IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            _config = config;
        }

        public async Task<IActionResult> Index()
        {
            var date = DateTime.Now.ToString("dd-MM-yyyy");
            //var LIST = await _context.SaveProduction.Where(a => a.Qty > 0 && a.SaveProduction_Date.Trim() == date.Trim()).OrderByDescending(a => a.Id).ToListAsync();

            Packagings_List.Clear();
            SaveProduction_List.Clear();
            ProductionCapture_List.Clear();
            //var list = await _context.Packaging.OrderByDescending(a => a.Id).ToListAsync();
            //foreach (var item in list)
            //{
            //    item.TotalNetWg_Uom = item.TotalNetWg + " " + item.TotalNetWg_Uom;
            //}
            //return View(list);
            Packagings_List.Clear();

            var list = _context.Packaging
                .Where(a => a.Packaging_Date.Trim() == date.Trim())
                .OrderByDescending(a => a.Id)
                .AsEnumerable()  // Fetch data first, then perform grouping in memory
                .GroupBy(a => new
                {
                    a.Production_Id,
                    a.DCNo,
                    a.Outlet_Name,
                    a.Box_No,
                    a.Reciept_Id,
                    a.Product_Name,
                    a.Category
                })
                .Select(g => new Packaging
                {
                    Production_Id = g.Key.Production_Id,
                    DCNo = g.Key.DCNo,
                    Outlet_Name = g.Key.Outlet_Name,
                    Box_No = g.Key.Box_No,
                    Reciept_Id = g.Key.Reciept_Id,
                    Product_Name = g.Key.Product_Name,
                    Category = g.Key.Category, // Ensure it's available in memory
                    Qty = g.Sum(a => a.Qty) // Summing the Quantity
                })
                .ToList();
            foreach (var item in list)
            {
                var check = _context.ProductMaster.Where(a => a.ProductName.Trim() == item.Product_Name.Trim()).FirstOrDefault();
                if (check != null)
                {
                    item.Category = check.Type;
                }
            }
            return View(list);
        }

        private static List<Packaging> Packagings_List = new List<Packaging>();
        private static List<SaveProduction> SaveProduction_List = new List<SaveProduction>();
        private static List<ProductionCapture> ProductionCapture_List = new List<ProductionCapture>();


        public IActionResult PickedData(string productcode, string productname, double wg, double mrp, string Production_Id, string Box_No, string outletName)
        {
            List<SaveProduction> check = new List<SaveProduction>();
            //check stock
            var check1 = _context.SaveProduction.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Qty > 0 && a.ProductName.Trim() == productname.Trim()).ToList();
            if (check1.Count > 0)
            {
                //against outlet getting totalqty to pick
                var check_outlet = _context.ProductionCapture.Where(a => a.Status.Trim() == "Pending" && a.OutletName.Trim() == outletName.Trim() && a.ProductName.Trim() == productname.Trim()).ToList().Sum(a => a.TotalQty);

                if (check_outlet == 0)
                {
                    return Json(new { success = false, message = "Please scan correct one against outlet " + outletName });
                }
                var check_packing = _context.Packaging.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Product_Name.Trim() == productname.Trim() && a.Outlet_Name.Trim() == outletName.Trim()).ToList().Sum(a => a.Qty);
                var check_packing1 = Packagings_List.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Product_Name.Trim() == productname.Trim() && a.Outlet_Name.Trim() == outletName.Trim()).ToList().Sum(a => a.Qty);

                if ((check_packing + check_packing1) == check_outlet)
                {
                    return Json(new { success = false, message = "You have already scan all qty against that outlet of production id " + Production_Id });
                }
                else
                {
                    //var checkagain = _context.SaveProduction.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Qty > 0 && a.ProductName.Trim() == productname.Trim() && a.Packaging_Flag == 0 && a.TotalNetWg == wg).FirstOrDefault();
                    // Fetch matching records from the database
                    var potentialRecords = _context.SaveProduction
                        .Where(a => a.Production_Id.Trim() == Production_Id.Trim()
                            && a.Qty > 0
                            && a.ProductName.Trim() == productname.Trim()
                            && a.Packaging_Flag == 0
                            && a.TotalNetWg == wg)
                        .ToList(); // Convert to List for in-memory filtering

                    // Filter out records already in Packagings_List
                    var checkagain = potentialRecords
                        .FirstOrDefault(a => !SaveProduction_List.Any(p => p.Id == a.Id && p.ProductName == a.ProductName));

                    if (checkagain != null)
                    {
                        checkagain.Packaging_Flag = 1;
                        checkagain.Box_No = Box_No;
                        checkagain.Packaging_Date = DateTime.Now.ToString("dd-MM-yyyy");
                        SaveProduction_List.Add(checkagain);
                        //_context.SaveProduction.Update(checkagain);
                        //_context.SaveChanges();

                        Packaging packaging = new Packaging()
                        {
                            //Id = _context.Packaging.Any() ? _context.Packaging.Max(e => e.Id) + 1 : 1,
                            Product_Name = productname,
                            Production_Dt = checkagain.SaveProduction_Date,
                            Production_Tm = checkagain.SaveProduction_Time,
                            Production_Id = Production_Id,
                            Box_No = Box_No,
                            Outlet_Name = outletName,
                            Qty = 1,
                            TotalNetWg = wg,
                            TotalNetWg_Uom = checkagain.TotalNetWg_Uom,
                            Exp_Dt = checkagain.Exp_Date,
                            sellingRs = checkagain.sellingRs,
                            mrpRs = checkagain.mrpRs,
                        };
                        Packagings_List.Add(packaging);
                        //_context.Packaging.Add(packaging);
                        //_context.SaveChanges();

                        var Packagingdata = _context.Packaging.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Outlet_Name.Trim() == outletName.Trim()).ToList().Sum(a => a.Qty);
                        var Packagingdata1 = Packagings_List.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Outlet_Name.Trim() == outletName.Trim()).ToList().Sum(a => a.Qty);
                        var ProductionCapturedata = _context.ProductionCapture.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Status.Trim() == "Pending" && a.OutletName.Trim() == outletName.Trim()).ToList().Sum(a => a.TotalQty);
                        var ProductionCapturedata1 = ProductionCapture_List.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.OutletName.Trim() == outletName.Trim()).ToList().Sum(a => a.TotalQty);

                        var qtyremainig = ProductionCapturedata + ProductionCapturedata1;
                        var qtypick = Packagingdata + Packagingdata1;
                        var valuefound = Math.Abs((qtypick - qtyremainig));

                        return Json(new { success = true, data = checkagain, qtypick = qtypick, qtyremainig = valuefound });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Already scan !" });
                    }
                }
            }

            return Json(new { success = false, message = "Save Production Data not found  !" });

        }

        //Added logic to restrict user for box
        /*  public IActionResult PickedData(string productcode, string productname, double wg, double mrp, string Production_Id, string Box_No, string outletName)
          {
              // ✅ STEP 0: Verify product exists in SaveProduction with Qty > 0
              var availableItems = _context.SaveProduction
                  .Where(a => a.Production_Id.Trim() == Production_Id.Trim() &&
                              a.Qty > 0 &&
                              a.ProductName.Trim() == productname.Trim())
                  .ToList();

              if (!availableItems.Any())
              {
                  return Json(new { success = false, message = "Save Production Data not found!" });
              }

              // ✅ STEP 1: Check required qty from ProductionCapture (Pending status for outlet & product)
              var outletQty = _context.ProductionCapture
                  .Where(a => a.Status.Trim() == "Pending" &&
                              a.OutletName.Trim() == outletName.Trim() &&
                              a.ProductName.Trim() == productname.Trim())
                  .Sum(a => a.TotalQty);

              if (outletQty == 0)
              {
                  return Json(new { success = false, message = $"Please scan correct product for outlet {outletName}" });
              }

              // ✅ STEP 2: Calculate already packed quantity (from DB + in-memory list)
              var packedQtyDb = _context.Packaging
                  .Where(a => a.Production_Id.Trim() == Production_Id.Trim() &&
                              a.Product_Name.Trim() == productname.Trim() &&
                              a.Outlet_Name.Trim() == outletName.Trim())
                  .Sum(a => a.Qty);

              var packedQtyMem = Packagings_List
                  .Where(a => a.Production_Id.Trim() == Production_Id.Trim() &&
                              a.Product_Name.Trim() == productname.Trim() &&
                              a.Outlet_Name.Trim() == outletName.Trim())
                  .Sum(a => a.Qty);

              if ((packedQtyDb + packedQtyMem) >= outletQty)
              {
                  return Json(new { success = false, message = $"All quantities for outlet '{outletName}' already scanned for Production ID {Production_Id}" });
              }

              // ✅ STEP 3: Get DialCode from matching SaveProduction item based on weight
              var dialCode = availableItems
                  .Where(x => x.TotalNetWg == wg)
                  .Select(x => x.DialCode)
                  .FirstOrDefault();

              if (string.IsNullOrWhiteSpace(dialCode))
              {
                  return Json(new { success = false, message = $"Dial code not found for product '{productname}'" });
              }

              // ✅ STEP 4: Get DialArea from DialMaster
              var dialAreaStr = _context.DialMaster
                  .Where(x => x.DialCode == dialCode)
                  .Select(x => x.DialArea)
                  .FirstOrDefault();

              if (!double.TryParse(dialAreaStr, out double dialArea) || dialArea == 0)
              {
                  return Json(new { success = false, message = $"Dial area not found or invalid for DialCode '{dialCode}'" });
              }

              // ✅ STEP 5: Get BoxArea from BoxMaster
              var boxAreaStr = _context.BoxMaster
                  .Where(x => x.BoxNumber == Box_No)
                  .Select(x => x.BoxArea)
                  .FirstOrDefault();

              if (!double.TryParse(boxAreaStr, out double boxArea) || boxArea == 0)
              {
                  return Json(new { success = false, message = $"Box area not found or invalid for Box No. '{Box_No}'" });
              }

              // ✅ STEP 6: Calculate already used area in the box (DB + memory)
              var usedAreaDb = (from p in _context.Packaging
                                join s in _context.SaveProduction on p.Production_Id equals s.Production_Id
                                join d in _context.DialMaster on s.DialCode equals d.DialCode
                                where p.Box_No == Box_No
                                select d.DialArea)
                                .ToList()
                                .Sum(a => double.TryParse(a, out var val) ? val : 0);

              var usedAreaMem = (from p in Packagings_List
                                 join s in _context.SaveProduction on p.Production_Id equals s.Production_Id
                                 join d in _context.DialMaster on s.DialCode equals d.DialCode
                                 where p.Box_No == Box_No
                                 select d.DialArea)
                                 .ToList()
                                 .Sum(a => double.TryParse(a, out var val) ? val : 0);

              var totalUsedArea = usedAreaDb + usedAreaMem;

              // ✅ STEP 7: Validate if new item fits in box area
              if ((totalUsedArea + dialArea) > boxArea)
              {
                  return Json(new
                  {
                      success = false,
                      message = $"Box '{Box_No}' is full! Used: {totalUsedArea}, New: {dialArea}, Capacity: {boxArea}"
                  });
              }

              // ✅ STEP 8: Find the item to be packed (not already packed, and matching weight)
              var itemToPack = availableItems
                  .Where(x => x.TotalNetWg == wg &&
                              x.Packaging_Flag == 0 &&
                              !SaveProduction_List.Any(p => p.Id == x.Id && p.ProductName == x.ProductName))
                  .FirstOrDefault();

              if (itemToPack == null)
              {
                  return Json(new { success = false, message = "Already scanned or no valid item found!" });
              }

              // ✅ STEP 9: Update SaveProduction in memory and add Packaging
              itemToPack.Packaging_Flag = 1;
              itemToPack.Box_No = Box_No;
              itemToPack.Packaging_Date = DateTime.Now.ToString("dd-MM-yyyy");
              SaveProduction_List.Add(itemToPack);

              Packagings_List.Add(new Packaging
              {
                  Product_Name = productname,
                  Production_Dt = itemToPack.SaveProduction_Date,
                  Production_Tm = itemToPack.SaveProduction_Time,
                  Production_Id = Production_Id,
                  Box_No = Box_No,
                  Outlet_Name = outletName,
                  Qty = 1,
                  TotalNetWg = wg,
                  TotalNetWg_Uom = itemToPack.TotalNetWg_Uom,
                  Exp_Dt = itemToPack.Exp_Date,
                  sellingRs = itemToPack.sellingRs,
                  mrpRs = itemToPack.mrpRs
              });

              // ✅ STEP 10: Return updated status with qty packed and remaining
              var totalPacked = packedQtyDb + packedQtyMem + 1;
              var totalToPack = outletQty;

              return Json(new
              {
                  success = true,
                  data = itemToPack,
                  qtypick = totalPacked,
                  qtyremainig = Math.Abs(totalToPack - totalPacked)
              });
          }*/



        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var packaging = await _context.Packaging
                .FirstOrDefaultAsync(m => m.Id == id);
            if (packaging == null)
            {
                return NotFound();
            }

            return View(packaging);
        }
        public IActionResult GetOutlet_NameData(string Production_Id, string Outlet_Name)
        {

            var date = DateTime.Now.ToString("dd-MM-yyyy");
            var yestdate = DateTime.Now.AddDays(-1).ToString("dd-MM-yyyy");
            var Packagingdata = _context.Packaging.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Outlet_Name.Trim() == Outlet_Name.Trim() && (a.Packaging_Date.Trim() == date.Trim() || a.Packaging_Date.Trim() == yestdate.Trim())).ToList().Sum(a => a.Qty);
            var Packagingdata1 = Packagings_List.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Outlet_Name.Trim() == Outlet_Name.Trim() && (a.Packaging_Date.Trim() == date.Trim() || a.Packaging_Date.Trim() == yestdate.Trim())).ToList().Sum(a => a.Qty);

            var ProductionCapturedata = _context.ProductionCapture.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Status.Trim() == "Pending" && a.OutletName.Trim() == Outlet_Name.Trim() && (a.Production_Date.Trim() == date.Trim() || a.Production_Date.Trim() == yestdate.Trim())).ToList().Sum(a => a.TotalQty);
            var ProductionCapturedata1 = ProductionCapture_List.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Status.Trim() == "Pending" && a.OutletName.Trim() == Outlet_Name.Trim() && (a.Production_Date.Trim() == date.Trim() || a.Production_Date.Trim() == yestdate.Trim())).ToList().Sum(a => a.TotalQty);

            var qtyremainig = ProductionCapturedata + ProductionCapturedata1;
            var qtypick = Packagingdata + Packagingdata1;
            var valuefound = Math.Abs(qtypick - qtyremainig);
            if (valuefound > 0)
            {
                var exist = Packagings_List.ToList();
                if (exist.Count > 0)
                {
                    var found = exist.Select(a => a.Outlet_Name.Trim()).FirstOrDefault();
                    if (Outlet_Name.Trim() != found)
                    {
                        return Json(new { success = false, message = "You have alredy scan items of another outlet please submit those first then do another !" });
                    }
                }
                return Json(new { success = true, qtypick = qtypick, qtyremainig = valuefound });
            }
            return Json(new { success = false, message = "No remaining Qty found against " + Outlet_Name + "  outlet !" });
        }

        /* public IActionResult GetOutlets(string Production_Id)
         {
             var lstProducts = new List<SelectListItem>();
             if (Production_Id != null)
             {
                 lstProducts = _context.ProductionCapture.Where(a => a.Status == "Pending" && a.Production_Id.Trim() == Production_Id.Trim()).AsNoTracking().Select(n =>
             new SelectListItem
             {
                 Value = n.OutletName,
                 Text = n.OutletName
             }).Distinct().ToList();

                 var defItem = new SelectListItem()
                 {
                     Value = "",
                     Text = "----Select OutletName ----"
                 };
                 lstProducts.Insert(0, defItem);
             }

             return Json(new { success = true, data = lstProducts });
         }*/

        public IActionResult GetOutlets(string Production_Id)
        {
            var lstProducts = new List<SelectListItem>();

            if (!string.IsNullOrEmpty(Production_Id))
            {
                var productNamesInSaveProduction = _context.SaveProduction
                    .Select(sp => sp.ProductName)
                    .Distinct();

                var outlets = _context.ProductionCapture
                    .Where(pc => productNamesInSaveProduction.Contains(pc.ProductName))
                    .Select(pc => pc.OutletName)
                    .Distinct()
                    .ToList();


                lstProducts = outlets
                    .Select(outlet => new SelectListItem
                    {
                        Value = outlet,
                        Text = outlet
                    })
                    .ToList();

                // Add default option
                lstProducts.Insert(0, new SelectListItem()
                {
                    Value = "",
                    Text = "-- Select Outlet --",
                    Selected = true,
                    Disabled = true
                });
            }
            return Json(new { success = true, data = lstProducts });
        }
        private List<SelectListItem> GetBoxNos(string selectedBoxNo = null)
        {
            var lstProducts = _context.BoxMaster
                .Where(b => b.Use_Flag == 0)
                .AsNoTracking()
                .Select(n => n.BoxNumber)
                .Distinct()
                .Select(n => new SelectListItem
                {
                    Value = n,
                    Text = n,
                    Selected = (selectedBoxNo != null && n == selectedBoxNo)
                })
                .OrderBy(x => x.Text)
                .ToList();

            lstProducts.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "-- Select Box No --",
                Selected = string.IsNullOrEmpty(selectedBoxNo),
                Disabled = true
            });

            return lstProducts;
        }
        private List<SelectListItem> GetProduction_Id()
        {
            var lstProducts = new List<SelectListItem>();
            var currentdate = DateTime.Now.ToString("dd-MM-yyyy");

            lstProducts = _context.SaveProduction.Where(a => a.Packaging_Flag == 0 && a.SaveProduction_Date.Trim() == currentdate.Trim()).AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.Production_Id,
                Text = n.Production_Id
            }).Distinct().ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "-- Select Production_Id --",
                Selected = true,
                Disabled = true
            };

            lstProducts.Insert(0, defItem);
            return lstProducts;
        }
        [HttpGet]
        public IActionResult Create()
        {
            Packagings_List.Clear();
            SaveProduction_List.Clear();
            ProductionCapture_List.Clear();

            ViewBag.GetProduction_Id = GetProduction_Id();
            ViewBag.GetBoxNos = GetBoxNos();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public string GetReciptIdFUN()
        {
            var currentYearMonth = DateTime.Now.ToString("yyMM"); // Example: "2412" for Dec 2024
            string newBoxId;
            int newCounter;
            var lastBox = _context.ReceiptIds.Where(a => a.ProductionId.Trim().StartsWith("RPT"))
                .OrderByDescending(b => b.id)
                .FirstOrDefault();
            if (lastBox != null)
            {
                string lastYearMonth = lastBox.ProductionId.Substring(3, 4); // Extract characters 4-7 ("YYMM")
                int lastCounter = int.Parse(lastBox.ProductionId.Substring(7)); // Extract counter part
                if (lastYearMonth == currentYearMonth)
                {
                    newCounter = lastCounter + 1;
                }
                else
                {
                    newCounter = 1;
                }
            }
            else
            {
                newCounter = 1;
            }
            newBoxId = $"RPT{currentYearMonth}{newCounter:D2}"; // Format: STBYYMMCC
            var maxId = _context.ReceiptIds.Any() ? _context.ReceiptIds.Max(e => e.id) + 1 : 1;
            var newBoxEntry = new ReceiptIds
            {
                id = maxId,
                ProductionId = newBoxId,
                date = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
            };
            _context.ReceiptIds.Add(newBoxEntry);
            _context.SaveChanges();

            return newBoxId;
        }
        public async Task<IActionResult> Create(Packaging packaging)
        {
            try
            {
                if (SaveProduction_List.Count == 0)
                {
                    return Json(new { status = "error", msg = "Please do scan the stickers first then submit !" });
                }
                var GetReciptId = GetReciptIdFUN();
                var DATE = DateTime.Now.ToString("dd-MM-yyyy");
                var TIME = DateTime.Now.ToString("HH:mm");
                var currentuser1 = HttpContext.User;
                string username = currentuser1.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name).Value;

                ////////save logic
                var Packagings_List1 = Packagings_List.ToList();
                var SaveProduction_List1 = SaveProduction_List.ToList();
                var ProductionCapture_List1 = ProductionCapture_List.ToList();
                SaveProduction_List.ForEach(a =>
                {
                    a.Packaging_Date = DATE;
                    a.Packaging_Time = TIME;
                });
                _context.SaveProduction.UpdateRange(SaveProduction_List);
                _context.BoxMaster.Where(b => b.BoxNumber == SaveProduction_List1[0].Box_No).ExecuteUpdate(setters => setters.SetProperty(b => b.Use_Flag, 1));
                _context.SaveChanges();

                foreach (var item in Packagings_List)
                {
                    item.Id = _context.Packaging.Any() ? _context.Packaging.Max(e => e.Id) + 1 : 1;
                    item.sellingRs = item.sellingRs;
                    item.mrpRs = item.mrpRs;
                    item.Packaging_Date = DATE;
                    item.Packaging_Time = TIME;
                    item.Reciept_Id = GetReciptId;
                    item.user = username.ToString();
                    _context.Packaging.Add(item);

                    _context.SaveChanges();
                }
                ////////END SAVE LGIC

                DataTable table1 = new DataTable();
                table1.Columns.AddRange(new DataColumn[]
                {
                    new DataColumn("product_name", typeof(string)),
                    new DataColumn("qnty", typeof(string)),
                    new DataColumn("wt", typeof(string)),
                });


                var PID = Packagings_List.Select(a => a.Production_Id.Trim()).FirstOrDefault();
                var BOXNO = Packagings_List.Select(a => a.Box_No.Trim()).FirstOrDefault();
                var OUTLET = Packagings_List.Select(a => a.Outlet_Name.Trim()).FirstOrDefault(); ;

                // Generate QR Code
                string qrcode = Packagings_List.Select(a => a.Box_No.Trim()).FirstOrDefault() + "$" + GetReciptId;
                //string qrcode = Packagings_List.Select(a => a.Box_No.Trim()).FirstOrDefault();
                string image;
                using (MemoryStream ms = new MemoryStream())
                {
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrcode, QRCodeGenerator.ECCLevel.Q);
                    PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                    byte[] qrCodeBytes = qrCode.GetGraphic(20);
                    ms.Write(qrCodeBytes, 0, qrCodeBytes.Length);
                    image = Convert.ToBase64String(ms.ToArray());
                }
                string renderFormat = "PDF";
                string extension = "pdf";
                string mimetype = "application/pdf";
                using var report = new LocalReport();
                var groupedList = Packagings_List1
                                  .GroupBy(p => new { p.Box_No, p.Outlet_Name }) // Group by multiple fields
                                  .Select(g => new
                                  {
                                      BoxNo = g.Key.Box_No,
                                      OutletName = g.Key.Outlet_Name,
                                      Products = g.Select(p => new
                                      {
                                          p.Product_Name,
                                          p.Qty,
                                          p.TotalNetWg,
                                          p.TotalNetWg_Uom,
                                      }).ToList()
                                  }).ToList();
                string pdfUrl = "";

                Packagings_List1.ForEach(item => table1.Rows.Add(item.Product_Name, item.Qty, item.TotalNetWg + " " + item.TotalNetWg_Uom));

                var parameters = new[]
                    {
                        new ReportParameter("d1", DATE),
                        new ReportParameter("pid", PID),
                        new ReportParameter("qr", image),
                        new ReportParameter("ot", OUTLET),
                        new ReportParameter("bx", BOXNO),
                        new ReportParameter("rd1", GetReciptId),
                    };

                report.ReportPath = $"{this._webHostEnvironment.WebRootPath}\\Reports\\DispatchRPT.rdlc";
                report.SetParameters(parameters);
                ReportDataSource rds = new ReportDataSource("DataSet1", table1);
                report.DataSources.Add(rds);
                var pdf = report.Render(renderFormat);

                string folderPath = $"{this._webHostEnvironment.WebRootPath}\\Reports\\";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // ✅ Optional: Instead of deleting all PDFs, remove only files older than 24 hours
                foreach (string pdfFile in Directory.GetFiles(folderPath, "*.pdf"))
                {
                    //if (System.IO.File.GetCreationTime(pdfFile) < DateTime.Now.AddHours(-24))
                    //{
                    System.IO.File.Delete(pdfFile);
                    //}
                }

                string fileName = $"PackagingSlip_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = System.IO.Path.Combine(folderPath, fileName);
                System.IO.File.WriteAllBytes(filePath, pdf);
                pdfUrl = Url.Content("~/Reports/") + fileName;

                //foreach (var item in groupedList)
                //{
                //    BOXNO = item.BoxNo;
                //    OUTLET = item.OutletName;

                //    // ✅ Corrected: Joining multiple product names and values into single string
                //    string productNames = string.Join(", ", item.Products.Select(a => a.Product_Name));
                //    string quantities = string.Join(", ", item.Products.Select(a => a.Qty.ToString()));
                //    string weights = string.Join(", ", item.Products.Select(a => $"{a.TotalNetWg} {a.TotalNetWg_Uom}"));




                //}
                Packagings_List.Clear();
                SaveProduction_List.Clear();
                ProductionCapture_List.Clear();
                var data1 = new
                {
                    pdfUrl = pdfUrl,
                    redirectToUrl = Url.Action("Index", "Packagings"),
                    status = "success",
                    data = ""
                };
                return Json(data1);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }); // ✅ Fixed: success = false
            }
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var packaging = await _context.Packaging.FindAsync(id);
            if (packaging == null)
            {
                return NotFound();
            }
            return View(packaging);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Packaging packaging)
        {
            if (id != packaging.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(packaging);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PackagingExists(packaging.Id))
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
            return View(packaging);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var packaging = await _context.Packaging
                .FirstOrDefaultAsync(m => m.Id == id);
            if (packaging == null)
            {
                return NotFound();
            }

            return View(packaging);
        }
        private bool PackagingExists(int id)
        {
            return _context.Packaging.Any(e => e.Id == id);
        }

        //[HttpGet]
        //public IActionResult GetCakeList(string outletName)
        //{
        //    if (string.IsNullOrEmpty(outletName))
        //    {
        //        return Json(new { success = false, message = "Invalid outlet!" });
        //    }

        //    var cakeList = _context.ProductionCapture
        //        .Where(pc => pc.OutletName.Trim() == outletName.Trim())
        //        .Select(pc => new
        //        {
        //            ProductName = pc.ProductName,
        //            Qty = pc.TotalQty,
        //            ProductionDate = pc.Production_Date,
        //            PackagingStatus = _context.Packaging.Any(p => p.Outlet_Name.Trim() == outletName.Trim() && p.Product_Name.Trim() == pc.ProductName.Trim()) ? "Packed" : "Pending"
        //        })
        //        .ToList();

        //    return Json(new { success = true, data = cakeList });
        //}

        [HttpGet]
        public IActionResult GetCakeList(string outletName)
        {
            if (string.IsNullOrEmpty(outletName))
            {
                return Json(new { success = false, message = "Invalid outlet!" });
            }
            var date = DateTime.Now.ToString("dd-MM-yyyy");
            var plannedProducts = _context.ProductionCapture
                .Where(pc => pc.OutletName.Trim() == outletName.Trim() && pc.Production_Date.Trim() == date.Trim())
                .GroupBy(pc => new { pc.ProductName, pc.OutletName })
                .Select(g => new
                {
                    ProductName = g.Key.ProductName,
                    PlannedQty = g.Sum(pc => pc.TotalQty) // Total planned quantity
                })
                .ToList();

            //var producedProducts = _context.SaveProduction
            //                    .Where(sp => sp.SaveProduction_Date.Trim() == date.Trim())
            //    .GroupBy(sp => sp.ProductName.Trim())
            //    .Select(g => new
            //    {
            //        ProductName = g.Key,
            //        ProducedQty = g.Sum(sp => sp.Qty) // Total produced quantity
            //    })
            //    .ToList();

            // Filter only products where planned quantity > produced quantity
            //var remainingProducts = plannedProducts
            //    .Select(pp =>
            //    {
            //        var producedQty = producedProducts.FirstOrDefault(sp => sp.ProductName == pp.ProductName)?.ProducedQty ?? 0;
            //        var remainingQty = /*pp.PlannedQty - */producedQty;

            //        return new
            //        {
            //            ProductName = pp.ProductName,
            //            RemainingQty = remainingQty > 0 ? remainingQty : 0  // Ensure it doesn't go negative
            //        };
            //    })
            //    .Where(p => p.RemainingQty > 0) // Only include products that still have remaining quantity
            //    .ToList();

            var remainingProducts = _context.SaveProduction
                                .Where(sp => sp.SaveProduction_Date.Trim() == date.Trim() && sp.Box_No.Trim() == null)
                .GroupBy(sp => sp.ProductName.Trim())
                .Select(g => new
                {
                    ProductName = g.Key,
                    RemainingQty = g.Sum(sp => sp.Qty) // Total produced quantity
                })
                .ToList();

            return Json(new { success = true, data = remainingProducts });
        }
        public async Task<IActionResult> PackagingReceipt(string selectedDate = null)
        {
            DateTime today = DateTime.Now.Date;
            DateTime filterDate = string.IsNullOrEmpty(selectedDate) ? today : DateTime.ParseExact(selectedDate, "yyyy-MM-dd", null);

            var packaging = await _context.Packaging
                .Where(p => p.Packaging_Date == filterDate.ToString("dd-MM-yyyy"))
                .Select(p => new Packaging
                {
                    Reciept_Id = p.Reciept_Id,
                    Production_Id = p.Production_Id,
                    Outlet_Name = p.Outlet_Name
                })
                .Distinct()
                .ToListAsync();

            return View(packaging);
        }

        [HttpPost]
        public async Task<IActionResult> ReprintSticker([FromBody] List<string> receiptIds)
        {
            if (receiptIds == null || !receiptIds.Any())
            {
                return Json(new { success = false, message = "No receipt selected!" });
            }

            try
            {
                var packagingList = await _context.Packaging
                    .Where(p => receiptIds.Contains(p.Reciept_Id))
                    .ToListAsync();

                if (!packagingList.Any())
                {
                    return Json(new { success = false, message = "No matching receipts found!" });
                }

                string pdfUrl = GeneratePackagingSlip(packagingList);

                // Send to printer over LAN
                try
                {
                    string printerIpStr = _config["AppSettings:loc2_printer"];
                    using var client = new TcpClient();
                    await client.ConnectAsync(IPAddress.Parse(printerIpStr), 9100);

                    byte[] data = Encoding.ASCII.GetBytes(pdfUrl);
                    using var stream = client.GetStream();
                    await stream.WriteAsync(data, 0, data.Length);
                    await stream.FlushAsync();
                }
                catch (Exception ex)
                {
                    // Consider logging the exception
                }

                return Json(new { success = true, pdfUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private string GeneratePackagingSlip(List<Packaging> packagingList)
        {
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Reports");
            Directory.CreateDirectory(folderPath);

            string fileName = $"Reprint_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string filePath = Path.Combine(folderPath, fileName);

            using var report = new LocalReport();
            report.ReportPath = Path.Combine(_webHostEnvironment.WebRootPath, "Reports", "DispatchRPT.rdlc");

            var firstItem = packagingList.First();
            string boxNo = firstItem.Box_No;
            string receiptId = firstItem.Reciept_Id.Trim();
            string outletName = firstItem.Outlet_Name;
            string productionId = firstItem.Production_Id;

            // Generate QR code
            string qrCodeContent = $"{boxNo}${receiptId}";
            string base64QrImage;
            using (var ms = new MemoryStream())
            {
                var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(qrCodeContent, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrBytes = qrCode.GetGraphic(20);
                ms.Write(qrBytes, 0, qrBytes.Length);
                base64QrImage = Convert.ToBase64String(ms.ToArray());
            }

            // Prepare DataTable
            DataTable table = new DataTable();
            table.Columns.AddRange(new[]
            {
        new DataColumn("product_name", typeof(string)),
        new DataColumn("qnty", typeof(string)),
        new DataColumn("wt", typeof(string))
    });

            packagingList.ForEach(item =>
                table.Rows.Add(item.Product_Name, item.Qty, $"{item.TotalNetWg} {item.TotalNetWg_Uom}"));

            // Set parameters and generate report
            var parameters = new[]
            {
        new ReportParameter("d1", date),
        new ReportParameter("pid", productionId),
        new ReportParameter("qr", base64QrImage),
        new ReportParameter("ot", outletName),
        new ReportParameter("bx", boxNo),
        new ReportParameter("rd1", receiptId)
    };

            report.SetParameters(parameters);
            report.DataSources.Add(new ReportDataSource("DataSet1", table));
            byte[] pdfBytes = report.Render("PDF");
            System.IO.File.WriteAllBytes(filePath, pdfBytes);

            return Url.Content("~/Reports/" + fileName);
        }

    }
}
