using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ajit_Bakery.Data;
using Ajit_Bakery.Models;
using System.Net.Sockets;
using System.Net;
using System.Text;
using NuGet.Configuration;
using DocumentFormat.OpenXml.Bibliography;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office.CustomUI;
using System.Security.Claims;

namespace Ajit_Bakery.Controllers
{
    [Authorize]
    public class SaveProductionsController : Controller
    {
        private readonly DataDBContext _context;
        private IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _config;

        public SaveProductionsController(DataDBContext context, IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            _context = context;
            _config = config;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult checkvalue(string Production_Id, string productName, double TotalNetWg, int DialTierWg, double ProductGrossWg)
        {
            if (double.IsNaN(TotalNetWg))
            {
                TotalNetWg = 0;
            }
            if (TotalNetWg > 0)
            {
                var check = _context.ProductionCapture.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.ProductName.Trim() == productName.Trim()).FirstOrDefault();
                if (check != null)
                {
                    var product = _context.ProductMaster.Where(a => a.ProductName.Trim() == productName.Trim()).FirstOrDefault();
                    if (product != null)
                    {
                        if (TotalNetWg > product.Unitqty && TotalNetWg <= (product.Unitqty * 2))
                        {
                            return Json(new { success = true,  unitqty = product.Unitqty });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Enter Correct Wt.,item unit range i,e " + product.Unitqty + " and you have enter " + TotalNetWg , unitqty= product.Unitqty });
                        }

                    }

                }
            }

            return Json(new { success = true });
        }
        public async Task<IActionResult> Index()
        {
            var LIST = await _context.SaveProduction.Where(a => a.Qty > 0).OrderByDescending(a => a.Id).ToListAsync();
            if (LIST.Count > 0)
            {
                foreach (var item in LIST)
                {
                    item.TotalNetWg_Uom = item.TotalNetWg + " " + item.TotalNetWg_Uom;
                }
            }
            return View(LIST);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var saveProduction = await _context.SaveProduction
                .FirstOrDefaultAsync(m => m.Id == id);
            if (saveProduction == null)
            {
                return NotFound();
            }

            return View(saveProduction);
        }
        public IActionResult GetDialShape(string shape)
        {
            var data = _context.DialMaster.Where(a => a.DialShape.Trim() == shape.Trim()).ToList();
            return Json(new { success = true, data = data });
        }
        public IActionResult GetDialCodes(string DialCode)
        {
            var data = _context.DialMaster
                .Where(a => a.DialCode.Trim() == DialCode.Trim())
                .Select(a => new { a.DialCode, a.DialWg, a.DialWgUom, a.DialShape }) // Select only required fields
                .FirstOrDefault();

            return Json(new { success = data != null, data });
        }

        public IActionResult GetProductDetails(string productName, string Production_Id)
        {
            var get = _context.ProductionCapture.Where(a => a.ProductName == productName.Trim() && a.Production_Id.Trim() == Production_Id.Trim()).ToList();
            var data = get.Sum(a => a.TotalQty);

            var saveproduction = _context.SaveProduction.Where(a => a.ProductName == productName.Trim() && a.Production_Id.Trim() == Production_Id.Trim()).ToList();
            var savedata = saveproduction.Sum(a => a.Qty);

            data = Math.Abs(data - savedata);

            List<string> DialCodes = new List<string>();
            var product = _context.ProductMaster.Where(a => a.ProductName.Trim() == productName.Trim()).FirstOrDefault();
            var list = _context.DialMaster.Where(a => a.DialUsedForCakes == product.Unitqty).Select(a => a.DialCode.Trim()).ToList();
            DialCodes.AddRange(list);

            return Json(new { success = true, data = data, DialCodes = DialCodes });
        }

