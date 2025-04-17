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
using Ajit_Bakery.Models.Tally_Models;
using Ajit_Bakery.Services;
using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Notyf;

namespace Ajit_Bakery.Controllers
{
    [Authorize]
    public class OutletMastersController : Controller
    {
        public INotyfService _notyfyService { get; }
        private readonly DataDBContext _context;
        private readonly IConfiguration _config;
        private readonly IApiService _apiService;
        public OutletMastersController(DataDBContext context, IConfiguration config, IApiService apiService,INotyfService notyfyService)
        {
            _context = context;
            _config = config;
            _apiService = apiService;
            _notyfyService = notyfyService;
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

            List<string> existingProductlist = new List<string>();
            var list = _context.OutletMaster.ToList();
            var baseurl = _config["AppSettings:BaseUrl"];
            var url2 = $"{baseurl}/AllLedger";
            //var result = await _apiService.GetAsync<ApiResponse<List<Ledger>>>(url2, null);
            var url1 = $"{baseurl}/AllLedger/GetCustomer";
            var existingProduct = await _apiService.GetAsync<ApiResponse<List<string>>>(url1, null);
            existingProductlist.AddRange(existingProduct.Data);
            int counter = 0;

            list = list.Where(product => !existingProductlist.Contains(product.OutletName?.Trim().ToUpper())).ToList();

            if (list.Count == 0)
            {
                return Json(new { success = false, message = "Already Synced to Tally Server!" });
            }
            foreach (var supplier_Master in list)
            {
                //AllLedger
                try
                {
                    if (supplier_Master.OutletContactNo == "NA")
                    {
                        supplier_Master.OutletContactNo = "0000000000";
                    }
                    Ledger ledger = new Ledger()
                    {
                        name1 = supplier_Master.OutletName ?? "",
                        name2 = supplier_Master.OutletCode ?? "",
                        type = "Sundry Debtors" ?? "",
                        GUID = "",
                        phoneno = supplier_Master.OutletContactNo ?? "0000000000",
                        address = supplier_Master.OutletAddress ?? "",
                        city = "",
                        state =/* supplier_Master.state ?? */ "",
                        zipcode = /*supplier_Master.pincode ??*/ "",
                        country = /*supplier_Master.Country ??*/ "",
                        gst =  "",
                        creditlimit = /*(supplier_Master.Creaditlimit).ToString() ??*/ "",
                        contactpersonno = supplier_Master.OutletContactPerson ?? "",
                        contactpersonemail = "",
                        contactpersonname = supplier_Master.OutletContactPerson ?? "",
                    };

                    var uom_val = supplier_Master.OutletName.Trim();

                    var data = await _apiService.PostAsync<ApiResponse<string>>(url2, ledger);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                //END
            }
            return Json(new { success = true, message = "Successfully Done !" });
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.OutletMaster.OrderByDescending(a=>a.Id).ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var outletMaster = await _context.OutletMaster
                .FirstOrDefaultAsync(m => m.Id == id);
            if (outletMaster == null)
            {
                return NotFound();
            }
            outletMaster.CreateDate = outletMaster.CreateDate + " " + outletMaster.Createtime;
            outletMaster.ModifiedDate = outletMaster.ModifiedDate + " " + outletMaster.Modifiedtime;
            return View(outletMaster);
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
            var code = "OUTLET";
            var getlast = _context.OutletMaster
                            .Where(a => a.OutletCode.StartsWith(code))
                            .OrderByDescending(a => a.OutletCode)
                            .Select(a => a.OutletCode)
                            .FirstOrDefault();

            int nextNumber = 1; // Default value if no records exist

            if (getlast != null && getlast.StartsWith(code))
            {
                var find = getlast.Substring(code.Length); // Extract numeric part

                if (int.TryParse(find, out int lastNumber))
                {
                    nextNumber = lastNumber + 1; // Increment the number
                }
            }

            var newOutletCode = code + nextNumber; // Generate the new outlet code

            ViewBag.outletcode = newOutletCode;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OutletMaster outletMaster)
        {
            try
            {
                if (outletMaster.OutletName != null)
                {
                    var exist = _context.OutletMaster.Where(a => a.OutletName.Trim() == outletMaster.OutletName.Trim()).FirstOrDefault();
                    if (exist != null)
                    {
                        return Json(new { success = false, message = "Already Exist ! " });
                    }
                }
                var currentuser1 = HttpContext.User;
                string username = currentuser1.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name).Value;

                int maxId = _context.OutletMaster.Any() ? _context.OutletMaster.Max(e => e.Id) + 1 : 1;
                outletMaster.Id = maxId;
                outletMaster.CreateDate = DateTime.Now.ToString("dd-MM-yyyy");
                outletMaster.ModifiedDate = DateTime.Now.ToString("dd-MM-yyyy");
                outletMaster.Createtime = DateTime.Now.ToString("HH:mm");
                outletMaster.Modifiedtime = DateTime.Now.ToString("HH:mm");
                outletMaster.User = username.ToString();
                _context.Add(outletMaster);
                await _context.SaveChangesAsync();

                //TALLY ADDED
                try
                {
                    Ledger ledger = new Ledger()
                    {
                        name1 = outletMaster.OutletName ?? "NA",
                        name2 = outletMaster.OutletCode ?? "NA",
                        type = "Sundry Debtors" ?? "NA",
                        GUID = "",
                        phoneno = outletMaster.OutletContactNo ?? "NA",
                        address = outletMaster.OutletAddress ?? "NA",
                        city =  "",
                        state =  "",
                        zipcode =  "",
                        country =  "",
                        gst =  "",
                        creditlimit =  "",
                        contactpersonno = outletMaster.OutletContactNo ?? "",
                        contactpersonemail = "",
                        contactpersonname = outletMaster.OutletContactPerson ?? "",
                    };

                    var uom_val = outletMaster.OutletName.Trim();
                    var baseurl = _config["AppSettings:BaseUrl"];
                    //var url = $"{baseurl}/UOM";
                    var url = $"{baseurl}/AllLedger";
                    var data = await _apiService.PostAsync<ApiResponse<string>>(url, ledger);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                //ENDEED

                
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

            var outletMaster = await _context.OutletMaster.FindAsync(id);
            if (outletMaster == null)
            {
                return NotFound();
            }
            return View(outletMaster);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,OutletMaster outletMaster)
        {
            try
            {
                outletMaster.ModifiedDate = DateTime.Now.ToString("dd-MM-yyyy");
                outletMaster.Modifiedtime = DateTime.Now.ToString("HH:mm");
                outletMaster.User = "admin";
                _context.Update(outletMaster);
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

                var productMaster = await _context.OutletMaster
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (productMaster == null)
                {
                    return Json(new { success = false, message = "Data not found in master ! " });
                }
                else
                {
                    _context.OutletMaster.Remove(productMaster);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Deleted Successfully !" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "WWarning : " + ex.Message });
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var outletMaster = await _context.OutletMaster.FindAsync(id);
            if (outletMaster != null)
            {
                _context.OutletMaster.Remove(outletMaster);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OutletMasterExists(int id)
        {
            return _context.OutletMaster.Any(e => e.Id == id);
        }
    }
}
