using Ajit_Bakery.Data;
using Ajit_Bakery.Models;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ajit_Bakery.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly DataDBContext _context;
        public ReportsController(DataDBContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult ProductionReport()
        {
            var list = _context.SaveProduction.OrderByDescending(a=>a.Id).ToList();
            foreach(var item in list)
            {
                item.SaveProduction_Date = item.SaveProduction_Date+" "+ item.SaveProduction_Time;
            }
            return View(list);
        }
        [HttpGet]
        public IActionResult DispatchReport()
        {
            var list = _context.Dispatch
               //.Where(a => a.DispatchReady_Flag == 1)
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
                   a.Dispatch_Date,
                   a.Dispatch_Time,
               })
               .Select(g => new Dispatch
               {
                   ProductionId = g.Key.ProductionId,
                   DCNo = g.Key.DCNo,
                   OutletName = g.Key.OutletName,
                   BoxNo = g.Key.BoxNo,
                   ReceiptNo = g.Key.ReceiptNo,
                   ProductName = g.Key.ProductName,
                   Dispatch_Date = g.Key.Dispatch_Date,
                   category = g.Key.category, // Ensure it's available in memory
                   Dispatch_Time = g.Key.Dispatch_Time, // Ensure it's available in memory
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
                item.Dispatch_Date = item.Dispatch_Date + " " + item.Dispatch_Time;
            }

            return View(list);
        }
        [HttpGet]
        public async Task<IActionResult> OrderStatusReport()
        {
            List<ProductionCapture> productionCaptures = new List<ProductionCapture>();

            //Fetch ordered data from database
            var list = await _context.ProductionCapture.OrderByDescending(a => a.Id).ToListAsync();
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
        [HttpGet]
        public IActionResult TATReport()
        {
            List<TATReport> TATReport = new List<TATReport>();
            var list = _context.ProductionCapture.AsNoTracking().OrderByDescending(a => a.Id).ToList();
            foreach (var item in list)
            {
                var totalnetwt = "-";
                var production_time = "-";
                var saveproduction_time = "-";
                var Packaging_time = "-";
                var transfer_time = "-";
                var Dispatch_Time = "-";

                if(item.TotalQty>0)
                {
                    var Saveproduction_list = _context.SaveProduction.Where(a => a.Production_Id.Trim() == item.Production_Id.Trim()).ToList();
                    var packaging_list = _context.Packaging.Where(a => a.Production_Id.Trim() == item.Production_Id.Trim()).ToList();
                    var dispatch_list = _context.Dispatch.Where(a => a.ProductionId.Trim() == item.Production_Id.Trim()).ToList();
                    var proqty = _context.ProductionCapture.Where(a=>a.Production_Id.Trim() == item.Production_Id.Trim() && a.ProductName.Trim() == item.ProductName.Trim() && a.TotalQty > 0).FirstOrDefault();
                    for (int i = 1; i <= item.TotalQty; i++)
                    {
                        var production = list.Where(a => a.Production_Id == item.Production_Id && a.OutletName.Trim() == item.OutletName.Trim() && a.ProductName.Trim() == item.ProductName.Trim() && a.TotalQty > 0).FirstOrDefault();
                        var dispatch = dispatch_list.Where(a => a.ProductionId == item.Production_Id && a.OutletName.Trim() == item.OutletName.Trim() && a.ProductName.Trim() == item.ProductName.Trim() && a.Qty > 0).FirstOrDefault();
                        var packaging = packaging_list.Where(a => a.Production_Id == item.Production_Id && a.Outlet_Name.Trim() == item.OutletName.Trim() && a.Product_Name.Trim() == item.ProductName.Trim() && a.Qty > 0).FirstOrDefault();
                        var saveproduction = Saveproduction_list.Where(a => a.Production_Id == item.Production_Id && a.ProductName.Trim() == item.ProductName.Trim() && a.Qty > 0).FirstOrDefault();

                        if(saveproduction != null)
                        {
                            totalnetwt = saveproduction.TotalNetWg + " " + saveproduction.TotalNetWg_Uom;
                            saveproduction_time = ConvertTo12HourFormat(saveproduction.SaveProduction_Time);
                            saveproduction.Qty = saveproduction.Qty - 1;
                        }
                        if(packaging != null)
                        {
                            Packaging_time = ConvertTo12HourFormat(packaging.Packaging_Time);
                            transfer_time = ConvertTo12HourFormat(packaging.DispatchReady_Time);
                            packaging.Qty = packaging.Qty - 1;
                        }
                        if(dispatch != null)
                        {
                            Dispatch_Time = ConvertTo12HourFormat(dispatch.Dispatch_Time);
                            dispatch.Qty = dispatch.Qty - 1;
                        }
                        if(production != null)
                        {
                            production_time = ConvertTo12HourFormat(production.Production_Time);
                            proqty.TotalQty = proqty.TotalQty - 1;
                        }
                        TATReport tt = new TATReport()
                        {
                            ProductionId = item.Production_Id,
                            outlet = item.OutletName,
                            productname = item.ProductName,
                            totalnetwg = totalnetwt,
                            order_date = item.Production_Date + " " + production_time,
                            production_date = (saveproduction_time),
                            packaging_date = (Packaging_time),
                            transfer_date = (transfer_time),
                            dispatch_date = (Dispatch_Time),
                        };
                        TATReport.Add(tt);
                    }
                }
            }
            return View(TATReport);
        }
        private string ConvertTo12HourFormat(string time24)
        {
            if (DateTime.TryParseExact(time24, "HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime dateTime))
            {
                return dateTime.ToString("hh:mm tt"); // Converts to 12-hour format with AM/PM
            }
            return time24; // Return original if parsing fails
        }
        //public IActionResult TATReport()
        //{
        //    List<TATReport> TATReport = new List<TATReport>();
        //    var list = _context.Dispatch.OrderByDescending(a => a.Id).ToList();

        //    foreach (var item in list)
        //    {
        //        var orderdatetime = _context.ProductionCapture
        //            .Where(a => a.Production_Id == item.ProductionId &&
        //                        a.OutletName.Trim() == item.OutletName.Trim() &&
        //                        a.ProductName.Trim() == item.ProductName.Trim())
        //            .FirstOrDefault();

        //        if (orderdatetime == null)
        //            continue; // Skip if no matching production record is found

        //        // Combine Production Date with Time
        //        DateTime orderTime = DateTime.ParseExact(orderdatetime.Production_Date + " " + orderdatetime.Production_Time, "dd-MM-yyyy HH:mm", null);
        //        DateTime baseDate = orderTime.Date; // Use orderTime's date for all other times

        //        // Convert time-only strings into DateTime by combining them with baseDate
        //        DateTime productionTime = DateTime.ParseExact(item.Production_Time, "HH:mm", null).Add(baseDate.TimeOfDay);
        //        DateTime packagingTime = DateTime.ParseExact(item.Packaging_Time, "HH:mm", null).Add(baseDate.TimeOfDay);
        //        DateTime transferTime = DateTime.ParseExact(item.TransferToDispatch_Time, "HH:mm", null).Add(baseDate.TimeOfDay);
        //        DateTime dispatchTime = DateTime.ParseExact(item.Dispatch_Time, "HH:mm", null).Add(baseDate.TimeOfDay);

        //        // Ensure time sequence is correct (handle cases where times might be past midnight)
        //        if (productionTime < orderTime) productionTime = productionTime.AddDays(1);
        //        if (packagingTime < productionTime) packagingTime = packagingTime.AddDays(1);
        //        if (transferTime < packagingTime) transferTime = transferTime.AddDays(1);
        //        if (dispatchTime < transferTime) dispatchTime = dispatchTime.AddDays(1);

        //        // Calculate TAT (Turnaround Time) in HH:mm format
        //        string tat_production = (productionTime - orderTime).ToString(@"hh\:mm");
        //        string tat_packaging = (packagingTime - productionTime).ToString(@"hh\:mm");
        //        string tat_transfer = (transferTime - packagingTime).ToString(@"hh\:mm");
        //        string tat_dispatch = (dispatchTime - transferTime).ToString(@"hh\:mm");

        //        // Create the report entry
        //        TATReport tt = new TATReport()
        //        {
        //            ProductionId = item.ProductionId,
        //            outlet = item.OutletName,
        //            productname = item.ProductName,
        //            totalnetwg = item.TotalNetWg + " " + item.TotalNetWg_Uom,
        //            order_date = orderTime.ToString("dd-MM-yyyy HH:mm"),
        //            production_date = tat_production,  // TAT from Order to Production
        //            packaging_date = tat_packaging,    // TAT from Production to Packaging
        //            transfer_date = tat_transfer,      // TAT from Packaging to Transfer
        //            dispatch_date = tat_dispatch       // TAT from Transfer to Dispatch
        //        };

        //        TATReport.Add(tt);
        //    }

        //    return View(TATReport);
        //}

    }
}
