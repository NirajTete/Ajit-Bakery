using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ajit_Bakery.Data;
using Ajit_Bakery.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Ajit_Bakery.Controllers
{
    [Authorize]
    public class UserManagmentsController : Controller
    {
        public INotyfService _notyfyService { get; }

        private readonly DataDBContext _context;
        public UserManagmentsController(DataDBContext context, INotyfService notyfyService)
        {
            _context = context;
            _notyfyService = notyfyService;

        }
        public class UserCheckResult
        {
            public string UserName { get; set; }
            public bool IsFound { get; set; }
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _context.UserMaster
                .Where(u => u.Email != "admin@gmail.com")
                .Select(u => u.UserName.Trim()) // Trim directly in query
                .Distinct()
                .ToListAsync();

            // Convert list to a HashSet for quick lookup
            var userSet = users.ToHashSet();

            var results = users.Select(userName => new UserCheckResult
            {
                UserName = userName,
                IsFound = userSet.Contains(userName)
            }).ToList();

            ViewBag.MyList = results; // Pass the processed data to View

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PageAllot()
        {
            var users = await _context.UserMaster
                .Where(u => u.Email != "admin@gmail.com")
                .Select(u => u.UserName.Trim()) // Trim directly in query
                .Distinct()
                .ToListAsync();

            // Convert list to a HashSet for quick lookup
            var userSet = users.ToHashSet();

            var results = users.Select(userName => new UserCheckResult
            {
                UserName = userName,
                IsFound = userSet.Contains(userName)
            }).ToList();

            ViewBag.MyList = results; // Pass the processed data to View

            return View();
        }

        private List<SelectListItem> GetMainMenu()
        {
            var lstProducts = new List<SelectListItem>();
            lstProducts = _context.MenuModel.Where(a => a.ParentMenuId == 0).AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.Title,
                Text = n.Title
            }).ToList();

            return lstProducts;
        }
        private List<SelectListItem> GetSubMenu()
        {
            var lstProducts = new List<SelectListItem>();
            lstProducts = _context.MenuModel.Where(a => a.ParentMenuId == 2).AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.Title,
                Text = n.Title
            }).ToList();

            return lstProducts;
        }
        private List<SelectListItem> GetOprationMenu()
        {
            var lstProducts = new List<SelectListItem>();
            lstProducts = _context.MenuModel.Where(a => a.ParentMenuId == 9).AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.Title,
                Text = n.Title
            }).ToList();

            return lstProducts;
        }
        private List<SelectListItem> GetMarketingMenu()
        {
            var lstProducts = new List<SelectListItem>();
            lstProducts = _context.MenuModel.Where(a => a.ParentMenuId == 19).AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.Title,
                Text = n.Title
            }).ToList();

            return lstProducts;
        }
        private List<SelectListItem> GetReportMenu()
        {
            var lstProducts = new List<SelectListItem>();
            lstProducts = _context.MenuModel.Where(a => a.ParentMenuId == 36).AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.Title,
                Text = n.Title
            }).ToList();

            return lstProducts;
        }
        private List<SelectListItem> GetRole()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.UserType.AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.designation,
                Text = n.designation
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select Role----"
            };

            lstProducts.Insert(0, defItem);
            //// ✅ Correct way to remove "Admin" from the list
            //var adminItem = lstProducts.FirstOrDefault(x => x.Text == "Admin");
            //if (adminItem != null)
            //{
            //    lstProducts.Remove(adminItem);
            //}
            return lstProducts;
        }
        public IActionResult Create(string? username)
        {
            //ViewBag.role= GetRole();
            //return View();

            ViewBag.role = GetRole();
            ViewBag.MainMenu = GetMainMenu();
            ViewBag.SubMenu = GetSubMenu();
            ViewBag.OprationMenu = GetOprationMenu();
            ViewBag.reportmenu = GetReportMenu();
            //ViewBag.WmarketingMenu = GetMarketingMenu();

          /*  if (username == null || _context.UserManagment == null)
            {
                return NotFound();
            }*/

            var userManagement = _context.UserManagment.Where(a => a.UserName == username).FirstOrDefault();
            if (userManagement == null)
            {
                return View();
            }
            return View(userManagement);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string[] mainmenu, string[] submenu, string[] oprationmenu,
            string[] reportmenu, string[] marketingmenu, UserManagment userManagement)
        {

            var previewdata = _context.UserManagment.Where(a => a.UserName == userManagement.UserName).ToList();

            _context.RemoveRange(previewdata);

            _context.SaveChanges();

            // Add new records based on the fruitIds
            foreach (var fruit in mainmenu)
            {
                int maxId = _context.UserManagment.Any() ? _context.UserManagment.Max(e => e.Id) + 1 : 1;
                userManagement.Id = maxId;
                var newUserManagement = new UserManagment
                {
                    UserName = userManagement.UserName,
                    PageName = fruit,
                    Role = userManagement.Role,
                    Id = maxId,
                };

                _context.Add(newUserManagement);
                _context.SaveChanges();
            }

            foreach (var fruit in submenu)
            {
                int maxId = _context.UserManagment.Any() ? _context.UserManagment.Max(e => e.Id) + 1 : 1;
                userManagement.Id = maxId;
                var newUserManagement = new UserManagment
                {
                    UserName = userManagement.UserName,
                    PageName = fruit,
                    Role = userManagement.Role,
                    Id = maxId,
                };

                _context.Add(newUserManagement);
                _context.SaveChanges();
            }

            foreach (var fruit in oprationmenu)
            {
                int maxId = _context.UserManagment.Any() ? _context.UserManagment.Max(e => e.Id) + 1 : 1;
                userManagement.Id = maxId;
                var newUserManagement = new UserManagment
                {
                    UserName = userManagement.UserName,
                    PageName = fruit,
                    Role = userManagement.Role,
                    Id = maxId,
                };

                _context.Add(newUserManagement);
                _context.SaveChanges();
            }

            foreach (var fruit in marketingmenu)
            {
                int maxId = _context.UserManagment.Any() ? _context.UserManagment.Max(e => e.Id) + 1 : 1;
                userManagement.Id = maxId;
                var newUserManagement = new UserManagment
                {
                    UserName = userManagement.UserName,
                    PageName = fruit,
                    Role = userManagement.Role,
                    Id = maxId,
                };

                _context.Add(newUserManagement);
                _context.SaveChanges();
            }

            foreach (var fruit in reportmenu)
            {
                int maxId = _context.UserManagment.Any() ? _context.UserManagment.Max(e => e.Id) + 1 : 1;
                userManagement.Id = maxId;
                var newUserManagement = new UserManagment
                {
                    UserName = userManagement.UserName,
                    PageName = fruit,
                    Role = userManagement.Role,
                    Id = maxId,
                };

                _context.Add(newUserManagement);
                _context.SaveChanges();
            }

            //maintain logs
            //ViewData["UserID"] = _userManager.GetUserId(this.User);
            //string username = _userManager.GetUserName(this.User);
            //var logs = new Logs();
            //logs.pagename = "User Managment";
            //logs.action = "Create";
            //logs.task = "Allot Page Access";
            //logs.taskid = userManagement.Id;
            //logs.date = DateTime.Now.ToString("dd-MM-yyyy");
            //logs.time = DateTime.Now.ToString("HH:mm:ss");
            //logs.username = username;
            //_context.Add(logs);


            // Save changes to the database
            _notyfyService.Success("Pages Alloted Successfully !");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string? username)
        {
            ViewBag.role = GetRole();
            ViewBag.MainMenu = GetMainMenu();
            ViewBag.SubMenu = GetSubMenu();
            ViewBag.OprationMenu = GetOprationMenu();
            ViewBag.reportmenu = GetReportMenu();
            ViewBag.WmarketingMenu = GetMarketingMenu();

            if (username == null || _context.UserManagment == null)
            {
                return NotFound();
            }
            List<string> accesslist = _context.UserManagment.Where(a => a.UserName == username).Select(a => a.PageName).ToList();
            var role = _context.UserManagment.Where(a => a.UserName == username).Select(a => a.Role).FirstOrDefault();

            var usermanage = new UserManagment
            {
                UserName = username,
                PageName = "",
                selectedpages = accesslist,
                Role = role,
            };

            if (usermanage == null)
            {
                return View();
            }
            return View(usermanage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string[] mainmenu, string[] submenu, string[] oprationmenu,
            string[] reportmenu, string[] marketingmenu, UserManagment userManagement)
        {
            if(userManagement.Role == null)
            {
                userManagement.Role = "User";
            }
            var previewdata = _context.UserManagment.Where(a => a.UserName == userManagement.UserName).ToList();

            _context.RemoveRange(previewdata);

            _context.SaveChanges();

            // Add new records based on the fruitIds
            foreach (var fruit in mainmenu)
            {
                int maxId = _context.UserManagment.Any() ? _context.UserManagment.Max(e => e.Id) + 1 : 1;
                userManagement.Id = maxId;
                var newUserManagement = new UserManagment
                {
                    Id = maxId,
                    UserName = userManagement.UserName,
                    PageName = fruit,
                    Role = userManagement.Role,
                };

                _context.Add(newUserManagement);
                _context.SaveChanges();
            }

            foreach (var fruit in submenu)
            {
                int maxId = _context.UserManagment.Any() ? _context.UserManagment.Max(e => e.Id) + 1 : 1;
                userManagement.Id = maxId;
                var newUserManagement = new UserManagment
                {
                    Id = maxId,
                    UserName = userManagement.UserName,
                    PageName = fruit,
                    Role = userManagement.Role,
                };

                _context.Add(newUserManagement);
                _context.SaveChanges();
            }

            foreach (var fruit in oprationmenu)
            {
                int maxId = _context.UserManagment.Any() ? _context.UserManagment.Max(e => e.Id) + 1 : 1;
                userManagement.Id = maxId;
                var newUserManagement = new UserManagment
                {
                    Id = maxId,
                    UserName = userManagement.UserName,
                    PageName = fruit,
                    Role = userManagement.Role,
                };

                _context.Add(newUserManagement);
                _context.SaveChanges();
            }

            foreach (var fruit in reportmenu)
            {
                int maxId = _context.UserManagment.Any() ? _context.UserManagment.Max(e => e.Id) + 1 : 1;
                userManagement.Id = maxId;
                var newUserManagement = new UserManagment
                {
                    Id = maxId,
                    UserName = userManagement.UserName,
                    PageName = fruit,
                    Role = userManagement.Role,
                };

                _context.Add(newUserManagement);
                _context.SaveChanges();
            }

            foreach (var fruit in marketingmenu)
            {
                int maxId = _context.UserManagment.Any() ? _context.UserManagment.Max(e => e.Id) + 1 : 1;
                userManagement.Id = maxId;
                var newUserManagement = new UserManagment
                {
                    UserName = userManagement.UserName,
                    PageName = fruit,
                    Role = userManagement.Role,
                    Id = maxId,
                };

                _context.Add(newUserManagement);
                _context.SaveChanges();
            }


            //maintain logs
            var currentuser = HttpContext.User;
            //ViewData["UserID"] = currentuser.Claims.FirstOrDefault(a=>a.Type == "MenuId").Value;
            string username = currentuser.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name).Value;
            //var logs = new Logs();
            //logs.pagename = "User Managment";
            //logs.action = "Create";
            //logs.task = "Allot Page Access";
            //logs.taskid = userManagement.Id;
            //logs.date = DateTime.Now.ToString("dd-MM-yyyy");
            //logs.time = DateTime.Now.ToString("HH:mm:ss");
            //logs.username = username;
            //_context.Add(logs);
            _context.SaveChanges();
            _notyfyService.Success("Updated Pages Alloted Successfully !");
            // Save changes to the database

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            int maxId = _context.UserMaster.Any() ? _context.UserMaster.Max(e => e.Id) + 1 : 1;
            ViewBag.usercode = "U"+ maxId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserMaster user_master)
        {
            try
            {
                if (await _context.UserMaster.AnyAsync(u => u.UserName == user_master.UserName))
                {
                    ModelState.AddModelError("", "User already exists.");
                    return View(user_master);
                }
                int newId = (_context.UserMaster.Max(u => (int?)u.Id) ?? 0) + 1;
                DateTime currentDateTime = DateTime.Now;
                ViewBag.usercode = "U" + newId;
                user_master.Id = newId;
                user_master.CreateDate = DateTime.Now.ToString("dd-MM-yyyy");
                user_master.ModifiedDate = DateTime.Now.ToString("dd-MM-yyyy");
                user_master.Createtime = DateTime.Now.ToString("HH:mm");
                user_master.Modifiedtime = DateTime.Now.ToString("HH:mm");
                user_master.KeepLoogedIn = true;

                _context.UserMaster.Add(user_master);
                await _context.SaveChangesAsync();

                _notyfyService.Success("Added Successfully !");
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while processing your request.");
                return View(user_master);
            }
        }
        private bool UserManagmentExists(int id)
        {
            return _context.UserManagment.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string username) // Changed parameter type from string? to string
        {
            if (string.IsNullOrEmpty(username)) // Check if username is null or empty
            {
                return NotFound();
            }
            var currentuser = HttpContext.User;
            string username1 = currentuser.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name).Value;

            var aspUser = await _context.UserMaster.Where(a => a.UserName.Trim() == username.Trim()).FirstOrDefaultAsync(); // Find the ASP.NET Core Identity user

            if (aspUser != null)
            {
                var userManagement = _context.UserManagment.Where(m => m.UserName == username).ToList();
                _context.UserManagment.RemoveRange(userManagement);
                _context.UserMaster.Remove(aspUser); // Delete the ASP.NET Core Identity user
                _context.SaveChangesAsync(); // Save changes to the UserManagement table
            }

            return Json(new { success = true, message = "User Removed Successfully !" });
        }

    }
}
