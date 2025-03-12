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

namespace Ajit_Bakery.Controllers
{
    public class ProductMastersController : Controller
    {
        private readonly DataDBContext _context;

        public ProductMastersController(DataDBContext context)
        {
            _context = context;
        }

        // GET: ProductMasters
        public async Task<IActionResult> Index()
        {
            var list = _context.ProductMaster.ToList();
            if(list.Count > 0)
            {    
                foreach(var item in list)
                {
                    var Unitqtyuom = item.Unitqty + " Gms";
                    item.Unitqtyuom = Unitqtyuom;
                }
            }
            list = list.OrderByDescending(a => a.Id).ToList();

            return View(list);
        }

        // GET: ProductMasters/Details/5
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


        // GET: ProductMasters/Create
        public IActionResult Create()
        {
            ViewBag.Dial = GetDial();
            return View();
        }

        // POST: ProductMasters/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductMaster productMaster)
        {

            try
            {
                productMaster.ProductName = productMaster.ProductName + " (" + productMaster.Qty + productMaster.Uom + ") ";
                productMaster.CreateDate = DateTime.Now.ToString("dd-MM-yyyy");
                productMaster.ModifiedDate = DateTime.Now.ToString("dd-MM-yyyy");
                productMaster.Createtime = DateTime.Now.ToString("HH:mm");
                productMaster.Modifiedtime = DateTime.Now.ToString("HH:mm");
                productMaster.User = "admin";

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

        // GET: ProductMasters/Edit/5
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

        // GET: ProductMasters/Delete/5
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
            var prefix = "AJIT";

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

        //public ActionResult GETPROD_CODE(string selectedvalue)
        //{
        //    if (selectedvalue == null)
        //    {
        //        return Json(new { success = false, message = "Please enter the description !" });
        //    }
        //    if (!string.IsNullOrWhiteSpace(selectedvalue))
        //    {
        //        string normalizedProductCode = Regex.Replace(selectedvalue.Trim().ToUpper(), @"\s+", "");

        //        var find = _context.ProductMaster.Where(a => a.ProductName.Trim().ToUpper() == selectedvalue.Trim().ToUpper()).FirstOrDefault();
        //        if (find != null)
        //        {
        //            return Json(new { success = false, message = "Product description already exists in the database." });
        //        }
        //    }

        //    selectedvalue.Trim();
        //    //var prefix = _context.companymaster.Select(a => a.cprefix.Trim()).FirstOrDefault();
        //    var prefix = "AJIT";
        //    var getLast = _context.ProductMaster
        //                        .OrderByDescending(a => a.Id).Where(a => a.ProductCode != "-")
        //                        .Select(a => a.ProductCode.Trim())
        //                        .FirstOrDefault();

        //    string letters = getLast.Substring(0, 3);

        //    string digits = getLast.Substring(3);
        //    int maxId = int.Parse(digits) + 1; // Increment maxId by 1

        //    var categoryId = prefix + "-" + maxId;

        //    var srno = maxId;
        //    var code = "";
        //    if (srno.ToString().Length == 1)
        //    {
        //        code = prefix + "0000" + maxId;
        //    }
        //    else if (srno.ToString().Length == 2)
        //    {
        //        code = prefix + "000" + maxId;
        //    }
        //    else if (srno.ToString().Length == 3)
        //    {
        //        code = prefix + "00" + maxId;
        //    }
        //    else if (srno.ToString().Length == 4)
        //    {
        //        code = prefix + "0" + maxId;
        //    }
        //    else if (srno.ToString().Length == 5)
        //    {
        //        code = prefix + maxId;
        //    }

        //    TempData["GeneratedProductCode"] = code;

        //    return Json(new { success = true, code });
        //}
    }
}
