using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ajit_Bakery.Data;
using Ajit_Bakery.Models;

namespace Ajit_Bakery.Controllers
{
    public class PackagingsController : Controller
    {
        private readonly DataDBContext _context;

        public PackagingsController(DataDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.Packaging.OrderByDescending(a=>a.Id).ToListAsync();
            foreach(var item in list)
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
            if(check1.Count > 0)
            {
                //against outlet getting totalqty to pick
                var check_outlet = _context.ProductionCapture.Where(a => a.Status.Trim() == "Pending" && a.OutletName.Trim() == outletName.Trim() && a.ProductName.Trim() == productname.Trim()).ToList().Sum(a=>a.TotalQty);

                if(check_outlet == 0)
                {
                    return Json(new { success = false, message = "Please scan correct one against outlet " + outletName });
                }
                var check_packing = _context.Packaging.Where(a=>a.Production_Id.Trim() == Production_Id.Trim() && a.Product_Name.Trim() == productname.Trim() && a.Outlet_Name.Trim() == outletName.Trim()).ToList().Sum(a=>a.Qty);
                var check_packing1 = Packagings_List.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Product_Name.Trim() == productname.Trim() && a.Outlet_Name.Trim() == outletName.Trim()).ToList().Sum(a => a.Qty);

                if ((check_packing+ check_packing1) == check_outlet)
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
                            Production_Dt = checkagain.Packaging_Date,
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

                        var qtyremainig = ProductionCapturedata+ ProductionCapturedata1;
                        var qtypick = Packagingdata+ Packagingdata1;
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
            var Packagingdata = _context.Packaging.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Outlet_Name.Trim() == Outlet_Name.Trim()).ToList().Sum(a=>a.Qty);
            var Packagingdata1 = Packagings_List.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Outlet_Name.Trim() == Outlet_Name.Trim()).ToList().Sum(a=>a.Qty);

            var ProductionCapturedata = _context.ProductionCapture.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Status.Trim() == "Pending" && a.OutletName.Trim() == Outlet_Name.Trim()).ToList().Sum(a=>a.TotalQty);
            var ProductionCapturedata1 = ProductionCapture_List.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Status.Trim() == "Pending" && a.OutletName.Trim() == Outlet_Name.Trim()).ToList().Sum(a=>a.TotalQty);

            var qtyremainig = ProductionCapturedata+ ProductionCapturedata1;
            var qtypick = Packagingdata+ Packagingdata1;
            var valuefound = Math.Abs(qtypick - qtyremainig);
            if(valuefound > 0)
            {
                return Json(new { success = true, qtypick = qtypick, qtyremainig = valuefound });
            }
            return Json(new { success = false, message = "You have already picked all qty against "+Outlet_Name + "  outlet !" });
        }
        public IActionResult GetOutlets(string Production_Id)
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.ProductionCapture.Where(a=>a.Status == "Pending").AsNoTracking().Select(n =>
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
            return Json(new {success=true,data = lstProducts  });
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

            lstProducts = _context.ProductionCapture.Where(a=>a.Status == "Pending").AsNoTracking().Select(n =>
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
        public IActionResult Create()
        {
            ViewBag.GetProduction_Id = GetProduction_Id();
            ViewBag.GetBoxNos = GetBoxNos();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( Packaging packaging)
        {
            //save logic
            var Packagings_List1 = Packagings_List.ToList();
            var SaveProduction_List1 = SaveProduction_List.ToList();
            var ProductionCapture_List1 = ProductionCapture_List.ToList();
            _context.SaveProduction.UpdateRange(SaveProduction_List);
            foreach(var item in Packagings_List)
            {
                item.Id = _context.Packaging.Any() ? _context.Packaging.Max(e => e.Id) + 1 : 1;
                _context.Packaging.Add(item);
                _context.SaveChanges();
            }

            //Generate recipt logic need to be added

            return Json(new { success = true, message = "Successfully Done !" });
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
