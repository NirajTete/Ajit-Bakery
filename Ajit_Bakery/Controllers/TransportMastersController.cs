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
using System.Security.Claims;

namespace Ajit_Bakery.Controllers
{
    [Authorize]
    public class TransportMastersController : Controller
    {
        private readonly DataDBContext _context;

        public TransportMastersController(DataDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.TransportMaster.OrderByDescending(a=>a.Id).ToListAsync());
        }

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
            transportMaster.CreateDate = transportMaster.CreateDate + " " + transportMaster.Createtime;
            transportMaster.ModifiedDate = transportMaster.ModifiedDate + " " + transportMaster.Modifiedtime;
            return View(transportMaster);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( TransportMaster transportMaster)
        {
            try
            {
                if (transportMaster.DriverName != null)
                {
                    var exist = _context.TransportMaster.Where(a => a.DriverName.Trim() == transportMaster.DriverName.Trim()).FirstOrDefault();
                    if (exist != null)
                    {
                        return Json(new { success = false, message = "Already Exist ! " });
                    }
                }
                var currentuser1 = HttpContext.User;
                string username = currentuser1.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name).Value;

                int maxId = _context.TransportMaster.Any() ? _context.TransportMaster.Max(e => e.Id) + 1 : 1;
                transportMaster.Id = maxId;
                transportMaster.CreateDate = DateTime.Now.ToString("dd-MM-yyyy");
                transportMaster.ModifiedDate = DateTime.Now.ToString("dd-MM-yyyy");
                transportMaster.Createtime = DateTime.Now.ToString("HH:mm");
                transportMaster.Modifiedtime = DateTime.Now.ToString("HH:mm");
                transportMaster.User = username;
                _context.Add(transportMaster);
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

            var transportMaster = await _context.TransportMaster.FindAsync(id);
            if (transportMaster == null)
            {
                return NotFound();
            }
            return View(transportMaster);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,TransportMaster transportMaster)
        {
            try
            {
                transportMaster.ModifiedDate = DateTime.Now.ToString("dd-MM-yyyy");
                transportMaster.Modifiedtime = DateTime.Now.ToString("HH:mm");
                transportMaster.User = "admin";
                _context.Update(transportMaster);
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

                var productMaster = await _context.TransportMaster
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (productMaster == null)
                {
                    return Json(new { success = false, message = "Data not found in master ! " });
                }
                else
                {
                    _context.TransportMaster.Remove(productMaster);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Deleted Successfully !" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "WWarning : " + ex.Message });
            }
        }


        private bool TransportMasterExists(int id)
        {
            return _context.TransportMaster.Any(e => e.Id == id);
        }
    }
}
