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
    public class ProductMastersController : Controller
    {
        private readonly DataDBContext _context;

        public ProductMastersController(DataDBContext context)
        {
            _context = context;
        }

        // GET: ProductMasters
        public async Task<IActionResult> Index()
        {
            return View(await _context.ProductMaster.ToListAsync());
        }

        // GET: ProductMasters/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productMaster = await _context.ProductMaster
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productMaster == null)
            {
                return NotFound();
            }

            return View(productMaster);
        }

        // GET: ProductMasters/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ProductMasters/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductName,Qty,Unitqty,Uom,Category,Type,PerGmRate,MRP,DialCode1,DialCode2,CreateDate,Createtime,ModifiedDate,Modifiedtime,User")] ProductMaster productMaster)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productMaster);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(productMaster);
        }

        // GET: ProductMasters/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productMaster = await _context.ProductMaster.FindAsync(id);
            if (productMaster == null)
            {
                return NotFound();
            }
            return View(productMaster);
        }

        // POST: ProductMasters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductName,Qty,Unitqty,Uom,Category,Type,PerGmRate,MRP,DialCode1,DialCode2,CreateDate,Createtime,ModifiedDate,Modifiedtime,User")] ProductMaster productMaster)
        {
            if (id != productMaster.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productMaster);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductMasterExists(productMaster.Id))
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
            return View(productMaster);
        }

        // GET: ProductMasters/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productMaster = await _context.ProductMaster
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productMaster == null)
            {
                return NotFound();
            }

            return View(productMaster);
        }

        // POST: ProductMasters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productMaster = await _context.ProductMaster.FindAsync(id);
            if (productMaster != null)
            {
                _context.ProductMaster.Remove(productMaster);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductMasterExists(int id)
        {
            return _context.ProductMaster.Any(e => e.Id == id);
        }
    }
}
