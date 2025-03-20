using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ajit_Bakery.Data;
using Ajit_Bakery.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace Ajit_Bakery.Controllers
{
    [Authorize]
    public class UserTypesController : Controller
    {
        private readonly DataDBContext _context;
        public INotyfService _notifyService { get; }

        public UserTypesController(DataDBContext context, INotyfService notifyService)
        {
            _context = context; _notifyService = notifyService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.UserType.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userType = await _context.UserType
                .FirstOrDefaultAsync(m => m.user_id == id);
            if (userType == null)
            {
                return NotFound();
            }

            return View(userType);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserType userType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userType);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userType = await _context.UserType.FindAsync(id);
            if (userType == null)
            {
                return NotFound();
            }
            return View(userType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,UserType userType)
        {
            try
            {
                //userType.ModifiedDate = DateTime.Now.ToString("dd-MM-yyyy");
                //userType.Modifiedtime = DateTime.Now.ToString("HH:mm");
                //userType.User = "admin";
                _context.Update(userType);
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

            var userType = await _context.UserType
                .FirstOrDefaultAsync(m => m.user_id == id);
            if (userType == null)
            {
                return NotFound();
            }

            return View(userType);
        }


        private bool UserTypeExists(int id)
        {
            return _context.UserType.Any(e => e.user_id == id);
        }
    }
}
