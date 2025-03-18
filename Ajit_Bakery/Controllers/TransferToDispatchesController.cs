using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ajit_Bakery.Data;
using Ajit_Bakery.Models;
using System.Drawing.Printing;
using System.Globalization;
using ZXing.OneD;
using AspNetCoreHero.ToastNotification.Notyf;
using Microsoft.AspNetCore.Hosting;
using System.Configuration;
using iTextSharp.text.pdf;
using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
using iText.Layout.Font;
using Font = System.Drawing.Font;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using iText.Kernel.Font;
//using iText.IO.Font;
using PdfFont = iText.Kernel.Font.PdfFont;

namespace Ajit_Bakery.Controllers
{
    public class TransferToDispatchesController : Controller
    {
        private readonly DataDBContext _context;
        private readonly IConfiguration _configuration;
        private IWebHostEnvironment _webHostEnvironment;
        public TransferToDispatchesController(DataDBContext context, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }
        public class RepackingRequest
        {
            //Production_Id: Production_Id, DCNo: DCNo, Box_No: Box_No, Reciept_Id: Reciept_Id   
            public string Production_Id { get; set; }
            public string DCNo { get; set; }
            public string Box_No { get; set; }
            public string Reciept_Id { get; set; }
        }

        //[HttpPost]

        //public IActionResult GenerateDCPdf([FromBody] RepackingRequest request)
        //{
        //    try
        //    {
        //        var path = _configuration["AppSettings:TakePath"];

        //        //logo
        //        string imagePath = Path.Combine(path, "img", "logo.png");
        //        if (!System.IO.File.Exists(imagePath))
        //        {
        //            return BadRequest(new { error = "Image file not found at path: " + imagePath });
        //        }
        //        byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
        //        string logoImgTag = Convert.ToBase64String(imageBytes);
        //        //end

        //        //signatur
        //        string imagePath1 = Path.Combine(path, "img", "sign.png");
        //        if (!System.IO.File.Exists(imagePath1))
        //        {
        //            return BadRequest(new { error = "Image file not found at path: " + imagePath1 });
        //        }

        //        byte[] imageBytes1 = System.IO.File.ReadAllBytes(imagePath1);
        //        string signatureImgTag = Convert.ToBase64String(imageBytes1);
        //        //end


        //        string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Fonts", "calibri-bold.ttf");
        //        PdfFont font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);
        //        var address = "";
        //        var CUSTOMER_NAME = "";
        //        var CONTACT_PERSON = "";
        //        var newmail = "";
        //        var CONTACT_MOBILE = "";
        //        var REMARKS = "";

        //        var ADD1 = "";
        //        var ADD2 = "";
        //        var ADD3 = "";
        //        string fullAddress = "";
        //        if (address.Length <= 50)
        //        {
        //            ADD1 = address;
        //            ADD2 = "";
        //            ADD3 = "";
        //        }
        //        else if (address.Length <= 100)
        //        {
        //            ADD1 = address.Substring(0, 50);
        //            ADD2 = address.Substring(50) + " .";
        //            ADD3 = "";
        //        }
        //        else
        //        {
        //            ADD1 = address.Substring(0, 50);
        //            ADD2 = address.Substring(50, 50);
        //            ADD3 = address.Substring(100) + " .";
        //        }

