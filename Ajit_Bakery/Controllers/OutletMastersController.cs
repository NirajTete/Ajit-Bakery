using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ajit_Bakery.Data;
using Ajit_Bakery.Models;
using Microsoft.AspNetCore.Authorization;

namespace Ajit_Bakery.Controllers
{
    [Authorize]
    public class OutletMastersController : Controller
    {
        private readonly DataDBContext _context;

        public OutletMastersController(DataDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.OutletMaster.OrderByDescending(a=>a.Id).ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var outletMaster = await _context.OutletMaster
                .FirstOrDefaultAsync(m => m.Id == id);
            if (outletMaster == null)
            {
                return NotFound();
            }
            outletMaster.CreateDate = outletMaster.CreateDate + " " + outletMaster.Createtime;
            outletMaster.ModifiedDate = outletMaster.ModifiedDate + " " + outletMaster.Modifiedtime;
            return View(outletMaster);
        }

        public IActionResult Create()
        {
            var code = "OUTLET";
            var getlast = _context.OutletMaster
                            .Where(a => a.OutletCode.StartsWith(code))
                            .OrderByDescending(a => a.OutletCode)
                            .Select(a => a.OutletCode)
                            .FirstOrDefault();

            int nextNumber = 1; // Default value if no records exist

            if (getlast != null && getlast.StartsWith(code))
            {
                var find = getlast.Substring(code.Length); // Extract numeric part

                if (int.TryParse(find, out int lastNumber))
                {
                    nextNumber = lastNumber + 1; // Increment the number
                }
            }

            var newOutletCode = code + nextNumber; // Generate the new outlet code

            ViewBag.outletcode = newOutletCode;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OutletMaster outletMaster)
        {
            try
            {
                if (outletMaster.OutletName != null)
                {
                    var exist = _context.OutletMaster.Where(a => a.OutletName.Trim() == outletMaster.OutletName.Trim()).FirstOrDefault();
                    if (exist != null)
                    {
                        return Json(new { success = false, message = "Already Exist ! " });
                    }
                }
                int maxId = _context.OutletMaster.Any() ? _context.OutletMaster.Max(e => e.Id) + 1 : 1;
                outletMaster.Id = maxId;
                outletMaster.CreateDate = DateTime.Now.ToString("dd-MM-yyyy");
                outletMaster.ModifiedDate = DateTime.Now.ToString("dd-MM-yyyy");
                outletMaster.Createtime = DateTime.Now.ToString("HH:mm");
                outletMaster.Modifiedtime = DateTime.Now.ToString("HH:mm");
                outletMaster.User = "admin";

                _context.Add(outletMaster);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Created Successfully !" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Warning : " + ex.Message });
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var outletMaster = await _context.OutletMaster.FindAsync(id);
            if (outletMaster == null)
            {
                return NotFound();
            }
            return View(outletMaster);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,OutletMaster outletMaster)
        {
            try
            {
                outletMaster.ModifiedDate = DateTime.Now.ToString("dd-MM-yyyy");
                outletMaster.Modifiedtime = DateTime.Now.ToString("HH:mm");
                outletMaster.User = "admin";
                _context.Update(outletMaster);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Updated Successfully !" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Warning : " + ex.Message });
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var productMaster = await _context.OutletMaster
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (productMaster == null)
                {
                    return Json(new { success = false, message = "Data not found in master ! " });
                }
                else
                {
                    _context.OutletMaster.Remove(productMaster);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Deleted Successfully !" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "WWarning : " + ex.Message });
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var outletMaster = await _context.OutletMaster.FindAsync(id);
            if (outletMaster != null)
            {
                _context.OutletMaster.Remove(outletMaster);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OutletMasterExists(int id)
        {
            return _context.OutletMaster.Any(e => e.Id == id);
        }
    }
}
