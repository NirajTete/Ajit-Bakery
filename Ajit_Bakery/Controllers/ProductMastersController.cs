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
using Ajit_Bakery.Services;
using Ajit_Bakery.Models.Tally_Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Notyf;
using NuGet.Packaging;

namespace Ajit_Bakery.Controllers
{
    [Authorize]
    public class ProductMastersController : Controller
    {
        public INotyfService _notyfyService { get; }
        private readonly DataDBContext _context;
        private readonly IConfiguration _config;
        private readonly IApiService _apiService;
        public ProductMastersController(DataDBContext context, IConfiguration config, IApiService apiService, INotyfService notyfyService)
        {
            _context = context;
            //ADDED
            _config = config;
            _apiService = apiService;
            _notyfyService = notyfyService;
        }
        public async Task<IActionResult> WriteDataToTally_category()
        {

            try
            {
                var baseurls = _config["AppSettings:BaseUrl"];
                var Cname = _config["AppSettings:CompanyName"];

                var tallyStatusUrl = $"{baseurls}/GetStatus";
                var companyUrl = $"{baseurls}/Company";

                // Check if Tally is running
                var tallyResponse = await _apiService.GetAsync<ApiResponse<List<string>>>(tallyStatusUrl, null);

                // Check if Company is available
                var companyResponse = await _apiService.GetAsync<ApiResponse<List<string>>>(companyUrl, null);

                if (tallyResponse == null || !tallyResponse.Success)
                {
                    return Json(new { success = false, message = "Tally Server is not running" });
                }

                if (companyResponse == null || !companyResponse.Success)
                {
                    return Json(new { success = false, message = $"{Cname} Company is not Open" });
                }

                if (!companyResponse.Data.Contains(Cname))
                {

                    return Json(new { success = false, message = $"{Cname} Company is not Open" });
                }
            }
            catch (Exception ex)
            {
                _notyfyService.Error("An error occurred while checking Tally and Company status.");
                return RedirectToAction(nameof(Index));  // Ensure redirect even in case of an error
            }

            var GETCATEGORY = _context.ProductMaster.Select(a => a.Category.Trim()).Distinct().ToList();
            List<ProductMaster> category_master = new List<ProductMaster>();
            foreach (var item in GETCATEGORY)
            {
                ProductMaster productMaster = new ProductMaster()
                {
                    Category = item.Trim(),
                };
                category_master.Add(productMaster);
            }
            var localCatList = category_master;

            var baseurl = _config["AppSettings:BaseUrl"];
            var url = $"{baseurl}/StockGroup";
            var response = await _apiService.GetAsync<ApiResponse<List<string>>>(url, null);
            var existingCatSet = new HashSet<string>(response.Data.Select(u => u.ToUpper()));
            var newCats = localCatList.Where(cat => !existingCatSet.Contains(cat.Category)).ToList();

            if (newCats.Count == 0)
            {
                return Json(new { success = false, message = "Already Synced to Tally Server!" });
            }

            var tasks = newCats.Select(item =>
            {
                var cat_val = item.Category.Trim();
                var url1 = $"{baseurl}/StockGroup?stockgroup={cat_val}";
                return _apiService.PostAsync<ApiResponse<string>>(url1, null);
            });

            await Task.WhenAll(tasks);

            return Json(new { success = true, message = "Done" });
        }

