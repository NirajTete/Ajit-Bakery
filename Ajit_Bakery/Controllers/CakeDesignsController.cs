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
    public class CakeDesignsController : Controller
    {
        private readonly DataDBContext _context;

        public CakeDesignsController(DataDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = _context.CakeDesign.OrderByDescending(a=>a.Id).ToList();
            return View(list);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.CakeDesign == null)
            {
                return NotFound();
            }

            var cakeDesign = await _context.CakeDesign
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cakeDesign == null)
            {
                return NotFound();
            }

            return View(cakeDesign);
        }

        public IActionResult Create()
        {
            int maxId = _context.CakeDesign.Any() ? _context.CakeDesign.Max(e => e.Id) + 1 : 1;
            ViewBag.Code = "D" + maxId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CakeDesign cakeDesign)
        {
            try
            {
                if (cakeDesign.CakeDesign_Name != null)
                {
                    var exist = _context.CakeDesign.Where(a => a.CakeDesign_Name.Trim() == cakeDesign.CakeDesign_Name.Trim()).FirstOrDefault();
                    if (exist != null)
                    {
                        return Json(new { success = false, message = "Already Exist ! " });
                    }
                }
                int maxId = _context.CakeDesign.Any() ? _context.CakeDesign.Max(e => e.Id) + 1 : 1;
                cakeDesign.Id = maxId;
                _context.Add(cakeDesign);
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
            if (id == null || _context.CakeDesign == null)
            {
                return NotFound();
            }

            var cakeDesign = await _context.CakeDesign.FindAsync(id);
            if (cakeDesign == null)
            {
                return NotFound();
            }
            return View(cakeDesign);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CakeDesign cakeDesign)
        {
            try
            {
                if (cakeDesign.CakeDesign_Name == null)
                {
                    
                        return Json(new { success = false, message = "Name found null ! " });
                    
                }
                _context.Update(cakeDesign);
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

                var CakeDesign = await _context.CakeDesign
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (CakeDesign == null)
                {
                    return Json(new { success = false, message = "Data not found in master ! " });
                }
                else
                {
                    _context.CakeDesign.Remove(CakeDesign);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Deleted Successfully !" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "WWarning : " + ex.Message });
            }
        }

        private bool CakeDesignExists(int id)
        {
          return (_context.CakeDesign?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
