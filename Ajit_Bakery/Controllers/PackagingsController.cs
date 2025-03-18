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
//using AspNetCore.Reporting.ReportExecutionService;

namespace Ajit_Bakery.Controllers
{
    public class PackagingsController : Controller
    {
        private readonly DataDBContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PackagingsController(DataDBContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        public async Task<IActionResult> Index()
        {
            Packagings_List.Clear();
            SaveProduction_List.Clear();
            ProductionCapture_List.Clear();
            var list = await _context.Packaging.OrderByDescending(a => a.Id).ToListAsync();
            foreach (var item in list)
            {
                item.TotalNetWg_Uom = item.TotalNetWg + " " + item.TotalNetWg_Uom;
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
                        .FirstOrDefault(a => !SaveProduction_List.Any(p => p.Id == a.Id));

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
                            Production_Id = Production_Id,
                            Box_No = Box_No,
                            Outlet_Name = outletName,
                            Qty = 1,
                            TotalNetWg = wg,
                            TotalNetWg_Uom = checkagain.TotalNetWg_Uom,
                            Exp_Dt = checkagain.Exp_Date,
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


            var Packagingdata = _context.Packaging.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Outlet_Name.Trim() == Outlet_Name.Trim()).ToList().Sum(a => a.Qty);
            var Packagingdata1 = Packagings_List.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Outlet_Name.Trim() == Outlet_Name.Trim()).ToList().Sum(a => a.Qty);

            var ProductionCapturedata = _context.ProductionCapture.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Status.Trim() == "Pending" && a.OutletName.Trim() == Outlet_Name.Trim()).ToList().Sum(a => a.TotalQty);
            var ProductionCapturedata1 = ProductionCapture_List.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Status.Trim() == "Pending" && a.OutletName.Trim() == Outlet_Name.Trim()).ToList().Sum(a => a.TotalQty);

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
            return Json(new { success = false, message = "You have already picked all qty against " + Outlet_Name + "  outlet !" });
        }
        public IActionResult GetOutlets(string Production_Id)
        {
            var lstProducts = new List<SelectListItem>();

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
            //var check = _context.Packaging.Where(a=>a.Production_Id.Trim() == Production_Id.Trim()).FirstOrDefault();
            //var qtyremainig = ;
            //var qtypick = ;
            //, qtypick = qtypick , qtyremainig = qtyremainig
            return Json(new { success = true, data = lstProducts });
        }
        private List<SelectListItem> GetBoxNos()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.BoxMaster.AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.BoxNumber,
                Text = n.BoxNumber
            }).Distinct().ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select Box No ----"
            };

            lstProducts.Insert(0, defItem);
            return lstProducts;
        }
        private List<SelectListItem> GetProduction_Id()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.SaveProduction.Where(a => a.Packaging_Flag == 0).AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.Production_Id,
                Text = n.Production_Id
            }).Distinct().ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select Production_Id ----"
            };

            lstProducts.Insert(0, defItem);
            return lstProducts;
        }
        [HttpGet]
        public IActionResult Create()
        {
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
                var GetReciptId = GetReciptIdFUN();
                var DATE = DateTime.Now.ToString("dd-MM-yyyy");
                var TIME = DateTime.Now.ToString("HH:mm");
                
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
                _context.SaveChanges();

                foreach (var item in Packagings_List)
                {
                    item.Id = _context.Packaging.Any() ? _context.Packaging.Max(e => e.Id) + 1 : 1;
                    item.Packaging_Date = DATE;
                    item.Packaging_Time = TIME;
                    item.Reciept_Id = GetReciptId;
                    _context.Packaging.Add(item);
                    _context.SaveChanges();
                }
                ////////END SAVE LGIC

                DataTable table1 = new DataTable();
                table1.Columns.AddRange(new DataColumn[]
                {
                    new DataColumn("product_name", typeof(string)),
                    new DataColumn("qnty", typeof(string)),
                    new DataColumn("wt", typeof(string))
                });


                var PID = Packagings_List.Select(a => a.Production_Id.Trim()).FirstOrDefault();
                var BOXNO = Packagings_List.Select(a => a.Box_No.Trim()).FirstOrDefault();
                var OUTLET = Packagings_List.Select(a => a.Outlet_Name.Trim()).FirstOrDefault(); ;

                // Generate QR Code
                string qrcode = Packagings_List.Select(a => a.Box_No.Trim()).FirstOrDefault() + "$"+ GetReciptId;
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
    }
}