        //        var basicHeaderHtml = $@"
        //                <div style='display: flex; justify-content: space-between; align-items: flex-start;margin-right:8px;'>
        //                <div style='text-align: center;margin-right:8px;'>
        //                <div style='text-align: center;margin-right:8px;'><img src='data:image/png;base64,{logoImgTag}'  alt='Company Logo' style='width: 78px; height: 60px; margin: auto; display: block;' /></div>
        //                <h4 style='margin: 0;margin-top:0px; padding: 0; color: #2c758c; text-align: center; font-weight: bold;font-family: Calibri, Arial, sans-serif; '>AARKAY TECHNO</h4>
        //                <h4 style='margin: 0;margin-top:0px; padding: 1px 0;  color: #2c758c; text-align:center; font-weight: bold;font-family: Calibri, Arial, sans-serif; '>CONSULTANTS PVT.LTD.</h4>
        //                <p style='margin: 0;margin-top:0px; padding: 5px 0; background-color: #ec7c34; color: white; text-align:center; font-size: 8pt; font-weight: bold;width:200px;margin-right:8px;'>A TOTAL IT SOLUTIONS COMPANY</p>
        //                </div>
        //                <div style='text-align: right; font-size: 10pt; margin: 0;margin-top: 0px; padding: 0; line-height: 1;'>
        //                <p style='margin: 0; padding: 0; line-height: 1;font-family: Calibri, Arial, sans-serif; '>First floor,1,Samarth Nagar(W),Ajni Square,Wardha Road,Nagpur-440015,India.</p>
        //                <p style='margin: 0; padding: 0; line-height: 1;font-family: Calibri, Arial, sans-serif; '>Tel: +91 (712) 2252443 / 2251696; Mobile: 9764440738, 9764440734.</p>
        //                <p style='margin: 0; padding: 0; line-height: 1;font-family: Calibri, Arial, sans-serif; '>GSTN: 27AABCA9111E1Z3; PAN: AABCA9111E</p>
        //                <p style='margin: 0; padding: 0; line-height: 1;font-family: Calibri, Arial, sans-serif; '>CIN: U30007MH1997PTC112414</p>
        //                <p style='margin: 0; padding: 0; line-height: 1;font-family: Calibri, Arial, sans-serif; '>URN: UDYAM-MH-20-0013989</p>
        //                <p style='margin: 0; padding: 0; line-height: 1;font-family: Calibri, Arial, sans-serif; '><a href='http://www.atcgroup.co.in'>www.atcgroup.co.in</a>, <a href='mailto:info@atcgroup.co.in'>info@atcgroup.co.in</a></p>
        //                <svg style=' margin-top: 15px; margin-bottom: 0px;' height='50' width='450'  xmlns='http://www.w3.org/2000/svg'>
        //                <line x1='590' y1='18' x2='60' y2='18' style='stroke:#2c758c;stroke-width:10' />
        //                <line x1='590' y1='21' x2='50' y2='21' style='stroke:#808080;stroke-width:4' />
        //                </svg>
        //                </div>
        //                </div>
        //                <div style='clear: both; text-align: center; margin-top: 0px;'>

        //                </div>";


        //        string htmlTemplate = $@"<!DOCTYPE html>
        //    <html lang='en'>
        //    <head>
        //        <meta charset='UTF-8'>
        //        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        //        <title>Delivery Challan</title>
        //        <style>
        //            @font-face {{
        //                        font-family: 'Calibri';
        //                        src: url('file://{fontPath}') format('truetype');}}
        //             body {{ font-family: Arial, sans-serif; padding: 15px; }}
        //            .header {{ text-align: center; }}
        //            .company-logo {{ width: 200px; }}
        //            .bordered-box {{ border: 1px solid black; padding: 5px; }}
        //            .highlight {{ font-weight: bold; color: #000; }}
        //            .table {{ width: 100%; border-collapse: collapse; margin-top: 3px; }}
        //            .table, .table th, .table td {{ border: 1px solid black; padding: 4px; text-align: center; }}
        //            .remarks {{ background-color: #f8f9fa; padding: 7px; font-weight: bold; }}
        //            .signature-section {{ margin-top: 40px; display: flex; justify-content: space-between; }}
        //            .signature {{ text-align: center; width: 30%; border-top: 1px solid black; padding-top: 5px; }}
        //        </style>
        //    </head>
        //    <body>
        //        <div class='header'>
        //                            {basicHeaderHtml}
        //                        </div>

        //        <div style='display: flex; justify-content: space-between; width: 100%; margin-bottom: 10px; margin-top:5px;'>
        //            <div style='flex: 1; text-align: left; font-weight: bold;'>
        //                Ref: <span class='bordered-box'>{request.Production_Id}</span>
        //            </div>
        //            <div style='flex: 1; text-align: right; font-weight: bold;'>
        //                Date: <span class='bordered-box'>{DateTime.Now:dd/MM/yyyy}</span>
        //            </div>
        //        </div>

        //        <h4 style='text-align:center; text-decoration:underline; font-weight:bold;'>DELIVERY CHALLAN - SALE</h4>

