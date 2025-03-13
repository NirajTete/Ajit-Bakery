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
    public class SaveProductionsController : Controller
    {
        private readonly DataDBContext _context;

        public SaveProductionsController(DataDBContext context)
        {
            _context = context;
        }

        // GET: SaveProductions
        public async Task<IActionResult> Index()
        {
            return View(await _context.SaveProduction.ToListAsync());
        }

        // GET: SaveProductions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var saveProduction = await _context.SaveProduction
                .FirstOrDefaultAsync(m => m.Id == id);
            if (saveProduction == null)
            {
                return NotFound();
            }

            return View(saveProduction);
        }

        // GET: SaveProductions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SaveProductions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductName,ProductGrossWg,DialTierWg,DialTierWg_Uom,DialCode,TotalNetWg,TotalNetWg_Uom,Qty,SaveProduction_Date,SaveProduction_Time")] SaveProduction saveProduction)
        {
            if (ModelState.IsValid)
            {
                _context.Add(saveProduction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(saveProduction);
        }

        // GET: SaveProductions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var saveProduction = await _context.SaveProduction.FindAsync(id);
            if (saveProduction == null)
            {
                return NotFound();
            }
            return View(saveProduction);
        }

        // POST: SaveProductions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductName,ProductGrossWg,DialTierWg,DialTierWg_Uom,DialCode,TotalNetWg,TotalNetWg_Uom,Qty,SaveProduction_Date,SaveProduction_Time")] SaveProduction saveProduction)
        {
            if (id != saveProduction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(saveProduction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SaveProductionExists(saveProduction.Id))
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
            return View(saveProduction);
        }

        // GET: SaveProductions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var saveProduction = await _context.SaveProduction
                .FirstOrDefaultAsync(m => m.Id == id);
            if (saveProduction == null)
            {
                return NotFound();
            }

            return View(saveProduction);
        }

        // POST: SaveProductions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var saveProduction = await _context.SaveProduction.FindAsync(id);
            if (saveProduction != null)
            {
                _context.SaveProduction.Remove(saveProduction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SaveProductionExists(int id)
        {
            return _context.SaveProduction.Any(e => e.Id == id);
        }
    }
}
