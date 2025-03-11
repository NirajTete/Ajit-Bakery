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

        // GET: OutletMasters
        public async Task<IActionResult> Index()
        {
            return View(await _context.OutletMaster.ToListAsync());
        }

        // GET: OutletMasters/Details/5
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

        // GET: OutletMasters/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: OutletMasters/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OutletCode,OutletName,OutletAddress,OutletContactNo,OutletContactPerson,CreateDate,Createtime,ModifiedDate,Modifiedtime,User")] OutletMaster outletMaster)
        {
            if (ModelState.IsValid)
            {
                _context.Add(outletMaster);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(outletMaster);
        }

        // GET: OutletMasters/Edit/5
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

        // POST: OutletMasters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OutletCode,OutletName,OutletAddress,OutletContactNo,OutletContactPerson,CreateDate,Createtime,ModifiedDate,Modifiedtime,User")] OutletMaster outletMaster)
        {
            if (id != outletMaster.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(outletMaster);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OutletMasterExists(outletMaster.Id))
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
            return View(outletMaster);
        }

        // GET: OutletMasters/Delete/5
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

        // POST: OutletMasters/Delete/5
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
