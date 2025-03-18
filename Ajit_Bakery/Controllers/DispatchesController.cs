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
    public class DispatchesController : Controller
    {
        private readonly DataDBContext _context;

        public DispatchesController(DataDBContext context)
        {
            _context = context;
        }
        private List<SelectListItem> GetProduction_Id()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.Packaging.Where(a => a.DispatchReady_Flag == 0).AsNoTracking().Select(n =>
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
        public async Task<IActionResult> Index()
        {
              return _context.Dispatch != null ? 
                          View(await _context.Dispatch.ToListAsync()) :
                          Problem("Entity set 'DataDBContext.Dispatch'  is null.");
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( Dispatch dispatch)
        {
            return View(dispatch);
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
