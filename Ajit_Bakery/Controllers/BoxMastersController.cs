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
    public class BoxMastersController : Controller
    {
        private readonly DataDBContext _context;

        public BoxMastersController(DataDBContext context)
        {
            _context = context;
        }

        // GET: BoxMasters
        public async Task<IActionResult> Index()
        {
            return View(await _context.BoxMaster.ToListAsync());
        }

        // GET: BoxMasters/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var boxMaster = await _context.BoxMaster
                .FirstOrDefaultAsync(m => m.Id == id);
            if (boxMaster == null)
            {
                return NotFound();
            }

            return View(boxMaster);
        }

        // GET: BoxMasters/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BoxMasters/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,BoxNumber,BoxLength,BoxBreadth,BoxHeight,BoxUom,BoxArea,CreateDate,Createtime,ModifiedDate,Modifiedtime,User")] BoxMaster boxMaster)
        {
            if (ModelState.IsValid)
            {
                _context.Add(boxMaster);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(boxMaster);
        }

        // GET: BoxMasters/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var boxMaster = await _context.BoxMaster.FindAsync(id);
            if (boxMaster == null)
            {
                return NotFound();
            }
            return View(boxMaster);
        }

        // POST: BoxMasters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BoxNumber,BoxLength,BoxBreadth,BoxHeight,BoxUom,BoxArea,CreateDate,Createtime,ModifiedDate,Modifiedtime,User")] BoxMaster boxMaster)
        {
            if (id != boxMaster.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(boxMaster);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BoxMasterExists(boxMaster.Id))
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
            return View(boxMaster);
        }

        // GET: BoxMasters/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var boxMaster = await _context.BoxMaster
                .FirstOrDefaultAsync(m => m.Id == id);
            if (boxMaster == null)
            {
                return NotFound();
            }

            return View(boxMaster);
        }

        // POST: BoxMasters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var boxMaster = await _context.BoxMaster.FindAsync(id);
            if (boxMaster != null)
            {
                _context.BoxMaster.Remove(boxMaster);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BoxMasterExists(int id)
        {
            return _context.BoxMaster.Any(e => e.Id == id);
        }
    }
}
