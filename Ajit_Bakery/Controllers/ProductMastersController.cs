using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ajit_Bakery.Data;
using Ajit_Bakery.Models;
using System.Text.RegularExpressions;
using NuGet.Common;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Ajit_Bakery.Controllers
{
    [Authorize]
    public class ProductMastersController : Controller
    {
        private readonly DataDBContext _context;

        public ProductMastersController(DataDBContext context)
        {
            _context = context;
        }

        public IActionResult GETDialCodes(double Unitqty)
        {
            var data = _context.DialMaster
                .Where(a => a.DialUsedForCakes >= Unitqty)
                .Select(a => a.DialCode.Trim())
                .ToList();

            return Json(new { success = true, data = data });
        }

        public async Task<IActionResult> Index()
        {
            var list = _context.ProductMaster.ToList();
            //if(list.Count > 0)
            //{    
            //    foreach(var item in list)
            //    {
            //        var Unitqtyuom = item.Unitqty + " Gms";
            //        item.Unitqtyuom = Unitqtyuom;
            //    }
            //}
            list = list.OrderByDescending(a => a.Id).ToList();

            return View(list);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productMaster = await _context.ProductMaster
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productMaster == null)
            {
                return NotFound();
            }
            var Unitqtyuom = productMaster.Unitqty + " Gms";
            productMaster.Unitqtyuom = Unitqtyuom;
            return View(productMaster);
        }

        private List<SelectListItem> GetDial()
        {
            var lstProducts = new List<SelectListItem>();
            lstProducts = _context.DialMaster
                         .Select(n => n.DialShape.Trim())
                         .Distinct()
                         .Select(uom => new SelectListItem
                         {
                             Value = uom,
                             Text = uom
                         }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "--Select Dial--"
            };

            lstProducts.Insert(0, defItem);
            return lstProducts;
        }

        public IActionResult Create()
        {
            ViewBag.Dial = GetDial();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductMaster productMaster)
        {

            try
            {
                if (productMaster.ProductName != null)
                {
                    var exist = _context.ProductMaster.Where(a => a.ProductName.Trim() == productMaster.ProductName.Trim()).FirstOrDefault();
                    if (exist != null)
                    {
                        return Json(new { success = false, message = "Already Exist ! " });
                    }
                }
                var currentuser1 = HttpContext.User;
                string username = currentuser1.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name).Value;


                int maxId = _context.ProductMaster.Any() ? _context.ProductMaster.Max(e => e.Id) + 1 : 1;
                productMaster.Id = maxId;
                productMaster.ProductName = productMaster.ProductName ;
                //productMaster.ProductName = productMaster.ProductName + " (" + productMaster.Qty + productMaster.Uom + ") ";
                productMaster.CreateDate = DateTime.Now.ToString("dd-MM-yyyy");
                productMaster.ModifiedDate = DateTime.Now.ToString("dd-MM-yyyy");
                productMaster.Createtime = DateTime.Now.ToString("HH:mm");
                productMaster.Modifiedtime = DateTime.Now.ToString("HH:mm");
                productMaster.User = username;

                _context.Add(productMaster);
                await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
                return Json(new {success = true, message = "Created Successfully !" });
            }
            catch(Exception ex)
            {
                //return RedirectToAction(nameof(Index));
                return Json(new { success = false, message = "Warning : "+ ex.Message });
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productMaster = await _context.ProductMaster.FindAsync(id);
            if (productMaster == null)
            {
                return NotFound();
            }
            return View(productMaster);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductMaster productMaster)
        {
            try
            {
                productMaster.ModifiedDate = DateTime.Now.ToString("dd-MM-yyyy");
                productMaster.Modifiedtime = DateTime.Now.ToString("HH:mm");
                productMaster.User = "admin";
                _context.Update(productMaster);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Updated Successfully !" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Warning : " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var productMaster = await _context.ProductMaster
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (productMaster == null)
                {
                    return Json(new { success = false, message = "Data not found in master ! "});
                }
                else
                {
                    _context.ProductMaster.Remove(productMaster);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Deleted Successfully !" });

                }
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = "WWarning : "+ex.Message });
            }
        }
        private bool ProductMasterExists(int id)
        {
            return _context.ProductMaster.Any(e => e.Id == id);
        }

        [HttpGet]
        public ActionResult GETPROD_CODE(string selectedvalue)
        {
            if (selectedvalue == null)
            {
                return Json(new { success = false, message = "Please enter the description!" });
            }
            if (!string.IsNullOrWhiteSpace(selectedvalue))
            {
                string normalizedProductCode = Regex.Replace(selectedvalue.Trim().ToUpper(), @"\s+", "");

                var find = _context.ProductMaster
                                   .FirstOrDefault(a => a.ProductName.Trim().ToUpper() == normalizedProductCode);
                if (find != null)
                {
                    return Json(new { success = false, message = "Product description already exists in the database." });
                }
            }

            selectedvalue = selectedvalue.Trim();
            var prefix = "AB";

            var getLast = _context.ProductMaster
                                  .Where(a => a.ProductCode != "-")
                                  .OrderByDescending(a => a.Id)
                                  .Select(a => a.ProductCode.Trim())
                                  .FirstOrDefault();

            int maxId = 1; // Default starting value if no product codes exist
            if (!string.IsNullOrEmpty(getLast) && getLast.Length > 3)
            {
                string digits = getLast.Substring(4);
                if (int.TryParse(digits, out int lastId))
                {
                    maxId = lastId + 1;
                }
            }

            // Format the new product code
            string code = $"{prefix}{maxId:D5}"; // Ensures 5-digit padding (e.g., AJIT00001, AJIT00002)

            TempData["GeneratedProductCode"] = code;
            return Json(new { success = true, code });
        }

    }
}