        public IActionResult GetProductData(string productName, string Production_Id, string uom, float weight)
        {
            var data = "";
            var found = _context.ProductMaster.Where(a => a.ProductName.Trim() == productName.Trim()).FirstOrDefault();
            if (found != null)
            {
                if (uom == "KGS")
                {
                    var value = weight * 1000;
                    if (value <= found.Unitqty)
                    {
                        return Json(new { success = false, message = "Weight should be greater than the basic range of the product!" });
                    }
                }
                else
                {
                    var value = weight;
                    if (value <= found.Unitqty)
                    {
                        return Json(new { success = false, message = "Weight should be greater than the basic range of the product!" });
                    }
                }
            }
            return Json(new { success = true, data = "" });
        }
        //public IActionResult GetOutlets(string Production_Id)
        //{
        //    var lstProducts = new List<SelectListItem>();

        //    lstProducts = _context.ProductionCapture.Where(a => a.Status == "Pending" && a.Production_Id.Trim() == Production_Id.Trim()).AsNoTracking().Select(n =>
        //    new SelectListItem
        //    {
        //        Value = n.ProductName,
        //        Text = n.ProductName
        //    }).Distinct().ToList();

        //    var defItem = new SelectListItem()
        //    {
        //        Value = "",
        //        Text = "----Select ProductName ----"
        //    };

        //    lstProducts.Insert(0, defItem);

        //    return Json(new { success = true, data = lstProducts });
        //}

        public IActionResult GetOutlets(string Production_Id)
        {
            var lstProducts = _context.ProductionCapture
                .Where(a => a.Status == "Pending" && a.Production_Id.Trim() == Production_Id.Trim())
                .AsNoTracking()
                .Select(n => new SelectListItem
                {
                    Value = n.ProductName,
                    Text = n.ProductName
                })
                .Distinct()
                .ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select ProductName ----"
            };

            List<DialDetailViewModel> DialDetailViewModellist = new List<DialDetailViewModel>();

            var list = _context.ProductionCapture.Where(a => a.Production_Id.Trim() == Production_Id.Trim() && a.Status == "Pending").ToList();
            if (list.Count > 0)
            {
                var productSummary = _context.SaveProduction
                        .GroupBy(p => new { p.ProductName, p.Production_Id }) // Group by both ProductName & Production_Id
                        .Select(g => new
                        {
                            Product_Name = g.Key.ProductName, // Accessing ProductName
                            Production_Id = g.Key.Production_Id, // Accessing Production_Id
                            Total_Quantity = g.Sum(p => p.Qty),  // Sum of Quantity
                            Total_Count = g.Count()  // Number of records per product
                        })
                        .ToList();

                List<SaveProduction> listtt = new List<SaveProduction>();
                foreach (var item in productSummary)
                {
                    SaveProduction sc = new SaveProduction()
                    {
                        ProductName = item.Product_Name,
                        Production_Id = item.Production_Id,
                        Qty = item.Total_Quantity,
                    };
                    listtt.Add(sc);
                }

                foreach (var item in list)
                {
                    var data = _context.ProductionCapture
                        .Where(a => a.ProductName.Trim() == item.ProductName.Trim() && a.Production_Id.Trim() == Production_Id.Trim() && a.OutletName.Trim() == item.OutletName.Trim())
                        .Sum(a => a.TotalQty);

                    var found = _context.ProductMaster.Where(a => a.ProductName.Trim() == item.ProductName.Trim()).FirstOrDefault();
                    if (found != null)
                    {
                        var value = 0;
                        var savededata1 = listtt
                            .Where(a => a.ProductName.Trim() == item.ProductName.Trim() && a.Production_Id.Trim() == Production_Id.Trim())
                            .FirstOrDefault();
                        if (savededata1 != null)
                        {
                            value = savededata1.Qty;
                            int minValue = Math.Min(savededata1.Qty, data);
                            savededata1.Qty = Math.Abs(savededata1.Qty - minValue);
                        }
                        int PendingQty = Math.Abs(data - value);
                        double mrp = found.MRP;
                        double Selling = found.Selling;
                        double MRP_Rs = found.MRP_Rs;
                        double Selling_Rs = found.Selling_Rs;
                        DialDetailViewModel DialDetailViewModel = new DialDetailViewModel()
                        {
                            ProductName = item.ProductName,
                            TotalQty = item.TotalQty,
                            OutletName = item.OutletName,
                            MRP = mrp,
                            Selling = Selling,
                            MRP_Rs = MRP_Rs,
                            Selling_Rs = Selling_Rs,
                            PendingQty = PendingQty,
                            BasicUnit = (found.Unitqty).ToString() + " " + found.Uom,
                        };
                        DialDetailViewModellist.Add(DialDetailViewModel);
                        //if (savededata1 != null)
                        //{
                        //    savededata1.Qty = Math.Abs(savededata1.Qty - data);
                        //}

                    }
                }
            }


