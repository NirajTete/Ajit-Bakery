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
    public class DialMastersController : Controller
    {
        private readonly DataDBContext _context;

        public DialMastersController(DataDBContext context)
        {
            _context = context;
        }

        // GET: DialMasters
        public async Task<IActionResult> Index()
        {
            return View(await _context.DialMaster.ToListAsync());
        }

        // GET: DialMasters/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dialMaster = await _context.DialMaster
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dialMaster == null)
            {
                return NotFound();
            }

            return View(dialMaster);
        }

        // GET: DialMasters/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DialMasters/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DialCode,DialShape,DialWg,DialWgUom,DialDiameter,DialLength,DialBreadth,DialUom,DialArea,CreateDate,Createtime,ModifiedDate,Modifiedtime,User")] DialMaster dialMaster)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dialMaster);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dialMaster);
        }

        // GET: DialMasters/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dialMaster = await _context.DialMaster.FindAsync(id);
            if (dialMaster == null)
            {
                return NotFound();
            }
            return View(dialMaster);
        }

        // POST: DialMasters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DialCode,DialShape,DialWg,DialWgUom,DialDiameter,DialLength,DialBreadth,DialUom,DialArea,CreateDate,Createtime,ModifiedDate,Modifiedtime,User")] DialMaster dialMaster)
        {
            if (id != dialMaster.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dialMaster);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DialMasterExists(dialMaster.Id))
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
            return View(dialMaster);
        }

        // GET: DialMasters/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dialMaster = await _context.DialMaster
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dialMaster == null)
            {
                return NotFound();
            }

            return View(dialMaster);
        }

        // POST: DialMasters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dialMaster = await _context.DialMaster.FindAsync(id);
            if (dialMaster != null)
            {
                _context.DialMaster.Remove(dialMaster);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DialMasterExists(int id)
        {
            return _context.DialMaster.Any(e => e.Id == id);
        }
    }
}
