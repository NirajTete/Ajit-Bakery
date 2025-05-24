using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ajit_Bakery.Data;
using Ajit_Bakery.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Security.Claims;
using Ajit_Bakery.Models.Tally_Models;
using Ajit_Bakery.Services;
using AspNetCoreHero.ToastNotification.Abstractions;
using iText.Html2pdf.Attach;
using PdfSharp.Snippets;
using System.Diagnostics.Metrics;
using Ajit_Bakery.Models.Tally_Models;
using TallyERPWebApi.Model;
using NuGet.Versioning;

namespace Ajit_Bakery.Controllers
{
    [Authorize]
    public class DispatchesController : Controller
    {
        private readonly DataDBContext _context;
        public INotyfService _notyfService { get; }
        private readonly IConfiguration _configuration;
        private IWebHostEnvironment _webHostEnvironment;
        private readonly IApiService _apiService;
        private readonly IConfiguration _config;
        public DispatchesController(DataDBContext context, INotyfService notyfService, IWebHostEnvironment webHostEnvironment, IConfiguration configuration, IApiService apiService, IConfiguration config)
        {
            _context = context;
            _notyfService = notyfService;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _apiService = apiService;
            _config = config;
        }

        private static List<Packaging> Packagings_List = new List<Packaging>();
        public IActionResult GetOutlets(string Production_Id)
        {
            List<DialDetailViewModel> DialDetailViewModellist = new List<DialDetailViewModel>();
            if(Production_Id != null)
            {
                var date = DateTime.Now.ToString("dd-MM-yyyy");
                var list = _context.ProductionCapture.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Status == "Pending" && a.Production_Date.Trim() == date.Trim()).ToList();
                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        var data = _context.ProductionCapture
                            .Where(a => a.ProductName.Trim() == item.ProductName.Trim() && a.Production_Id.Trim() == Production_Id.Trim() && a.OutletName.Trim() == item.OutletName.Trim() && a.Production_Date.Trim() == date.Trim())
                            .Sum(a => a.TotalQty);

                        var savedata = _context.Packaging
                            .Where(a => a.Product_Name.Trim() == item.ProductName.Trim() && a.Production_Id.Trim() == Production_Id.Trim() && a.DispatchReady_Flag == 1 && a.Dispatch_Flag == 0 && a.Outlet_Name.Trim() == item.OutletName.Trim() && a.Packaging_Date.Trim() == date.Trim())
                            .Sum(a => a.Qty);

                        var savedata1 = _context.Packaging
                            .Where(a => a.Product_Name.Trim() == item.ProductName.Trim() && a.Production_Id.Trim() == Production_Id.Trim() && a.DispatchReady_Flag == 1 && a.Dispatch_Flag == 1 && a.Outlet_Name.Trim() == item.OutletName.Trim() && a.Packaging_Date.Trim() == date.Trim())
                            .Sum(a => a.Qty);

                        int PendingQty = 0;

                        if (data == savedata1)
                        {
                            PendingQty = Math.Abs(data - savedata1);
                        }
                        else
                        {
                            PendingQty = Math.Abs(data - savedata);
                        }
                        int DispatchReady = Math.Abs(savedata);


                        //var found = _context.ProductMaster.Where(a => a.ProductName.Trim() == item.ProductName.Trim()).FirstOrDefault();
                        //if (found != null)
                        //{
                        DialDetailViewModel DialDetailViewModel = new DialDetailViewModel()
                        {
                            ProductName = item.ProductName,
                            TotalQty = item.TotalQty,
                            OutletName = item.OutletName,
                            PendingQty = PendingQty,
                            DispatchReady = DispatchReady,
                        };
                        DialDetailViewModellist.Add(DialDetailViewModel);
                        //}
                    }
                }

                var outlet_list = _context.Packaging.Where(a => a.DispatchReady_Flag == 1 && a.Dispatch_Flag == 0 && a.Production_Id.Trim() == Production_Id.Trim() && a.Packaging_Date.Trim() == date.Trim()).Select(a => a.Outlet_Name.Trim()).Distinct().ToList();