            // Filter products where the difference is NOT zero
            lstProducts = lstProducts
                .Where(item =>
                {
                    var value = item.Value.Trim();
                    var data = _context.ProductionCapture
                        .Where(a => a.ProductName == value && a.Production_Id.Trim() == Production_Id.Trim())
                        .Sum(a => a.TotalQty);

                    var savedata = _context.SaveProduction
                        .Where(a => a.ProductName == value && a.Production_Id.Trim() == Production_Id.Trim())
                        .Sum(a => a.Qty);

                    return Math.Abs(data - savedata) != 0; // Keep items where difference is NOT zero
                })
                .ToList();

            lstProducts.Insert(0, defItem);

            return Json(new { success = true, data = lstProducts, TableData = DialDetailViewModellist });
        }


        public IActionResult CalculateTotalNetWeight(string Production_Id, string productName, double TotalNetWg)
        {
            double sellingRs = 0;
            double mrpRs = 0;

            var found = _context.ProductMaster.Where(a => a.ProductName.Trim() == productName.Trim()).FirstOrDefault();
            if (found != null)
            {
                double mrp = found.MRP;
                double Selling = found.Selling;
                double MRP_Rs = found.MRP_Rs;
                double Selling_Rs = found.Selling_Rs;

                sellingRs = TotalNetWg * Selling_Rs;
                mrpRs = TotalNetWg * MRP_Rs;
            }

            return Json(new { success = true, sellingRs = sellingRs, mrpRs = mrpRs });
        }
        private List<SelectListItem> GetProduction_Id()
        {
            var lstProducts = new List<SelectListItem>();
            var currentdate = DateTime.Now.ToString("dd-MM-yyyy");
            lstProducts = _context.ProductionCapture.Where(a => a.Status == "Pending" && a.Production_Date.Trim() == currentdate.Trim()).AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.Production_Id,
                Text = n.Production_Id
            }).Distinct().ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select Production_Id ----"
            };

            lstProducts.Insert(0, defItem);
            return lstProducts;
        }

        private List<SelectListItem> GetProducts()
        {
            var lstProducts = new List<SelectListItem>();

            lstProducts = _context.ProductMaster.AsNoTracking().Select(n =>
            new SelectListItem
            {
                Value = n.ProductName,
                Text = n.ProductName
            }).ToList();

            var defItem = new SelectListItem()
            {
                Value = "",
                Text = "----Select Product Name----"
            };

            lstProducts.Insert(0, defItem);
            return lstProducts;
        }
        public IActionResult Create()
        {
            int completecount = _context.SaveProduction.Count();
            //int total_count = _context.ProductionCapture.Sum(sp => sp.TotalQty);
            int pendingcount = Math.Max(0, _context.ProductionCapture.Sum(pc => pc.TotalQty) - completecount);


            ViewBag.GetProduction_Id = GetProduction_Id();
            ViewBag.GetProducts = GetProducts();
            ViewBag.completecount = completecount;
            ViewBag.pendingcount = pendingcount;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveProduction saveProduction)
        {
            try
            {
                var currentuser1 = HttpContext.User;
                string username = currentuser1.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name).Value;

                if (saveProduction.ProductGrossWg == 0)
                {
                    return Json(new { success = false, message = "Please do enter the gross wt.!" });
                }
                int maxId = _context.SaveProduction.Any() ? _context.SaveProduction.Max(e => e.Id) + 1 : 1;
                saveProduction.SaveProduction_Date = DateTime.Now.ToString("dd-MM-yyyy");
                saveProduction.SaveProduction_Time = DateTime.Now.ToString("HH:mm");
                //saveProduction.User = "admin";
                saveProduction.DialTierWg_Uom = saveProduction.DialTierWg_Uom.ToUpper();
                saveProduction.Qty = 1;
                saveProduction.Exp_Date = DateTime.Now.AddDays(2).ToString("dd-MM-yyyy");
                saveProduction.Id = maxId;
                saveProduction.User = username;
                _context.Add(saveProduction);
                await _context.SaveChangesAsync();

                // GENERATE STICKER OR NOT
                if (saveProduction.generatesticker == "1")
                {
                    var mrp = _context.ProductMaster
                             .Where(a => a.ProductName.Trim() == saveProduction.ProductName.Trim())
                             .FirstOrDefault();

                    List<Sticker> SaveProduction_list = new List<Sticker>();

                    string trimmedProductName = saveProduction.ProductName.Trim();
                    string productname1 = trimmedProductName.Length > 11 ? trimmedProductName.Substring(0, 11) : trimmedProductName;
                    string productname2 = trimmedProductName.Length > 11 ? trimmedProductName.Substring(11, Math.Min(11, trimmedProductName.Length - 11)) : "";
                    //for(int i = 1; i <= 4; i++)
                    //{
                    //    Sticker Sticker = new Sticker()
                    //    {
                    //        productname = saveProduction.ProductName,
                    //        productname1 = productname1,
                    //        productname2 = productname2,
                    //        wg = saveProduction.TotalNetWg.ToString(),  // Fixed parentheses
                    //        wgvalue = saveProduction.TotalNetWg + " " + saveProduction.TotalNetWg_Uom,
                    //        wguom = saveProduction.TotalNetWg_Uom,
                    //        mrp = mrp.MRP.ToString() ?? "NA" , // Check for null before accessing MRP
                    //        productcode = mrp.ProductCode ?? "NA",
                    //    };
                    //    SaveProduction_list.Add(Sticker);
                    //}
                    SaveProduction_list.AddRange(Enumerable.Range(1, 4).Select(i => new Sticker
                    {
                        productname = saveProduction.ProductName,
                        productname1 = productname1,
                        productname2 = productname2,
                        wg = saveProduction.TotalNetWg.ToString(),
                        wgvalue = saveProduction.TotalNetWg + " " + saveProduction.TotalNetWg_Uom,
                        wguom = saveProduction.TotalNetWg_Uom,
                        mrp = mrp.MRP.ToString() ?? "NA",
                        productcode = mrp.ProductCode ?? "NA"
                    }));


                    string printstk1 = null;
                    string printprn1 = "";
                    string printerName1 = _config["AppSettings:loc1_printer"];
                    string prnFilePath = string.Empty;
                    var count = 0;
                    var rowCount = 0;
                    int qnty = Convert.ToInt32(SaveProduction_list.Count);
                    int def = Convert.ToInt32(qnty / 4);
                    int mod = Convert.ToInt32(qnty % 4);
                    int count1 = 0;

                    var DT = saveProduction.SaveProduction_Date;
                    //var DT = DateTime.Now.ToString("dd-MM-yyyy");
                    var TM = saveProduction.SaveProduction_Time;
                    //var TM = DateTime.Now.ToString("hh:mm");

                    if (def > 0)
                    {
                        for (int i = 0; i < def; i++)
                        {
                            string s = SaveProduction_list[count1].productname;
                            string s1 = SaveProduction_list[count1 + 1].productname;
                            string s2 = SaveProduction_list[count1 + 2].productname;
                            string s3 = SaveProduction_list[count1 + 3].productname;
                            //prnFilePath = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Cake20x10-300-DP-4.prn";
                            //var valuee = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Cake20x10-300-DP-4VALUE.prn";
                            prnFilePath = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Cake20x10-300-DP-4-Dt.prn";
                            var valuee = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Cake20x10-300-DP-4-DtVALUE.prn";

                            //take 20 charcters from productname

                            if (System.IO.File.Exists(valuee))
                            {
                                System.IO.File.Delete(valuee);
                            }
                            System.IO.File.Copy(prnFilePath, valuee);
                            string fileContent = System.IO.File.ReadAllText(prnFilePath);
                            fileContent = fileContent
                                .Replace("<PRODUCT_NAME>", SaveProduction_list[count1].productname.Trim())//1
                                .Replace("<PRODUCT_CODE>", SaveProduction_list[count1].productcode.Trim())//1
                                .Replace("<WG>", SaveProduction_list[count1].wg.ToString())
                                .Replace("<WGVALUE>", SaveProduction_list[count1].wgvalue.ToString())
                                .Replace("<MRP>", (saveProduction.mrpRs).ToString())
                                .Replace("<PRODUCT_NAME1>", SaveProduction_list[count1].productname1)
                                .Replace("<PRODUCT_NAME2>", SaveProduction_list[count1].productname2)
                                .Replace("<DT>", DT)
                                .Replace("<TM>", TM)

                                .Replace("<PRODUCT_NAME_1>", SaveProduction_list[count1 + 1].productname)//2
                                .Replace("<PRODUCT_CODE_1>", SaveProduction_list[count1 + 1].productcode)//2
                                .Replace("<WG_1>", SaveProduction_list[count1 + 1].wg.ToString())
                                .Replace("<WGVALUE_1>", SaveProduction_list[count1 + 1].wgvalue.ToString())
                                .Replace("<MRP_1>", (saveProduction.mrpRs).ToString())
                                .Replace("<PRODUCT_NAME1_1>", SaveProduction_list[count1 + 1].productname1)
                                .Replace("<PRODUCT_NAME2_1>", SaveProduction_list[count1 + 1].productname2)
                                .Replace("<DT>", DT)
                                .Replace("<TM>", TM)

                                .Replace("<PRODUCT_NAME_2>", SaveProduction_list[count1 + 2].productname)//3
                                .Replace("<PRODUCT_CODE_2>", SaveProduction_list[count1 + 2].productcode)//3
                                .Replace("<WG_2>", SaveProduction_list[count1 + 2].wg.ToString())
                                .Replace("<WGVALUE_2>", SaveProduction_list[count1 + 2].wgvalue.ToString())
                                .Replace("<MRP_2>", (saveProduction.mrpRs).ToString())
                                .Replace("<PRODUCT_NAME1_2>", SaveProduction_list[count1 + 2].productname1)
                                .Replace("<PRODUCT_NAME2_2>", SaveProduction_list[count1 + 2].productname2)
                                .Replace("<DT>", DT)
                                .Replace("<TM>", TM)

                                .Replace("<PRODUCT_NAME_3>", SaveProduction_list[count1 + 3].productname)//4
                                .Replace("<PRODUCT_CODE_3>", SaveProduction_list[count1 + 3].productcode)//4
                                .Replace("<WG_3>", SaveProduction_list[count1 + 3].wg.ToString())
                                .Replace("<WGVALUE_3>", SaveProduction_list[count1 + 3].wgvalue.ToString())
                                .Replace("<MRP_3>", (saveProduction.mrpRs).ToString())
                                .Replace("<PRODUCT_NAME1_3>", SaveProduction_list[count1 + 3].productname1)
                                .Replace("<PRODUCT_NAME2_3>", SaveProduction_list[count1 + 3].productname2)
                                .Replace("<DT>", DT)
                                .Replace("<TM>", TM);

                            System.IO.File.WriteAllText(valuee, fileContent);
                            string fileContent1 = System.IO.File.ReadAllText(valuee);
                            count1 = count1 + 4;

                            try
                            {
                                var printerIp1 = IPAddress.Parse(printerName1);
                                var printerPort1 = 9100;
                                var client1 = new TcpClient();
                                client1.Connect(printerIp1, printerPort1);

                                byte[] byteArray1 = Encoding.ASCII.GetBytes(fileContent1);
                                var stream1 = client1.GetStream();
                                stream1.Write(byteArray1, 0, byteArray1.Length);
                                stream1.Flush();

                                client1.Close();
                                Thread.Sleep(300);
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }


                }
                //ENDED

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

            var saveProduction = await _context.SaveProduction.FindAsync(id);
            if (saveProduction == null)
            {
                return NotFound();
            }
            return View(saveProduction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SaveProduction saveProduction)
        {
            if (id != saveProduction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(saveProduction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SaveProductionExists(saveProduction.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(saveProduction);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var saveProduction = await _context.SaveProduction
                .FirstOrDefaultAsync(m => m.Id == id);
            if (saveProduction == null)
            {
                return NotFound();
            }

            return View(saveProduction);
        }

        private bool SaveProductionExists(int id)
        {
            return _context.SaveProduction.Any(e => e.Id == id);
        }

        //Product Sticker Reprint
        public async Task<IActionResult> ReprintStk(string selectedDate = null)
        {
            DateTime today = DateTime.Now.Date;
            DateTime filterDate = string.IsNullOrEmpty(selectedDate) ? today : DateTime.ParseExact(selectedDate, "yyyy-MM-dd", null);

            var productions = await _context.SaveProduction
                .Where(p => p.SaveProduction_Date == filterDate.ToString("dd-MM-yyyy"))
                .ToListAsync();

            return View(productions);
        }


        [HttpPost]
        public async Task<IActionResult> ReprintStickersBulk([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "No stickers selected for reprinting!" });
            }

            var productions = await _context.SaveProduction
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            foreach (var production in productions)
            {
                await GenerateStickers(production);
            }

            return Json(new { success = true, message = "Selected stickers reprinted successfully!" });
        }

        private async Task GenerateStickers(SaveProduction saveProduction)
        {
            var mrp = _context.ProductMaster
                .Where(a => a.ProductName.Trim() == saveProduction.ProductName.Trim())
                .FirstOrDefault();

            List<Sticker> stickerList = new List<Sticker>();

            string trimmedProductName = saveProduction.ProductName.Trim();
            string productname1 = trimmedProductName.Length > 11 ? trimmedProductName.Substring(0, 11) : trimmedProductName;
            string productname2 = trimmedProductName.Length > 11 ? trimmedProductName.Substring(11, Math.Min(11, trimmedProductName.Length - 11)) : "";

            stickerList.AddRange(Enumerable.Range(1, 4).Select(i => new Sticker
            {
                productname = saveProduction.ProductName,
                productname1 = productname1,
                productname2 = productname2,
                wg = saveProduction.TotalNetWg.ToString(),
                wgvalue = saveProduction.TotalNetWg + " " + saveProduction.TotalNetWg_Uom,
                wguom = saveProduction.TotalNetWg_Uom,
                mrp = mrp?.MRP.ToString() ?? "NA",
                productcode = mrp?.ProductCode ?? "NA"
            }));

            // ✅ GENERATE STICKER LOGIC

            List<Sticker> SaveProduction_list = new List<Sticker>();

            SaveProduction_list.AddRange(Enumerable.Range(1, 4).Select(i => new Sticker
            {
                productname = saveProduction.ProductName,
                productname1 = productname1,
                productname2 = productname2,
                wg = saveProduction.TotalNetWg.ToString(),
                wgvalue = saveProduction.TotalNetWg + " " + saveProduction.TotalNetWg_Uom,
                wguom = saveProduction.TotalNetWg_Uom,
                mrp = mrp?.MRP.ToString() ?? "NA",
                productcode = mrp?.ProductCode ?? "NA"
            }));

            string printerName1 = _config["AppSettings:loc1_printer"];
            string prnFilePath = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Cake20x10-300-DP-4.prn";
            string valueFilePath = $"{_webHostEnvironment.WebRootPath}\\Sticker\\Cake20x10-300-DP-4VALUE.prn";

            if (System.IO.File.Exists(valueFilePath))
            {
                System.IO.File.Delete(valueFilePath);
            }
            System.IO.File.Copy(prnFilePath, valueFilePath);

            int qnty = SaveProduction_list.Count;
            int def = qnty / 4;  // Number of complete sets of 4
            int count1 = 0;

            for (int i = 0; i < def; i++)
            {
                string fileContent = System.IO.File.ReadAllText(prnFilePath);

                // ✅ REPLACING STICKER VARIABLES IN FILE CONTENT
                fileContent = fileContent
                    .Replace("<PRODUCT_NAME>", SaveProduction_list[count1].productname.Trim())//1
                    .Replace("<PRODUCT_CODE>", SaveProduction_list[count1].productcode.Trim())//1
                    .Replace("<WG>", SaveProduction_list[count1].wg.ToString())
                    .Replace("<WGVALUE>", SaveProduction_list[count1].wgvalue.ToString())
                    .Replace("<MRP>", saveProduction.mrpRs.ToString())
                    .Replace("<PRODUCT_NAME1>", SaveProduction_list[count1].productname1)
                    .Replace("<PRODUCT_NAME2>", SaveProduction_list[count1].productname2)

                    .Replace("<PRODUCT_NAME_1>", SaveProduction_list[count1 + 1].productname)//2
                    .Replace("<PRODUCT_CODE_1>", SaveProduction_list[count1 + 1].productcode)//2
                    .Replace("<WG_1>", SaveProduction_list[count1 + 1].wg.ToString())
                    .Replace("<WGVALUE_1>", SaveProduction_list[count1 + 1].wgvalue.ToString())
                    .Replace("<MRP_1>", saveProduction.mrpRs.ToString())
                    .Replace("<PRODUCT_NAME1_1>", SaveProduction_list[count1 + 1].productname1)
                    .Replace("<PRODUCT_NAME2_1>", SaveProduction_list[count1 + 1].productname2)

                    .Replace("<PRODUCT_NAME_2>", SaveProduction_list[count1 + 2].productname)//3
                    .Replace("<PRODUCT_CODE_2>", SaveProduction_list[count1 + 2].productcode)//3
                    .Replace("<WG_2>", SaveProduction_list[count1 + 2].wg.ToString())
                    .Replace("<WGVALUE_2>", SaveProduction_list[count1 + 2].wgvalue.ToString())
                    .Replace("<MRP_2>", saveProduction.mrpRs.ToString())
                    .Replace("<PRODUCT_NAME1_2>", SaveProduction_list[count1 + 2].productname1)
                    .Replace("<PRODUCT_NAME2_2>", SaveProduction_list[count1 + 2].productname2)

                    .Replace("<PRODUCT_NAME_3>", SaveProduction_list[count1 + 3].productname)//4
                    .Replace("<PRODUCT_CODE_3>", SaveProduction_list[count1 + 3].productcode)//4
                    .Replace("<WG_3>", SaveProduction_list[count1 + 3].wg.ToString())
                    .Replace("<WGVALUE_3>", SaveProduction_list[count1 + 3].wgvalue.ToString())
                    .Replace("<MRP_3>", saveProduction.mrpRs.ToString())
                    .Replace("<PRODUCT_NAME1_3>", SaveProduction_list[count1 + 3].productname1)
                    .Replace("<PRODUCT_NAME2_3>", SaveProduction_list[count1 + 3].productname2);

                // ✅ Writing modified content back to file
                System.IO.File.WriteAllText(valueFilePath, fileContent);

                try
                {
                    //Uncomment and configure printer logic if needed
                    var printerIp = IPAddress.Parse(printerName1);
                    var printerPort = 9100;
                    var client = new TcpClient();
                    client.Connect(printerIp, printerPort);

                    byte[] byteArray = Encoding.ASCII.GetBytes(fileContent);
                    var stream = client.GetStream();
                    stream.Write(byteArray, 0, byteArray.Length);
                    stream.Flush();
                    client.Close();
                    Thread.Sleep(300);
                }
                catch (Exception ex)
                {
                    // Log error if needed
                }

                count1 += 4; // Move to the next set of 4
            }


            await Task.CompletedTask;
        }


    }


}