        //        <div class='bordered-box'>
        //            <h3>To, <br><span class='highlight'>{CUSTOMER_NAME}</span></h3>
        //            <p>{ADD1},<br>{ADD2}<br>{ADD3}</p>
        //            <p><strong>Contact Person: </strong>{CONTACT_PERSON}</p>
        //            <p><strong>Mob: </strong>{CONTACT_MOBILE}</p>
        //            <p><strong>Email: </strong>{newmail}</p>
        //        </div>
        //         <table class='table'>
        //            <thead>
        //                <tr style='background-color:#f0f0f0;'>
        //                    <th>Product Code</th>
        //                    <th>Product Name</th>
        //                    <th>Picking Qty</th>
        //                </tr>
        //            </thead>
        //            <tbody>
        //            </tbody>
        //            <tfoot>
        //                <tr style='font-weight:bold;'>
        //                    <td colspan='2' style='text-align:right;'>Total Qty: </td>
        //                </tr>
        //            </tfoot>
        //        </table>

        //        <div class='remarks'>
        //            <strong>Remarks: </strong>{REMARKS}
        //        </div>

        //        <p>Note: Material was handed over in good condition with proper packaging.<br>This is a system-generated document.</p>

        //        <div style='display: flex; justify-content: space-between; align-items: flex-end; width: 100%; margin-top: 40px;'>
        //                            <div style='text-align: left;margin-right:280px;'>
        //                                <p style='margin: 0;'>Receiver's Signature</p>
        //                            </div>
        //                            <div style='text-align: right;'>
        //                                <img src='data:image/png;base64,{signatureImgTag}' width='120'' style='display: block; margin-bottom: 5px;margin-left: 50px;' />
        //                                <p style='margin: 0;'>Aarkay Techno Consultants Pvt. Ltd.</p>
        //                            </div>
        //                        </div>

        //    </body>
        //    </html>";

        //        System.Diagnostics.Debug.WriteLine(htmlTemplate); // Debugging

        //        byte[] pdfBytes;
        //        using (MemoryStream outputStream = new MemoryStream())
        //        {
        //            PdfWriter writer = new PdfWriter(outputStream);
        //            PdfDocument pdfDocument = new PdfDocument(writer);
        //            ConverterProperties converterProperties = new ConverterProperties();
        //            FontProvider fontProvider = new DefaultFontProvider(false, false, false);
        //            fontProvider.AddFont(fontPath, PdfEncodings.IDENTITY_H);
        //            converterProperties.SetFontProvider(fontProvider);
        //            HtmlConverter.ConvertToPdf(htmlTemplate, pdfDocument, converterProperties);
        //            pdfBytes = outputStream.ToArray();
        //        }

        //        return File(pdfBytes, "application/pdf", $"DeliveryChallan_{request.Production_Id}.pdf");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { error = ex.Message });
        //    }
        //}


        private static List<Packaging> Packagings_List = new List<Packaging>();

