using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ajit_Bakery.Data;
using Ajit_Bakery.Models;
using Microsoft.AspNetCore.Identity;

namespace Ajit_Bakery.Controllers
{
    public class UserManagmentsController : Controller
    {
        private readonly DataDBContext _context;

        public UserManagmentsController(DataDBContext context)
        {
            _context = context;
        }

        // GET: UserManagments
        public class UserCheckResult
        {
            public string UserName { get; set; }
            public bool IsFound { get; set; }
        }
       
        public async Task<IActionResult> Index()
        {
            var users = await _context.UserManagment
                .Where(u => u.UserName != "admin@gmail.com")
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

        //// GET: UserManagements/Create  -  get main menu
        //private List<SelectListItem> GetMainMenu()
        //{
        //    var lstProducts = new List<SelectListItem>();
        //    lstProducts = _context.MenuModel.Where(a => a.ParentMenuId == 0).AsNoTracking().Select(n =>
        //    new SelectListItem
        //    {
        //        Value = n.Title,
        //        Text = n.Title
        //    }).ToList();

        //    return lstProducts;
        //}
        //// GET: UserManagements/Create  -  get sub menu
        //private List<SelectListItem> GetSubMenu()
        //{
        //    var lstProducts = new List<SelectListItem>();
        //    lstProducts = _context.MenuModel.Where(a => a.ParentMenuId == 2).AsNoTracking().Select(n =>
        //    new SelectListItem
        //    {
        //        Value = n.Title,
        //        Text = n.Title
        //    }).ToList();

        //    return lstProducts;
        //}
        //// GET: UserManagements/Create  -  get opration menu
        //private List<SelectListItem> GetOprationMenu()
        //{
        //    var lstProducts = new List<SelectListItem>();
        //    lstProducts = _context.MenuModel.Where(a => a.ParentMenuId == 11).AsNoTracking().Select(n =>
        //    new SelectListItem
        //    {
        //        Value = n.Title,
        //        Text = n.Title
        //    }).ToList();

        //    return lstProducts;
        //}
        //private List<SelectListItem> GetMarketingMenu()
        //{
        //    var lstProducts = new List<SelectListItem>();
        //    lstProducts = _context.MenuModel.Where(a => a.ParentMenuId == 52).AsNoTracking().Select(n =>
        //    new SelectListItem
        //    {
        //        Value = n.Title,
        //        Text = n.Title
        //    }).ToList();

        //    return lstProducts;
        //}
        //private List<SelectListItem> GetReportMenu()
        //{
        //    var lstProducts = new List<SelectListItem>();
        //    lstProducts = _context.MenuModel.Where(a => a.ParentMenuId == 36).AsNoTracking().Select(n =>
        //    new SelectListItem
        //    {
        //        Value = n.Title,
        //        Text = n.Title
        //    }).ToList();

        //    return lstProducts;
        //}
        ////create view show
        //private List<SelectListItem> GetRole()
        //{
        //    var lstProducts = new List<SelectListItem>();

        //    lstProducts = _context.usertypeMaster.AsNoTracking().Select(n =>
        //    new SelectListItem
        //    {
        //        Value = n.designation,
        //        Text = n.designation
        //    }).ToList();

        //    var defItem = new SelectListItem()
        //    {
        //        Value = "",
        //        Text = "----Select Role----"
        //    };

        //    lstProducts.Insert(0, defItem);

        //    return lstProducts;
        //}
        //public IActionResult Create(string? username)
        //{
        //    //ViewBag.role= GetRole();
        //    //return View();

        //    ViewBag.role = GetRole();
        //    ViewBag.MainMenu = GetMainMenu();
        //    ViewBag.SubMenu = GetSubMenu();
        //    ViewBag.OprationMenu = GetOprationMenu();
        //    ViewBag.reportmenu = GetReportMenu();
        //    ViewBag.WmarketingMenu = GetMarketingMenu();

        //    if (username == null || _context.UserManagement == null)
        //    {
        //        return NotFound();
        //    }

        //    var userManagement = _context.UserManagement.Where(a => a.UserName == username).FirstOrDefault();
        //    if (userManagement == null)
        //    {
        //        return View();
        //    }
        //    return View(userManagement);

        //}

        //// POST: UserManagements/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(string[] mainmenu, string[] submenu, string[] oprationmenu,
        //    string[] reportmenu, string[] marketingmenu, UserManagment userManagement)
        //{

        //    var previewdata = _context.UserManagement.Where(a => a.UserName == userManagement.UserName).ToList();

        //    _context.RemoveRange(previewdata);

        //    _context.SaveChanges();

        //    // Add new records based on the fruitIds
        //    foreach (var fruit in mainmenu)
        //    {
        //        int maxId = _context.UserManagement.Any() ? _context.UserManagement.Max(e => e.Id) + 1 : 1;
        //        userManagement.Id = maxId;
        //        var newUserManagement = new UserManagment
        //        {
        //            UserName = userManagement.UserName,
        //            PageName = fruit,
        //            Role = userManagement.Role,
        //            Id = maxId,
        //        };

        //        _context.Add(newUserManagement);
        //        _context.SaveChanges();
        //    }

        //    foreach (var fruit in submenu)
        //    {
        //        int maxId = _context.UserManagement.Any() ? _context.UserManagement.Max(e => e.Id) + 1 : 1;
        //        userManagement.Id = maxId;
        //        var newUserManagement = new UserManagment
        //        {
        //            UserName = userManagement.UserName,
        //            PageName = fruit,
        //            Role = userManagement.Role,
        //            Id = maxId,
        //        };

        //        _context.Add(newUserManagement);
        //        _context.SaveChanges();
        //    }

        //    foreach (var fruit in oprationmenu)
        //    {
        //        int maxId = _context.UserManagement.Any() ? _context.UserManagement.Max(e => e.Id) + 1 : 1;
        //        userManagement.Id = maxId;
        //        var newUserManagement = new UserManagment
        //        {
        //            UserName = userManagement.UserName,
        //            PageName = fruit,
        //            Role = userManagement.Role,
        //            Id = maxId,
        //        };

        //        _context.Add(newUserManagement);
        //        _context.SaveChanges();
        //    }

        //    foreach (var fruit in marketingmenu)
        //    {
        //        int maxId = _context.UserManagement.Any() ? _context.UserManagement.Max(e => e.Id) + 1 : 1;
        //        userManagement.Id = maxId;
        //        var newUserManagement = new UserManagment
        //        {
        //            UserName = userManagement.UserName,
        //            PageName = fruit,
        //            Role = userManagement.Role,
        //            Id = maxId,
        //        };

        //        _context.Add(newUserManagement);
        //        _context.SaveChanges();
        //    }

        //    foreach (var fruit in reportmenu)
        //    {
        //        int maxId = _context.UserManagement.Any() ? _context.UserManagement.Max(e => e.Id) + 1 : 1;
        //        userManagement.Id = maxId;
        //        var newUserManagement = new UserManagment
        //        {
        //            UserName = userManagement.UserName,
        //            PageName = fruit,
        //            Role = userManagement.Role,
        //            Id = maxId,
        //        };

        //        _context.Add(newUserManagement);
        //        _context.SaveChanges();
        //    }

        //    //maintain logs
        //    //ViewData["UserID"] = _userManager.GetUserId(this.User);
        //    //string username = _userManager.GetUserName(this.User);
        //    //var logs = new Logs();
        //    //logs.pagename = "User Managment";
        //    //logs.action = "Create";
        //    //logs.task = "Allot Page Access";
        //    //logs.taskid = userManagement.Id;
        //    //logs.date = DateTime.Now.ToString("dd-MM-yyyy");
        //    //logs.time = DateTime.Now.ToString("HH:mm:ss");
        //    //logs.username = username;
        //    //_context.Add(logs);


        //    // Save changes to the database
        //    _notyfService.Success("Pages Alloted Successfully !");
        //    return RedirectToAction(nameof(Index));
        //}

        //// GET: UserManagements/Edit/5 : for view bag
        //public async Task<IActionResult> Edit(string? username)
        //{
        //    ViewBag.role = GetRole();
        //    ViewBag.MainMenu = GetMainMenu();
        //    ViewBag.SubMenu = GetSubMenu();
        //    ViewBag.OprationMenu = GetOprationMenu();
        //    ViewBag.reportmenu = GetReportMenu();
        //    ViewBag.WmarketingMenu = GetMarketingMenu();

        //    if (username == null || _context.UserManagement == null)
        //    {
        //        return NotFound();
        //    }
        //    List<string> accesslist = _context.UserManagement.Where(a => a.UserName == username).Select(a => a.PageName).ToList();
        //    var role = _context.UserManagement.Where(a => a.UserName == username).Select(a => a.Role).FirstOrDefault();

        //    var usermanage = new UserManagment
        //    {
        //        UserName = username,
        //        PageName = "",
        //        selectedpages = accesslist,
        //        Role = role,

        //    };

        //    if (usermanage == null)
        //    {
        //        return View();
        //    }
        //    return View(usermanage);
        //}

        //// POST: UserManagements/Edit/5 : display edit page on login
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, string[] mainmenu, string[] submenu, string[] oprationmenu,
        //    string[] reportmenu, string[] marketingmenu, UserManagment userManagement)
        //{

        //    var previewdata = _context.UserManagement.Where(a => a.UserName == userManagement.UserName).ToList();

        //    _context.RemoveRange(previewdata);

        //    _context.SaveChanges();

        //    // Add new records based on the fruitIds
        //    foreach (var fruit in mainmenu)
        //    {
        //        int maxId = _context.UserManagement.Any() ? _context.UserManagement.Max(e => e.Id) + 1 : 1;
        //        userManagement.Id = maxId;
        //        var newUserManagement = new UserManagment
        //        {
        //            Id = maxId,
        //            UserName = userManagement.UserName,
        //            PageName = fruit,
        //            Role = userManagement.Role,
        //        };

        //        _context.Add(newUserManagement);
        //        _context.SaveChanges();
        //    }

        //    foreach (var fruit in submenu)
        //    {
        //        int maxId = _context.UserManagement.Any() ? _context.UserManagement.Max(e => e.Id) + 1 : 1;
        //        userManagement.Id = maxId;
        //        var newUserManagement = new UserManagment
        //        {
        //            Id = maxId,
        //            UserName = userManagement.UserName,
        //            PageName = fruit,
        //            Role = userManagement.Role,
        //        };

        //        _context.Add(newUserManagement);
        //        _context.SaveChanges();
        //    }

        //    foreach (var fruit in oprationmenu)
        //    {
        //        int maxId = _context.UserManagement.Any() ? _context.UserManagement.Max(e => e.Id) + 1 : 1;
        //        userManagement.Id = maxId;
        //        var newUserManagement = new UserManagment
        //        {
        //            Id = maxId,
        //            UserName = userManagement.UserName,
        //            PageName = fruit,
        //            Role = userManagement.Role,
        //        };

        //        _context.Add(newUserManagement);
        //        _context.SaveChanges();
        //    }

        //    foreach (var fruit in reportmenu)
        //    {
        //        int maxId = _context.UserManagement.Any() ? _context.UserManagement.Max(e => e.Id) + 1 : 1;
        //        userManagement.Id = maxId;
        //        var newUserManagement = new UserManagment
        //        {
        //            Id = maxId,
        //            UserName = userManagement.UserName,
        //            PageName = fruit,
        //            Role = userManagement.Role,
        //        };

        //        _context.Add(newUserManagement);
        //        _context.SaveChanges();
        //    }

        //    foreach (var fruit in marketingmenu)
        //    {
        //        int maxId = _context.UserManagement.Any() ? _context.UserManagement.Max(e => e.Id) + 1 : 1;
        //        userManagement.Id = maxId;
        //        var newUserManagement = new UserManagment
        //        {
        //            UserName = userManagement.UserName,
        //            PageName = fruit,
        //            Role = userManagement.Role,
        //            Id = maxId,
        //        };

        //        _context.Add(newUserManagement);
        //        _context.SaveChanges();
        //    }


        //    //maintain logs
        //    ViewData["UserID"] = _userManager.GetUserId(this.User);
        //    string username = _userManager.GetUserName(this.User);
        //    var logs = new Logs();
        //    logs.pagename = "User Managment";
        //    logs.action = "Create";
        //    logs.task = "Allot Page Access";
        //    logs.taskid = userManagement.Id;
        //    logs.date = DateTime.Now.ToString("dd-MM-yyyy");
        //    logs.time = DateTime.Now.ToString("HH:mm:ss");
        //    logs.username = username;
        //    _context.Add(logs);
        //    _context.SaveChanges();
        //    _notyfService.Success("Updated Pages Alloted Successfully !");
        //    // Save changes to the database

        //    return RedirectToAction(nameof(Index));
        //}


        private bool UserManagmentExists(int id)
        {
            return _context.UserManagment.Any(e => e.Id == id);
        }
    }
}
