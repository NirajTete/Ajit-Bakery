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
    public class TransportMastersController : Controller
    {
        private readonly DataDBContext _context;

        public TransportMastersController(DataDBContext context)
        {
            _context = context;
        }

        // GET: TransportMasters
        public async Task<IActionResult> Index()
        {
            return View(await _context.TransportMaster.ToListAsync());
        }

        // GET: TransportMasters/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transportMaster = await _context.TransportMaster
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transportMaster == null)
            {
                return NotFound();
            }

            return View(transportMaster);
        }

        // GET: TransportMasters/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TransportMasters/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DriverName,DriverContactNo,VehicleNo,VehicleOwn,VehicleType,VehicleNoOfTyre,VehicleCapacity,VehicleVolume,CreateDate,Createtime,ModifiedDate,Modifiedtime,User")] TransportMaster transportMaster)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transportMaster);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(transportMaster);
        }

        // GET: TransportMasters/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transportMaster = await _context.TransportMaster.FindAsync(id);
            if (transportMaster == null)
            {
                return NotFound();
            }
            return View(transportMaster);
        }

        // POST: TransportMasters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DriverName,DriverContactNo,VehicleNo,VehicleOwn,VehicleType,VehicleNoOfTyre,VehicleCapacity,VehicleVolume,CreateDate,Createtime,ModifiedDate,Modifiedtime,User")] TransportMaster transportMaster)
        {
            if (id != transportMaster.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transportMaster);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransportMasterExists(transportMaster.Id))
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
            return View(transportMaster);
        }

        // GET: TransportMasters/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transportMaster = await _context.TransportMaster
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transportMaster == null)
            {
                return NotFound();
            }

            return View(transportMaster);
        }

        // POST: TransportMasters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transportMaster = await _context.TransportMaster.FindAsync(id);
            if (transportMaster != null)
            {
                _context.TransportMaster.Remove(transportMaster);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransportMasterExists(int id)
        {
            return _context.TransportMaster.Any(e => e.Id == id);
        }
    }
}
