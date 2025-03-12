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
    public class OutletMastersController : Controller
    {
        private readonly DataDBContext _context;

        public OutletMastersController(DataDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.OutletMaster.ToListAsync());
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

            return View(outletMaster);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OutletMaster outletMaster)
        {
            if (ModelState.IsValid)
            {
                _context.Add(outletMaster);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(outletMaster);
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

            return View(outletMaster);
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
