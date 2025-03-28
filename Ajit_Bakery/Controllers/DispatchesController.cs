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

namespace Ajit_Bakery.Controllers
{
    [Authorize]
    public class DispatchesController : Controller
    {
        private readonly DataDBContext _context;

        public DispatchesController(DataDBContext context)
        {
            _context = context;
        }

        private static List<Packaging> Packagings_List = new List<Packaging>();
        public IActionResult GetOutlets(string Production_Id)
        {
            List<DialDetailViewModel> DialDetailViewModellist = new List<DialDetailViewModel>();
            var list = _context.ProductionCapture.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Status == "Pending").ToList();
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    var data = _context.ProductionCapture
                        .Where(a => a.ProductName.Trim() == item.ProductName.Trim() && a.Production_Id.Trim() == Production_Id.Trim() && a.OutletName.Trim() == item.OutletName.Trim())
                        .Sum(a => a.TotalQty);

                    var savedata = _context.Packaging
                        .Where(a => a.Product_Name.Trim() == item.ProductName.Trim() && a.Production_Id.Trim() == Production_Id.Trim() && a.DispatchReady_Flag == 1 && a.Dispatch_Flag == 0 && a.Outlet_Name.Trim() == item.OutletName.Trim())
                        .Sum(a => a.Qty);

                    int DispatchReady = savedata;
                    //var found = _context.ProductMaster.Where(a => a.ProductName.Trim() == item.ProductName.Trim()).FirstOrDefault();
                    //if (found != null)
                    //{
                        int PendingQty = Math.Abs(data - savedata);
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

            var outlet_list = _context.Packaging.Where(a => a.DispatchReady_Flag == 1 && a.Dispatch_Flag == 0 &&  a.Production_Id.Trim() == Production_Id.Trim()).Select(a=>a.Outlet_Name.Trim()).Distinct().ToList();

            return Json(new { success = true, TableData = DialDetailViewModellist, outlet_list });
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
                // Get total packaged quantity from Packaging table where DispatchReady_Flag == 1
                getcount = _context.Packaging
                    .Where(a => a.DispatchReady_Flag == 1 && a.Outlet_Name.Trim() == dcno.Trim())
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
                Text = "----Select DCNo ----"
            };

            lstProducts.Insert(0, defItem);
            return lstProducts;
        }
        private List<SelectListItem> GetProduction_Id()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.Packaging.Where(a => a.DispatchReady_Flag == 1).AsNoTracking().Select(n =>
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
                Text = "----Select DriverName ----"
            };

            lstProducts.Insert(0, defItem);
            return Json(new {sucess = true, data = lstProducts});

        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Packagings_List.Clear();

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

        public IActionResult Create()
        {
            ViewBag.GetProduction_Id = GetProduction_Id();
            //ViewBag.DCNO = GetDCNO();
            return View();
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
            var TIME = DateTime.Now.ToString("HH:mm");

            if (Packagings_List.Count > 0)
            {
                var currentuser1 = HttpContext.User;
                string username = currentuser1.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name).Value;


                var DCNO = GetDCNOSTR();
                //ADD TO DISPATCH TABLE 
                foreach (var item in Packagings_List)
                {
                    DAProduction_id = Packagings_List.Select(a => a.Production_Id.Trim()).FirstOrDefault() ?? "NA";
                    DAOutletName = Packagings_List.Select(a => a.Outlet_Name.Trim()).FirstOrDefault() ?? "NA";
                    var SaveProduction_Date = _context.SaveProduction.Where(a => a.Production_Id.Trim() == item.Production_Id.Trim() && a.ProductName.Trim() == item.Product_Name.Trim() && a.TotalNetWg == item.TotalNetWg && a.TotalNetWg_Uom.Trim() == item.TotalNetWg_Uom.Trim() && a.Box_No.Trim() == item.Box_No.Trim()).FirstOrDefault();

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
                        Production_Date =item.Production_Dt,
                        Production_Time =item.Production_Tm,
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
    }
}
