using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ajit_Bakery.Data;
using Ajit_Bakery.Models;
using Microsoft.AspNetCore.Authorization;

namespace Ajit_Bakery.Controllers
{
    [Authorize]
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
            if (dialMaster.DialShape == "Round")
            {
                var calvalue1 = (dialMaster.DialDiameter).ToString();
                dialMaster.calvalue = calvalue1 + " " + dialMaster.LengthUom;
                dialMaster.areacalvalue = dialMaster.DialArea + " " + dialMaster.LengthUom;
                dialMaster.CreateDate = dialMaster.CreateDate + " " + dialMaster.Createtime;
                dialMaster.ModifiedDate = dialMaster.ModifiedDate + " " + dialMaster.Modifiedtime;
            }
            else
            {
                var calvalue = dialMaster.DialLength + " X " + dialMaster.DialBreadth + " " + dialMaster.LengthUom;
                dialMaster.calvalue = calvalue;
                dialMaster.areacalvalue = dialMaster.DialArea + " " + dialMaster.LengthUom;
                dialMaster.CreateDate = dialMaster.CreateDate + " " + dialMaster.Createtime;
                dialMaster.ModifiedDate = dialMaster.ModifiedDate + " " + dialMaster.Modifiedtime;
            }
            var value = dialMaster.DialWg + " " + dialMaster.DialWgUom;
            dialMaster.value = value;
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
                
                int maxId = _context.DialMaster.Any() ? _context.DialMaster.Max(e => e.Id) + 1 : 1;
                dialMaster.Id = maxId;
                var data = "D";
                if(dialMaster.DialShape == "Circle")
                {
                    data = data + "C"+dialMaster.DialDiameter+"*"+dialMaster.DialDiameter;
                    dialMaster.DialCode = data;
                    dialMaster.DialLength = dialMaster.DialDiameter;
                    dialMaster.DialBreadth = dialMaster.DialDiameter;
                    dialMaster.DialArea = (dialMaster.DialBreadth * dialMaster.DialLength).ToString();
                    if (dialMaster.DialCode != null)
                    {
                        var exist = _context.DialMaster.Where(a => a.DialCode.Trim() == dialMaster.DialCode.Trim() && a.DialShape.Trim() == dialMaster.DialShape.Trim()).FirstOrDefault();
                        if (exist != null)
                        {
                            return Json(new { success = false, message = "Already Exist ! " });
                        }
                    }
                }
                else
                {
                    data = data + "S" + dialMaster.DialLength + "*" + dialMaster.DialBreadth;
                    dialMaster.DialCode = data;
                    if (dialMaster.DialCode != null)
                    {
                        var exist = _context.DialMaster.Where(a => a.DialCode.Trim() == dialMaster.DialCode.Trim() && a.DialShape.Trim() == dialMaster.DialShape.Trim()).FirstOrDefault();
                        if (exist != null)
                        {
                            return Json(new { success = false, message = "Already Exist ! " });
                        }
                    }
                }

                dialMaster.CreateDate = DateTime.Now.ToString("dd-MM-yyyy");
                dialMaster.ModifiedDate = DateTime.Now.ToString("dd-MM-yyyy");
                dialMaster.Createtime = DateTime.Now.ToString("HH:mm");
                dialMaster.Modifiedtime = DateTime.Now.ToString("HH:mm");
                dialMaster.User = "admin";
                if(dialMaster.DialUsedForCakes_Uom == "KGS")
                {
                    dialMaster.DialUsedForCakes = dialMaster.DialUsedForCakes * 1000; //converted gm value
                    dialMaster.DialUsedForCakes_Uom = "GMS";//gm 
                }
                else
                {
                    dialMaster.DialUsedForCakes = dialMaster.DialUsedForCakes;//gm value
                    dialMaster.DialUsedForCakes_Uom = dialMaster.DialUsedForCakes_Uom;//gm
                }
                

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
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var productMaster = await _context.DialMaster
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (productMaster == null)
                {
                    return Json(new { success = false, message = "Data not found in master ! " });
                }
                else
                {
                    _context.DialMaster.Remove(productMaster);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Deleted Successfully !" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "WWarning : " + ex.Message });
            }
        }

        private bool DialMasterExists(int id)
        {
            return _context.DialMaster.Any(e => e.Id == id);
        }
    }
}