        public async Task<IActionResult> WriteDataToTally()
        {
            try
            {
                var baseurls = _config["AppSettings:BaseUrl"];
                var Cname = _config["AppSettings:CompanyName"];

                var tallyStatusUrl = $"{baseurls}/GetStatus";
                var companyUrl = $"{baseurls}/Company";

                // Check if Tally is running
                var tallyResponse = await _apiService.GetAsync<ApiResponse<List<string>>>(tallyStatusUrl, null);

                // Check if Company is available
                var companyResponse = await _apiService.GetAsync<ApiResponse<List<string>>>(companyUrl, null);

                if (tallyResponse == null || !tallyResponse.Success)
                {
                    return Json(new { success = false, message = "Tally Server is not running" });
                }

                if (companyResponse == null || !companyResponse.Success)
                {
                    return Json(new { success = false, message = $"{Cname} Company is not Open" });
                }

                if (!companyResponse.Data.Contains(Cname))
                {

                    return Json(new { success = false, message = $"{Cname} Company is not Open" });
                }
            }
            catch (Exception ex)
            {
                //_notyfyService.Warning(" Tally Server is not running!!.");
                return RedirectToAction(nameof(Index));  // Ensure redirect even in case of an error
            }

            // To check duplicate
            var baseurl = _config["AppSettings:BaseUrl"];
            var url = $"{baseurl}/StockItem";

            var result = await _apiService.GetAsync<ApiResponse<List<StockItem>>>(url, null);

            // Extract all product descriptions into a list
            List<string> existingProduct = result?.Data?.Select(item => item.name?.Trim().ToUpper()).Where(desc => !string.IsNullOrEmpty(desc)).ToList() ?? new List<string>();

            int counter = 0;

            // Fetch product master data
            var list = _context.ProductMaster.ToList();

            // Filter products not already in Tally
            list = list.Where(product => !existingProduct.Contains(product.ProductName?.Trim().ToUpper())).ToList();

            if (list.Count == 0)
            {
                return Json(new { success = false, message = "Already Synced to Tally Server!" });
            }

            try
            {
                foreach (var product in list)
                {
                    //var UNIT = product.Unitqty * 1000;
                    if (product != null)
                    {
                        int cgst = 0, sgst = 0, igst = 0;

                        StockItem data = new StockItem()
                        {
                            name = product.ProductCode?.Trim(),
                            alias = product.ProductName?.Trim(),
                            GUID = "",
                            unit = "KGS",
                            //unit = product.Uom?.Trim(),
                            category = product.Category.Trim(),
                            openingrate = 0,
                            openingqnty = "",
                            hsncode = "",
                            cgst = cgst.ToString().Trim(),
                            sgst = sgst.ToString().Trim(),
                            igst = igst.ToString().Trim()
                        };

                        var url1 = $"{baseurl}/StockItem";
                        var result1 = await _apiService.PostAsync<ApiResponse<string>>(url1, data);

                        counter++;
                    }
                }

                return Json(new { success = true, message = "Done!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        //public async Task<IActionResult> ReadDataToTally1()
        //{

        //    try
        //    {
        //        var baseurls = _config["AppSettings:BaseUrl"];
        //        var Cname = _config["AppSettings:CompanyName"];

        //        var tallyStatusUrl = $"{baseurls}/GetStatus";
        //        var companyUrl = $"{baseurls}/Company";

        //        // Check if Tally is running
        //        var tallyResponse = await _apiService.GetAsync<ApiResponse<List<string>>>(tallyStatusUrl, null);

        //        // Check if Company is available
        //        var companyResponse = await _apiService.GetAsync<ApiResponse<List<string>>>(companyUrl, null);

        //        if (tallyResponse == null || !tallyResponse.Success)
        //        {
        //            return Json(new { success = false, message = "Tally Server is not running" });
        //        }

        //        if (companyResponse == null || !companyResponse.Success)
        //        {
        //            return Json(new { success = false, message = $"{Cname} Company is not Open" });
        //        }

        //        if (!companyResponse.Data.Contains(Cname))
        //        {
        //            return Json(new { success = false, message = $"{Cname} Company is not Open" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //_notyfyService.Warning(" Tally Server is not running!!.");
        //        return RedirectToAction(nameof(Index));  // Ensure redirect even in case of an error
        //    }

        //    try
        //    {
        //        //read system desc
        //        var baseurl = _config["AppSettings:BaseUrl"];
        //        var url = $"{baseurl}/StockItem";
        //        var result = await _apiService.GetAsync<ApiResponse<List<StockItem>>>(url, null);

        //        // Check
        //        var list = _context.ProductMaster.ToList();

        //        foreach (var item in result.Data)
        //        {
        //            var pro = list.Where(a => a.ProductName.Trim() == item.name.Trim()).FirstOrDefault();
        //            if (pro == null)
        //            {
        //                var procode = "";
        //                int maxId = _context.ProductMaster.Any() ? _context.ProductMaster.Max(e => e.id) + 1 : 1;
        //                if (item.alias == null || item.alias == "NA")
        //                {
        //                    procode = GETPROD_CODE1(item.alias);
        //                }
        //                else
        //                {
        //                    procode = item.alias;
        //                }
        //                var catcode = "NA";
        //                var Subcat = "NA";
        //                var cate = _context.category_master.Where(a => a.categoryname.Trim() == item.category.Trim()).FirstOrDefault();
        //                if (cate == null)
        //                {
        //                    item.category = "NA";
        //                }
        //                else
        //                {
        //                    catcode = cate.categoryid;
        //                }

        //                if (string.IsNullOrWhiteSpace(item.unit) || item.unit.Trim().Contains("Applicable", StringComparison.OrdinalIgnoreCase))
        //                {
        //                    item.unit = "No";
        //                }

        //                // Save UOM if not exist
        //                var checkuom = _context.uom.Where(a => a.shortcut.Trim() == item.unit.Trim()).FirstOrDefault();
        //                if (checkuom == null)
        //                {
        //                    int maxiduom = _context.uom.Any() ? _context.uom.Max(e => e.Id) + 1 : 1;
        //                    var newUom = new uom()
        //                    {
        //                        Id = maxiduom,
        //                        shortcut = item.unit, // The shortcut passed in as the parameter
        //                        Name = item.unit,     // You might want to adjust this if you have a specific name for the UOM
        //                    };
        //                    _context.uom.Add(newUom);
        //                    _context.SaveChanges();
        //                }

        //                //Save Product Code
        //                if (procode != null || item.name != null)
        //                {
        //                    var check = _context.Product_Master.Where(a => a.productcode.Trim() == procode.Trim() || a.productdescription.Trim() == item.name.Trim()).FirstOrDefault();
        //                    if (check == null)
        //                    {
        //                        //if(procode == null)
        //                        //{
        //                        //    procode = 
        //                        //}
        //                        Product_Master Product_Master = new Product_Master()
        //                        {
        //                            productcode = procode,
        //                            productdescription = item.name,
        //                            hsncode = item.hsncode,
        //                            uom = item.unit,
        //                            categoryname = item.category,
        //                            id = maxId,
        //                            categorycode = catcode,
        //                            subcategory = Subcat,
        //                            TypeOfProduct = false,
        //                            rate = 0,
        //                            Warranty = "NA",
        //                            categorytype = "Hardware",
        //                            modelno = "NA",
        //                            servicecode = "NA",
        //                            servicedescription = "NA",
        //                            version = 0,
        //                            label = 1,
        //                            minlevel = 1,
        //                            maxlevel = 10,
        //                            GST = Convert.ToInt32(item.igst),
        //                        };
        //                        _context.Product_Master.Add(Product_Master);
        //                        _context.SaveChanges();
        //                    }
        //                }
        //                else
        //                {

        //                }
        //            }
        //            else
        //            {
        //            }
        //        }

        //        var listdata1 = _context.Product_Master.OrderByDescending(a => a.id).ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        _notyfyService.Error("An unexpected error occurred.");
        //        Console.WriteLine($"Unexpected error: {ex.Message}");
        //    }


        //    return Json(new { success = true, message = "Done !" });

        //}

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
                Text = "-- Select Dial --",
                Selected = true,
                Disabled = true
            };

            lstProducts.Insert(0, defItem);
            return lstProducts;
        }

        public async Task<IActionResult> Create()
        {
            //TALLY ADDED
            try
            {
                var baseurls = _config["AppSettings:BaseUrl"];
                var Cname = _config["AppSettings:CompanyName"];

                var tallyStatusUrl = $"{baseurls}/GetStatus";
                var companyUrl = $"{baseurls}/Company";

                // Check if Tally is running
                var tallyResponse = await _apiService.GetAsync<ApiResponse<List<string>>>(tallyStatusUrl, null);

                // Check if Company is available
                var companyResponse = await _apiService.GetAsync<ApiResponse<List<string>>>(companyUrl, null);

                if (tallyResponse == null || !tallyResponse.Success)
                {
                    _notyfyService.Error("Tally Server is not running!");
                    return RedirectToAction(nameof(Index));  // Corrected return
                }

                if (companyResponse == null || !companyResponse.Success)
                {
                    _notyfyService.Warning("Tally is running, but the Company is not selected or available.");
                    return RedirectToAction(nameof(Index));  // Corrected return
                }

                if (!companyResponse.Data.Contains(Cname))
                {
                    _notyfyService.Warning($"{Cname} Company is not Open.");
                    return RedirectToAction(nameof(Index));  // Corrected return
                }
            }
            catch (Exception ex)
            {
                _notyfyService.Warning(" Tally Server is not running!!.");
                return RedirectToAction(nameof(Index));  // Ensure redirect even in case of an error
            }
            //ENDED

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
                productMaster.ProductName = productMaster.ProductName;
                //productMaster.ProductName = productMaster.ProductName + " (" + productMaster.Qty + productMaster.Uom + ") ";
                productMaster.CreateDate = DateTime.Now.ToString("dd-MM-yyyy");
                productMaster.ModifiedDate = DateTime.Now.ToString("dd-MM-yyyy");
                productMaster.Createtime = DateTime.Now.ToString("HH:mm");
                productMaster.Modifiedtime = DateTime.Now.ToString("HH:mm");
                productMaster.User = username;
                _context.Add(productMaster);
                await _context.SaveChangesAsync();

                //TALLY ADDED
                var baseurl = _config["AppSettings:BaseUrl"];
                var url = $"{baseurl}/StockItem";
                var result = await _apiService.GetAsync<ApiResponse<List<StockItem>>>(url, null);
                List<string> existinguom = result?.Data?.Select(item => item.alias?.Trim()).Where(desc => !string.IsNullOrEmpty(desc)).ToList() ?? new List<string>();
                var product = _context.ProductMaster.Where(a => a.ProductName.Trim() == productMaster.ProductName.Trim()).FirstOrDefault();
                if (product != null)
                {
                    int cgst = 0;
                    int sgst = 0;
                    int igst = 0;

                    if (existinguom.Count > 0)
                    {
                        if (!existinguom.Contains(product.ProductName.Trim(), StringComparer.OrdinalIgnoreCase))
                        {
                            try
                            {
                                string bal = "0";
                                StockItem data = new StockItem()
                                {
                                    name = product.ProductCode.Trim(),
                                    alias = product.ProductName.Trim(),
                                    GUID = "",
                                    //unit = product.Uom.Trim(),
                                    unit = "KGS",
                                    category = product.Category.Trim(),
                                    openingrate = 0,
                                    openingqnty = bal.Trim(),
                                    hsncode = "",
                                    cgst = cgst.ToString().Trim(),
                                    sgst = sgst.ToString().Trim(),
                                    igst = igst.ToString().Trim()
                                };
                                var url1 = $"{baseurl}/StockItem";
                                var result1 = await _apiService.PostAsync<ApiResponse<string>>(url1, data);

                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            string bal = "0";
                            StockItem data = new StockItem()
                            {
                                name = product.ProductCode.Trim(),
                                alias = product.ProductName.Trim(),
                                GUID = "",
                                //unit = product.Uom.Trim(),
                                unit = "KGS",
                                category = product.Category.Trim(),
                                openingrate = 0,
                                openingqnty = bal.Trim(),
                                hsncode = "",
                                cgst = cgst.ToString().Trim(),
                                sgst = sgst.ToString().Trim(),
                                igst = igst.ToString().Trim()

                            };
                            var url2 = $"{baseurl}/StockItem";
                            var result2 = await _apiService.PostAsync<ApiResponse<string>>(url2, data);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                //ENDED


                //return RedirectToAction(nameof(Index));
                return Json(new { success = true, message = "Created Successfully !" });
            }
            catch (Exception ex)
            {
                //return RedirectToAction(nameof(Index));
                return Json(new { success = false, message = "Warning : " + ex.Message });
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
                    return Json(new { success = false, message = "Data not found in master ! " });
                }
                else
                {
                    _context.ProductMaster.Remove(productMaster);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Deleted Successfully !" });

                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "WWarning : " + ex.Message });
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