        public IActionResult PickedData(string boxno, string receiptno)
        {
            if (Packagings_List.Count > 0)
            {
                var recipt_outlet = _context.Packaging.Where(a=>a.Reciept_Id == receiptno.Trim()).Select(a=>a.Outlet_Name.Trim()).FirstOrDefault();
                if(recipt_outlet != null)
                {
                    var exist_outlet = Packagings_List.Where(a => a.Reciept_Id == receiptno.Trim()).Select(a => a.Outlet_Name.Trim()).FirstOrDefault() ?? "NA";

                    if(exist_outlet != recipt_outlet)
                    {
                        if(exist_outlet == "NA")
                        {
                            exist_outlet = "Outlet";
                        }
                        return Json(new { success = false, message = $"Please scan correct item box barcode againts {exist_outlet} ,of receipt id no.: {receiptno} !" });
                    }
                }

                if (Packagings_List.Any(a => a.Reciept_Id.Trim() == receiptno.Trim()))
                {
                    return Json(new { success = false, message = $"You have already scanned items against receipt ID no.: {receiptno}!" });
                }
            }
            

            // ✅ Avoid duplicate queries and only store necessary data
            var newItems = _context.Packaging
                .Where(a => a.Reciept_Id.Trim() == receiptno.Trim() && a.Box_No.Trim() == boxno.Trim())
                .ToList();

            if(newItems.Count > 0)
            {

                var newitem1 = newItems.Where(a => a.DispatchReady_Flag == 0).ToList();
                if (newitem1.Count > 0)
                {
                    var packingfoun = _context.Packaging
                            .Where(a => a.Reciept_Id.Trim() == receiptno.Trim() && a.Box_No.Trim() == boxno.Trim() && a.DispatchReady_Flag == 0)
                            .Select(a => new
                            {
                                a.Production_Id,
                                a.Outlet_Name, // Ensure this field exists in Packaging table
                                a.Box_No,
                                a.Product_Name, // Ensure these fields exist
                                a.Qty,
                                TotalNetWg = a.TotalNetWg, // Ensure nullable fields are handled
                                TotalNetWg_Uom = a.TotalNetWg_Uom ?? "-"
                            })
                            .ToList();


                    if (packingfoun.Count > 0)
                    {
                        Packagings_List.AddRange(newItems);

                        return Json(new { success = true, data = packingfoun, qtypick = Packagings_List.Count });
                    }
                }
                else
                {
                    return Json(new { success = false, message = $"You have already scan all items against receipt id no.: {receiptno}!" });
                }
            }

            return Json(new { success = false, message = $"Data not found against receipt ID: {receiptno}!" });
        }

        public async Task<IActionResult> Index()
        {
            Packagings_List.Clear();

            var list = _context.Packaging
                .Where(a => a.DispatchReady_Flag == 1)
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
            foreach(var item in list)
            {
                var check = _context.ProductMaster.Where(a => a.ProductName.Trim() == item.Product_Name.Trim()).FirstOrDefault();
                if (check != null)
                {
                    item.Category = check.Type;
                }
            }
            return View(list);
        }



        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TransferToDispatch == null)
            {
                return NotFound();
            }

            var transferToDispatch = await _context.TransferToDispatch
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transferToDispatch == null)
            {
                return NotFound();
            }

            return View(transferToDispatch);
        }

        public IActionResult Create()
        {
            return View();
        }

        public string GetDCNO()
        {
            var currentYearMonth = DateTime.Now.ToString("yyMM"); // Example: "2412" for Dec 2024
            string newBoxId;
            int newCounter;
            var lastBox = _context.ReceiptIds.Where(a => a.ProductionId.Trim().StartsWith("DC"))
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
            newBoxId = $"DC{currentYearMonth}{newCounter:D2}"; // Format: STBYYMMCC
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( TransferToDispatch transferToDispatch)
        {
            if (Packagings_List.Count > 0)
            {
                var DATE = DateTime.Now.ToString("dd-MM-yyyy");
                var TIME = DateTime.Now.ToString("HH:mm");
                var DNO = GetDCNO();
                Packagings_List.ForEach(a =>
                {
                    a.DispatchReady_Flag = 1;
                    a.DCNo = DNO;
                    a.DispatchReady_Date =DATE;
                    a.DispatchReady_Time =TIME;
                });
                _context.Packaging.UpdateRange(Packagings_List);
                _context.SaveChanges();
                return Json(new { success = true, message = "Successfully Done !" });
            }
            return Json(new { success = false, message = "Please do scan the receipt barcode first against the production id !" });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TransferToDispatch == null)
            {
                return NotFound();
            }

            var transferToDispatch = await _context.TransferToDispatch.FindAsync(id);
            if (transferToDispatch == null)
            {
                return NotFound();
            }
            return View(transferToDispatch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TransferToDispatch transferToDispatch)
        {
            if (id != transferToDispatch.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transferToDispatch);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransferToDispatchExists(transferToDispatch.Id))
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
            return View(transferToDispatch);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TransferToDispatch == null)
            {
                return NotFound();
            }

            var transferToDispatch = await _context.TransferToDispatch
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transferToDispatch == null)
            {
                return NotFound();
            }

            return View(transferToDispatch);
        }


        private bool TransferToDispatchExists(int id)
        {
          return (_context.TransferToDispatch?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