                return Json(new { success = true, TableData = DialDetailViewModellist.Where(a=>a.DispatchReady > 0), outlet_list });
            }
            return Json(new {success = false, message = "Production id is found null !"} );
        }

        public IActionResult PickedData(string boxno, string receiptno, string dcno)
        {
            if (Packagings_List.Count > 0)
            {
                var recipt_outlet = _context.Packaging.Where(a => a.Reciept_Id == receiptno.Trim() && a.DispatchReady_Flag == 1 && a.Outlet_Name.Trim() == dcno.Trim()).Select(a => a.Outlet_Name.Trim()).FirstOrDefault();
                if (recipt_outlet != null)
                {
                    var getprodictionid = Packagings_List.Select(a => a.Production_Id).FirstOrDefault();
                    var exist_outlet = Packagings_List.Where(a => a.Production_Id.Trim() == getprodictionid.Trim() /*.Reciept_Id == receiptno.Trim()*/).Select(a => a.Outlet_Name.Trim()).FirstOrDefault() ?? "NA";
                    if (exist_outlet != recipt_outlet)
                    {
                        if (exist_outlet == "NA")
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
            var newItems = _context.Packaging
                .Where(a => a.Reciept_Id.Trim() == receiptno.Trim() && a.Box_No.Trim() == boxno.Trim() && a.DispatchReady_Flag == 1 && a.Outlet_Name.Trim() == dcno.Trim())
                .ToList();
            if (newItems.Count > 0)
            {
                var newitem1 = newItems.Where(a => a.DispatchReady_Flag == 1).ToList();
                if (newitem1.Count > 0)
                {
                    var packingfoun = _context.Packaging
                            .Where(a => a.Reciept_Id.Trim() == receiptno.Trim() && a.Box_No.Trim() == boxno.Trim() && a.DispatchReady_Flag == 1 && a.Outlet_Name.Trim() == dcno.Trim())
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

                        var totalInDb = _context.Packaging
                            .Where(a => a.DispatchReady_Flag == 1 && a.Outlet_Name.Trim() == dcno.Trim())
                            .Sum(a => (int?)a.Qty) ?? 0; // Summing directly in DB, handles empty case

                        // Get total dispatched quantity from Dispatch table for the same outlet
                        var dispatchedQty = _context.Dispatch
                            .Where(d => d.OutletName.Trim() == dcno.Trim())
                            .Sum(d => (int?)d.Qty) ?? 0;

                        var totalInList = Packagings_List.Sum(a => a.Qty); // Sum of in-memory list
                        var remainingQty = totalInDb - (totalInList + dispatchedQty); // Calculate remaining quantity
                        return Json(new { success = true, data = packingfoun, qtypick = remainingQty });

                        //var listr = _context.Packaging
                        //    .Where(a => a.DispatchReady_Flag == 1 
                        //    && a.DCNo.Trim() == dcno.Trim()).ToList();
                        //var lisrw = Packagings_List.Sum(a => a.Qty);
                        //var valuefound = listr - lisrw;
                        //return Json(new { success = true, data = packingfoun, qtypick = valuefound });
                    }
                }
                else
                {
                    return Json(new { success = false, message = $"You have already scan all items against receipt id no.: {receiptno}!" });
                }
            }
            return Json(new { success = false, message = $"Data not found against receipt ID: {receiptno}!" });
        }
        public IActionResult GetDCData(string dcno)
        {
            var getcount = 0;

            if (!string.IsNullOrEmpty(dcno))
            {

                var datetime = DateTime.Now.ToString("dd-MM-yyyy");

                // Get total packaged quantity from Packaging table where DispatchReady_Flag == 1
                getcount = _context.Packaging
                    .Where(a => a.DispatchReady_Flag == 1 && a.Outlet_Name.Trim() == dcno.Trim() && a.Packaging_Date.Trim() == datetime.Trim())
                    .Sum(a => (int?)a.Qty) ?? 0;

                // Get total dispatched quantity from Dispatch table for the same outlet
                var dispatchedQty = _context.Dispatch
                    .Where(d => d.OutletName.Trim() == dcno.Trim())
                    .Sum(d => (int?)d.Qty) ?? 0;

                // Subtract dispatched quantity from the total packaged quantity
                getcount -= dispatchedQty;

                // Ensure count doesn't go negative
                if (getcount < 0) getcount = 0;

                // Debugging logs
                Console.WriteLine($"Outlet: {dcno}, Packaged Qty: {getcount + dispatchedQty}, Dispatched Qty: {dispatchedQty}, Remaining: {getcount}");
            }

            return Json(new { success = true, data = getcount });
        }

        public IActionResult GetDriverData(string VehicleDriverName)
        {
            var drivercontactno = "";
            var drivervehicaleno = "";
            if(VehicleDriverName !=null || VehicleDriverName != "")
            {
                var data = _context.TransportMaster.Where(a => a.DriverName.Trim() == VehicleDriverName.Trim()).FirstOrDefault();
                if (data != null)
                {
                    drivercontactno = data.DriverContactNo;
                    drivervehicaleno = data.VehicleNo;
                }
            }
            
            return Json(new { success = true, drivercontactno, drivervehicaleno });
        }
        private List<SelectListItem> GetDCNO()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.Packaging.Where(a => a.DispatchReady_Flag == 1 && a.Dispatch_Flag == 0).AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.DCNo,
                Text = n.DCNo
            }).Distinct().ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "-- Select DCNo --",
                Selected = true,
                Disabled = true
            };

            lstProducts.Insert(0, defItem);
            return lstProducts;
        }
        private List<SelectListItem> GetProduction_Id()
        {
            var lstProducts = new List<SelectListItem>();
            var currentdate = DateTime.Now.ToString("dd-MM-yyyy");
            lstProducts = _context.Packaging.Where(a => a.DispatchReady_Flag == 1 && a.Packaging_Date == currentdate.Trim()).AsNoTracking().Select(n =>
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
        public IActionResult GetVehicleOwn(string VehicleOwn)
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.TransportMaster.Where(a => a.VehicleOwn.Trim() == VehicleOwn.Trim()).AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.DriverName,
                Text = n.DriverName
            }).Distinct().ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "-- Select DriverName --",
                Selected = true,
                Disabled = true
            };

            lstProducts.Insert(0, defItem);
            return Json(new {sucess = true, data = lstProducts});

        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Packagings_List.Clear();
            var date = DateTime.Now.ToString("dd-MM-yyyy");
            //var LIST = await _context.SaveProduction.Where(a => a.Qty > 0 && a.SaveProduction_Date.Trim() == date.Trim()).OrderByDescending(a => a.Id).ToListAsync();
            var data = _context.Dispatch.OrderByDescending(a => a.Id).ToList();
            var list = data
                .Where(a => a.Dispatch_Date.Trim() == date.Trim())
                .AsEnumerable()  // Fetch data first, then perform grouping in memory
                .GroupBy(a => new
                {
                    a.ProductionId,
                    a.DCNo,
                    a.OutletName,
                    a.BoxNo,
                    a.ReceiptNo,
                    a.ProductName,
                    a.category,
                    a.Status,
                })
                .Select(g => new Dispatch
                {
                    ProductionId = g.Key.ProductionId,
                    DCNo = g.Key.DCNo,
                    OutletName = g.Key.OutletName,
                    BoxNo = g.Key.BoxNo,
                    ReceiptNo = g.Key.ReceiptNo,
                    ProductName = g.Key.ProductName,
                    category = g.Key.category, // Ensure it's available in memory
                    Status = g.Key.Status, // Ensure it's available in memory
                    Qty = g.Sum(a => a.Qty) // Summing the Quantity
                })
                .ToList();
            foreach (var item in list)
            {
                var check = _context.ProductMaster.Where(a => a.ProductName.Trim() == item.ProductName.Trim()).FirstOrDefault();
                if (check != null)
                {
                    item.category = check.Type;
                }
            }
            return View(list);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Dispatch == null)
            {
                return NotFound();
            }

            var dispatch = await _context.Dispatch
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dispatch == null)
            {
                return NotFound();
            }

            return View(dispatch);
        }
        //private async Task<List<SelectListItem>> GetLedgerTypes()
        //{
        //    var baseurl = _config["AppSettings:BaseUrl"];
        //    var url1 = $"{baseurl}/Vouchers/GetVoucherTypeData";

        //    var result = await _apiService.GetAsync<ApiResponse<List<getVouchers>>>(url1, null);
        //    var ledgers = result?.Data ?? new List<getVouchers>();

        //    var voucherTypeList = ledgers
        //        .Where(a => a.parent == "Sales")
        //        .Select(a => new SelectListItem
        //        {
        //            Text = a.vouchername.Trim(),
        //            Value = a.vouchername.Trim()
        //        })
        //        .Distinct()
        //        .ToList();
        //    return (voucherTypeList);
        //}

        public async Task<IActionResult> Create()
        {
            //TALLY ADDED
            try
            {
                Packagings_List.Clear();

                var baseurls = _config["AppSettings:BaseUrl"];
                var Cname = _config["AppSettings:CompanyName"];

                var tallyStatusUrl = $"{baseurls}/GetStatus";
                var companyUrl = $"{baseurls}/Company";

                // Check if Tally is running
                var tallyResponse = await _apiService.GetAsync<ApiResponse<List<string>>>(tallyStatusUrl, null);

                // Check if Company is available
                var companyResponse = await _apiService.GetAsync<ApiResponse<List<string>>>(companyUrl, null);

                if (tallyResponse == null || !tallyResponse.Success)
                {
                    _notyfService.Error("Tally Server is not running!");
                    return RedirectToAction(nameof(Index));  // Corrected return
                }

                if (companyResponse == null || !companyResponse.Success)
                {
                    _notyfService.Warning("Tally is running, but the Company is not selected or available.");
                    return RedirectToAction(nameof(Index));  // Corrected return
                }

                if (!companyResponse.Data.Contains(Cname))
                {
                    _notyfService.Warning($"{Cname} Company is not Open.");
                    return RedirectToAction(nameof(Index));  // Corrected return
                }

                //var voucherTypes = await GetLedgerTypes();
                //ViewBag.GetVoucherType = voucherTypes;

                var (voucherTypes, LedgerTypes) = await GetLedgerTypes();
                ViewBag.GetVoucherType = voucherTypes;
                ViewBag.GetLedgerType = LedgerTypes;

            }
            catch (Exception ex)
            {
                _notyfService.Warning(" Tally Server is not running!!.");
                return RedirectToAction(nameof(Index));  // Ensure redirect even in case of an error
            }
            //ENDED
            ViewBag.GetProduction_Id = GetProduction_Id();
            return View();
        }
        private async Task<(List<SelectListItem> VoucherTypes, List<SelectListItem> LedgerTypes)> GetLedgerTypes()
        {
        
            var baseurl = _config["AppSettings:BaseUrl"];
            var url1 = $"{baseurl}/AllLedger";
            var result = await _apiService.GetAsync<ApiResponse<List<Ledger>>>(url1, null);
            var ledgers = result?.Data ?? new List<Ledger>();
            var LedgerTypeList = ledgers
                .Where(a => a.type == "Sales Accounts")
                .Select(a => new SelectListItem
                {
                    Text = a.name1.Trim(),
                    Value = a.name1.Trim()
                })
                .Distinct()
                .ToList();
         
            var baseurl1 = _config["AppSettings:BaseUrl"];
            var url11 = $"{baseurl}/Vouchers/GetVoucherTypeData";
            var result11 = await _apiService.GetAsync<ApiResponse<List<getVouchers>>>(url11, null);
            var ledgers11 = result11?.Data ?? new List<getVouchers>();
            var voucherTypeList = ledgers11
                .Where(a => a.parent == "Sales")
                .Select(a => new SelectListItem
                {
                    Text = a.vouchername.Trim(),
                    Value = a.vouchername.Trim()
                })
                .Distinct()
                .ToList();
            return (voucherTypeList, LedgerTypeList);
        }
        public string GetINNOSTR()
        {
            try
            {
                var currentYearMonth = DateTime.Now.ToString("yyMM"); // Example: "2412" for Dec 2024
                string newBoxId;
                int newCounter;
                var lastBox = _context.DCIds.Where(a => a.ProductionId.Trim().StartsWith("IN-"))
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
                newBoxId = $"IN-{currentYearMonth}{newCounter:D2}"; // Format: STBYYMMCC
                var maxId = _context.DCIds.Any() ? _context.DCIds.Max(e => e.id) + 1 : 1;
                var newBoxEntry = new DCIds
                {
                    id = maxId,
                    ProductionId = newBoxId,
                    date = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                };
                _context.DCIds.Add(newBoxEntry);
                _context.SaveChanges();

                return newBoxId;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string GetDCNOSTR()
        {
            try
            {
                var currentYearMonth = DateTime.Now.ToString("yyMM"); // Example: "2412" for Dec 2024
                string newBoxId;
                int newCounter;
                var lastBox = _context.DCIds.Where(a => a.ProductionId.Trim().StartsWith("DC-"))
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
                newBoxId = $"DC-{currentYearMonth}{newCounter:D2}"; // Format: STBYYMMCC
                var maxId = _context.DCIds.Any() ? _context.DCIds.Max(e => e.id) + 1 : 1;
                var newBoxEntry = new DCIds
                {
                    id = maxId,
                    ProductionId = newBoxId,
                    date = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                };
                _context.DCIds.Add(newBoxEntry);
                _context.SaveChanges();

                return newBoxId;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( Dispatch dispatch)
        {
            var DAProduction_id = "";
            var DAOutletName = "";

            var DATE = DateTime.Now.ToString("dd-MM-yyyy");
            //var DATE = "01-04-2025";
            DateTime dcdate = DateTime.ParseExact(DATE, "dd-MM-yyyy", null);
            string indate = dcdate.ToString("yyyy-MM-dd");

            var TIME = DateTime.Now.ToString("HH:mm");

            //TALLY ENTRY-INVOICE 
            var DCNO = GetDCNOSTR();
            var INNO = GetINNOSTR();
            var inward = dispatch;
            var outletdetails = _context.OutletMaster.Where(a => a.OutletName.Trim() == dispatch.OutletName.Trim()).FirstOrDefault();
            float BaseAmount = (float)(Packagings_List.Sum(a => a.sellingRs * a.Qty));
            Invoice Invoice = new Invoice()
            {
                EntryType = inward.LedgerType ?? "",
                refno = inward.ProductionId ?? "",
                partyname = inward.OutletName ?? "",
                contactno = outletdetails.OutletContactNo ?? "",
                address = outletdetails.OutletAddress ?? "",
                fright = 0,
                Freight_type = "",
                totalamount = BaseAmount,//
                FreighAmount = 0,
                BaseAmount = BaseAmount,
                paymentterm = "",
                termofdilivery = "",
                remark = "",
                gst_type = "",
                cgst = 0,
                sgst = 0,
                FinalAmount = BaseAmount,
                igst = 0,
                gstno = "",
                country = "",
                state = "",
                pincode = "",
                VoucherType = inward.VoucherType ?? "NA",
                InvoiceItemDetails = new List<InvoiceItemDetails>(),
                dcno = DCNO ?? "NA",
                invoiceno = INNO ?? "NA",
                truckno = inward.VehicleNumber ?? "NA",
                dispatchby = inward.VehicleDriverName ?? "NA",
                destination = "",
                agent = inward.VehicleOwn,
                invDate = indate,
                dcDate = indate,
                orderDate = DATE,
            };
            foreach (var item in Packagings_List)
            {
                int gst = 0;
                var HSN = "NA";
                double amount = item.Qty * item.sellingRs;
                double UNIT = item.TotalNetWg / 1000;
                InvoiceItemDetails SOorderItemDetails = new InvoiceItemDetails()
                {
                    productname = item.Product_Name ?? "NA",
                    UNIT = Convert.ToDouble(UNIT),
                    qty = Convert.ToInt32(item.Qty),
                    uom = "KGS",
                    amount = (float)(amount),
                    rate = (float)(item.sellingRs),
                    cgst = "0",
                    sgst = "0",
                    igst = "0",
                    hsn = HSN,
                };
                Invoice.InvoiceItemDetails.Add(SOorderItemDetails);
            }
            var baseurl = _configuration["AppSettings:BaseUrl"];
            var url = $"{baseurl}/Vouchers/Save_Delievery";
            var data = await _apiService.PostAsync<ApiResponse<string>>(url, Invoice);
            //END

            if (Packagings_List.Count > 0)
            {
                var currentuser1 = HttpContext.User;
                string username = currentuser1.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name).Value;

                //ADD TO DISPATCH TABLE 
                foreach (var item in Packagings_List)
                {
                    DAProduction_id = Packagings_List.Select(a => a.Production_Id.Trim()).FirstOrDefault() ?? "NA";
                    DAOutletName = Packagings_List.Select(a => a.Outlet_Name.Trim()).FirstOrDefault() ?? "NA";
                    var SaveProduction_Date = _context.SaveProduction.Where(a => a.Production_Id.Trim() == item.Production_Id.Trim() && a.ProductName.Trim() == item.Product_Name.Trim() && a.TotalNetWg == item.TotalNetWg && a.TotalNetWg_Uom.Trim() == item.TotalNetWg_Uom.Trim() && a.Box_No.Trim() == item.Box_No.Trim()).FirstOrDefault();
                    var productiondetails = _context.ProductionCapture.Where(a=>a.ProductName.Trim() == item.Product_Name.Trim() && a.OutletName.Trim() == item.Outlet_Name.Trim()).FirstOrDefault();
                    var maxId = _context.Dispatch.Any() ? _context.Dispatch.Max(e => e.Id) + 1 : 1;
                    
                    //ADD INTO DISPATCH TABLE
                    Dispatch Dispatch = new Dispatch()
                    {
                        Id = maxId,
                        ProductionId =item.Production_Id,
                        ProductName =item.Product_Name,
                        OutletName =item.Outlet_Name,
                        ReceiptNo =item.Reciept_Id,
                        DCNo =DCNO,
                        BoxNo =item.Box_No,
                        Qty =item.Qty,
                        TotalNetWg =item.TotalNetWg,
                        TotalNetWg_Uom =item.TotalNetWg_Uom,
                        Production_Date = productiondetails.Production_Date,
                        Production_Time = productiondetails.Production_Time,
                        SaveProduction_Date = SaveProduction_Date.SaveProduction_Date,
                        SaveProduction_Time = SaveProduction_Date.SaveProduction_Time,
                        Packaging_Date = item.Packaging_Date,
                        Packaging_Time =item.Packaging_Time,
                        TransferToDispatch_Date =item.DispatchReady_Date,
                        TransferToDispatch_Time =item.DispatchReady_Time,
                        Dispatch_Date =DATE,
                        Dispatch_Time = TIME,
                        VehicleDriverContactNo = dispatch.VehicleDriverContactNo,
                        VehicleNumber = dispatch.VehicleNumber,
                        VehicleDriverName = dispatch.VehicleDriverName,
                        VehicleOwn = dispatch.VehicleOwn,
                        user = username.ToString(),
                        INNO = INNO,
                    };
                    _context.Dispatch.Add(Dispatch);
                    _context.BoxMaster.Where(b => b.BoxNumber == item.Box_No).ExecuteUpdate(setters => setters.SetProperty(b => b.Use_Flag, 2));
                    _context.SaveChanges();

                    //UPDATE PACKAGING TABLE
                    item.Dispatch_Flag = 1;
                    item.DCNo = DCNO;
                    _context.Packaging.Update(item);
                    _context.SaveChanges();
                }
            }

            //UPDATE PRODUCTION_CAPTURE TABLE
            var find_production = _context.ProductionCapture.Where(a=>a.Production_Id.Trim() == DAProduction_id.Trim() && a.OutletName.Trim() == DAOutletName.Trim()).ToList();
            var find_dispatch = _context.Dispatch.Where(a => a.ProductionId.Trim() == DAProduction_id.Trim() && a.OutletName.Trim() == DAOutletName.Trim()).ToList();
            if (find_production.Sum(a => a.TotalQty) == find_dispatch.Sum(a => a.Qty))
            {
                find_production.ForEach(a =>
                {
                    a.Status = "Completed";
                });
                _context.ProductionCapture.UpdateRange(find_production);
                _context.SaveChanges();

                find_dispatch.ForEach(a =>
                {
                    a.Status = "Completed";
                });
                _context.Dispatch.UpdateRange(find_dispatch);
                _context.SaveChanges();
            }

            return Json(new {success = true, message = "Successfully Done !"});
        }


        public IActionResult getdatatobind(string ProductionId, string DCNo, string BoxNo, string ReceiptNo, string OutletName)
        {
                List<Dispatch> dispatch = new List<Dispatch>();
                double totalqty = 0;
                double totalamount = 0;
                string Datee = "";
                string category = "";

                var list = _context.Dispatch
                    .Where(a => a.OutletName.Trim() == OutletName.Trim() &&
                                a.ReceiptNo.Trim() == ReceiptNo.Trim() &&
                                a.DCNo.Trim() == DCNo.Trim() &&
                                a.ProductionId.Trim() == ProductionId.Trim())
                    .ToList();

                if (list.Any())
                {
                    Datee = list.First().Dispatch_Date;

                    foreach (var item in list)
                    {
                        double sellingrate = _context.Packaging
                            .Where(a => a.Production_Id.Trim() == item.ProductionId.Trim() &&
                                        a.Reciept_Id.Trim() == item.ReceiptNo.Trim() &&
                                        a.DCNo.Trim() == item.DCNo.Trim() &&
                                        a.Product_Name.Trim() == item.ProductName.Trim()
                                        && a.TotalNetWg == item.TotalNetWg)
                            .Select(a => a.sellingRs)
                            .FirstOrDefault();

                        category = _context.ProductMaster
                            .Where(a => a.ProductName.Trim() == item.ProductName.Trim())
                            .Select(a => a.Type)
                            .FirstOrDefault() ?? "NA";

                        dispatch.Add(new Dispatch
                        {
                            ProductName = item.ProductName,
                            Qty = item.Qty,
                            rate = sellingrate,
                            categary = category,
                            amount = item.Qty * sellingrate
                        });

                        totalamount += item.Qty * sellingrate;
                        totalqty += item.Qty;
                    }
                }

                return Json(new { success = true, tabledata = dispatch, totalamount, totalqty, DCNo, OutletName, Datee, category });
            
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Dispatch == null)
            {
                return NotFound();
            }

            var dispatch = await _context.Dispatch.FindAsync(id);
            if (dispatch == null)
            {
                return NotFound();
            }
            return View(dispatch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  Dispatch dispatch)
        {
            if (id != dispatch.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dispatch);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DispatchExists(dispatch.Id))
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
            return View(dispatch);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Dispatch == null)
            {
                return NotFound();
            }

            var dispatch = await _context.Dispatch
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dispatch == null)
            {
                return NotFound();
            }

            return View(dispatch);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Dispatch == null)
            {
                return Problem("Entity set 'DataDBContext.Dispatch'  is null.");
            }
            var dispatch = await _context.Dispatch.FindAsync(id);
            if (dispatch != null)
            {
                _context.Dispatch.Remove(dispatch);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DispatchExists(int id)
        {
          return (_context.Dispatch?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public IActionResult GetDMData(string ProductionId, string DCNo, string BoxNo, string ReceiptNo, string OutletName)
        {
            List<Dispatch> dispatch = new List<Dispatch>();
            double totalqty = 0;
            double totalamount = 0;
            string Datee = "";
            string category = "";

            var list = _context.Dispatch
                .Where(a => a.OutletName.Trim() == OutletName.Trim() &&
                            a.ReceiptNo.Trim() == ReceiptNo.Trim() &&
                            a.DCNo.Trim() == DCNo.Trim() &&
                            a.ProductionId.Trim() == ProductionId.Trim())
                .ToList();

            if (list.Any())
            {
                Datee = list.First().Dispatch_Date;

                foreach (var item in list)
                {
                    double sellingrate = _context.Packaging
                        .Where(a => a.Production_Id.Trim() == item.ProductionId.Trim() &&
                                    a.Reciept_Id.Trim() == item.ReceiptNo.Trim() &&
                                    a.DCNo.Trim() == item.DCNo.Trim() &&
                                    a.Product_Name.Trim() == item.ProductName.Trim()
                                    && a.TotalNetWg == item.TotalNetWg)
                        .Select(a => a.sellingRs)
                        .FirstOrDefault();

                    category = _context.ProductMaster
                        .Where(a => a.ProductName.Trim() == item.ProductName.Trim())
                        .Select(a => a.Type)
                        .FirstOrDefault() ?? "NA";

                    double UNIT = item.TotalNetWg / 1000;
                    double amount = item.Qty * sellingrate;
                    string VALUE = (UNIT) + " KGS";
                    var ratee = (amount/ UNIT) + " /KGS";
                    
                    dispatch.Add(new Dispatch
                    {
                        ProductName = item.ProductName,
                        value = VALUE,
                        UNIT = UNIT,
                        TotalNetWg = UNIT,
                        Qty = item.Qty,
                        uom = "KGS",
                        ratee = (ratee),
                        categary = category,
                        amount = (float)(amount)
                    });

                    totalamount += item.Qty * sellingrate;
                    totalqty += UNIT;

                    //double amount = item.Qty * item.sellingRs;
                    //double UNIT = item.TotalNetWg / 1000;
                    //InvoiceItemDetails SOorderItemDetails = new InvoiceItemDetails()
                    //{
                    //    productname = item.Product_Name ?? "NA",
                    //    UNIT = Convert.ToDouble(UNIT),
                    //    qty = Convert.ToInt32(item.Qty),
                    //    uom = "KGS",
                    //    amount = (float)(amount),
                    //    rate = (float)(item.sellingRs),
                    //    cgst = "0",
                    //    sgst = "0",
                    //    igst = "0",
                    //    hsn = HSN,
                    //};
                }
            }
            var totalqtyy = (totalqty).ToString() + " KGS";

            return Json(new { success = true, tabledata = dispatch, totalamount, totalqtyy, DCNo, OutletName, Datee, category });
        }
    }
}
