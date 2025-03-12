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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Ajit_Bakery.Controllers
{
    public class UserMastersController : Controller
    {
        private readonly DataDBContext _context;
        public INotyfService _notyfyService { get; }
        public UserMastersController(DataDBContext context, INotyfService notyfyService)
        {
            _context = context;
            _notyfyService = notyfyService;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            //HttpContext.Session.Clear(); // Clear all session data
            //HttpContext.Session.Remove("username"); // Remove specific session key if needed
            //HttpContext.Session.Remove("DEPT_NAME");
            //HttpContext.Session.Remove("Role");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //_notyfyService.Warning("Logout Successfully !");
            return RedirectToAction("Login", "UserMasters"); // Redirect to login page
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserMaster loginPage)
        {
            try
            {
                // Fetch user details from the database using LINQ
                var user = _context.UserMaster
                            .Where(x => x.UserName.ToLower().Trim() == loginPage.UserName.ToLower().Trim())
                            //.Select(x => new { x.UserPassward, x.UserDept, x.UserRole , x.UserName,})
                            .FirstOrDefault();

                if (user != null)
                {
                    var pageallot = _context.UserManagment.Where(a => a.UserName == user.UserName.Trim()).Select(a => a.PageName).ToList();
                    //data show in list
                    var data = _context.MenuModel.Where(menu => pageallot.Contains(menu.Title)).ToList();
                    var jsonData = JsonConvert.SerializeObject(data);
                    List<Claim> claims = new List<Claim>() {
                         new Claim(ClaimTypes.Name,user.UserName),
                         new Claim(ClaimTypes.Email, user.Email),
                         //new Claim("UnitName", user.UnitLocation),
                         //new Claim("PlantName", user.plantname),
                         new Claim("Menu", jsonData),
                         new Claim(ClaimTypes.Role,user.UserRole)
                     };

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    AuthenticationProperties properties = new AuthenticationProperties()
                    {
                        AllowRefresh = false,
                        //IsPersistent = true,
                
                        IsPersistent = user.KeepLoogedIn
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);
                    _notyfyService.Success("Login Successfully !");
                    return RedirectToAction("Index", "Home");
                    //if (user.UserPassward == loginPage.UserPassward)
                    //{
                    //    HttpContext.Session.SetString("username", loginPage.UserName.Trim().ToLower());
                    //    HttpContext.Session.SetString("DEPT_NAME", user.UserDept.Trim());
                    //    HttpContext.Session.SetString("Role", user.UserRole.ToString().Trim());

                    //    return RedirectToAction("Index", "Home");
                    //}
                    //else
                    //{
                    //    _notyfyService.Warning("Invalid Password !");
                    //    return RedirectToAction("create", "LoginPages");
                    //}
                }
                else
                {
                    _notyfyService.Warning("Invalid UserName!");
                    return RedirectToAction("Login", "UserMasters");
                }
            }
            catch (Exception ex)
            {
                //TempData["ExMessage"] = ex.Message;
                //TempData["ExType"] = "error";
                _notyfyService.Error(ex.Message);
                return RedirectToAction("Login", "UserMasters");
            }
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.UserMaster.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userMaster = await _context.UserMaster
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userMaster == null)
            {
                return NotFound();
            }

            return View(userMaster);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserMaster userMaster)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userMaster);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userMaster);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userMaster = await _context.UserMaster.FindAsync(id);
            if (userMaster == null)
            {
                return NotFound();
            }
            return View(userMaster);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  UserMaster userMaster)
        {
            try
            {
                userMaster.ModifiedDate = DateTime.Now.ToString("dd-MM-yyyy");
                userMaster.Modifiedtime = DateTime.Now.ToString("HH:mm");
                //userMaster.User = "admin";
                _context.Update(userMaster);
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

            var userMaster = await _context.UserMaster
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userMaster == null)
            {
                return NotFound();
            }

            return View(userMaster);
        }

        private bool UserMasterExists(int id)
        {
            return _context.UserMaster.Any(e => e.Id == id);
        }
    }
}
