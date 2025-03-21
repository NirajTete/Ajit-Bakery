﻿using System.ComponentModel.DataAnnotations;

namespace Ajit_Bakery.Models
{
    public class ProductionCapture
    {
        public int Id {  get; set; }
        [Display(Name = "Production Id")]
        public string Production_Id { get; set; }
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }
        [Display(Name = "Product Uom")]
        public string Unit { get; set; }
        [Display(Name = "Outlet Name")]
        public string OutletName { get; set; }
        [Display(Name = "Total Qty")]
        public int TotalQty { get; set; }
        [Display(Name = "Production Date")]
        public string Production_Date { get; set; }
        [Display(Name = "Production Time")]
        public string Production_Time { get; set; }
        public string? User { get; set; }
        public string? Status { get; set; }

    }
}
