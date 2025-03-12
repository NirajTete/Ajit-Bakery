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

        public async Task<IActionResult> Index()
        {
            var list = await _context.BoxMaster.OrderByDescending(a=>a.Id).ToListAsync();
            foreach(var item in list)
            {
                item.area = item.BoxLength + " X " + item.BoxBreadth + " X " + item.BoxHeight;
            }
            return View(list);
        }

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

            boxMaster.area = boxMaster.BoxLength + " X " + boxMaster.BoxBreadth + " X " + boxMaster.BoxHeight;
            return View(boxMaster);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( BoxMaster boxMaster)
        {
            if (ModelState.IsValid)
            {
                _context.Add(boxMaster);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(boxMaster);
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  BoxMaster boxMaster)
        {
            try
            {
                boxMaster.ModifiedDate = DateTime.Now.ToString("dd-MM-yyyy");
                boxMaster.Modifiedtime = DateTime.Now.ToString("HH:mm");
                boxMaster.User = "admin";
                _context.Update(boxMaster);
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

            var boxMaster = await _context.BoxMaster
                .FirstOrDefaultAsync(m => m.Id == id);
            if (boxMaster == null)
            {
                return NotFound();
            }

            return View(boxMaster);
        }

        private bool BoxMasterExists(int id)
        {
            return _context.BoxMaster.Any(e => e.Id == id);
        }
    }
}
