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

        public async Task<IActionResult> Index()
        {
            var listdata =await _context.DialMaster.OrderByDescending(a=>a.Id).ToListAsync();
            foreach(var item in listdata)
            {
                if(item.DialShape == "Round")
                {
                    var calvalue1 = (item.DialDiameter).ToString();
                    item.calvalue = calvalue1 + " " + item.LengthUom;
                    item.areacalvalue = item.DialArea+" "+ item.LengthUom;
                }
                else
                {
                    var calvalue = item.DialLength + " X " + item.DialBreadth+ " "+item.LengthUom;
                    item.calvalue = calvalue;
                    item.areacalvalue = item.DialArea + " " + item.LengthUom;
                }
                var value = item.DialWg + " " + item.DialWgUom;
                item.value = value;
            }
            return View(listdata);
        }

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

        public IActionResult Create()
        {
            int newId = (_context.DialMaster.Max(u => (int?)u.Id) ?? 0) + 1;
            DateTime currentDateTime = DateTime.Now;
            ViewBag.dialcode = "D0" + newId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( DialMaster dialMaster)
        {
            try
            {
                dialMaster.CreateDate = DateTime.Now.ToString("dd-MM-yyyy");
                dialMaster.ModifiedDate = DateTime.Now.ToString("dd-MM-yyyy");
                dialMaster.Createtime = DateTime.Now.ToString("HH:mm");
                dialMaster.Modifiedtime = DateTime.Now.ToString("HH:mm");
                dialMaster.User = "admin";

                _context.Add(dialMaster);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  DialMaster dialMaster)
        {
            try
            {
                dialMaster.ModifiedDate = DateTime.Now.ToString("dd-MM-yyyy");
                dialMaster.Modifiedtime = DateTime.Now.ToString("HH:mm");
                dialMaster.User = "admin";
                _context.Update(dialMaster);
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

            var dialMaster = await _context.DialMaster
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dialMaster == null)
            {
                return NotFound();
            }

            return View(dialMaster);
        }

        private bool DialMasterExists(int id)
        {
            return _context.DialMaster.Any(e => e.Id == id);
        }
    }
}
