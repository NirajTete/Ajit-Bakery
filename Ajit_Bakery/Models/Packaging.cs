using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class Packaging
    {
        [Display(Name = "Sr.No")]
        public int Id { get; set; }
        [Display(Name = "Outlet Name")]
        public string Outlet_Name { get; set; }
        [Display(Name = "Box.No")]
        public string Box_No { get; set; }
        [Display(Name = "Product Name")]
        public string Product_Name { get; set; }
        [Display(Name = "Qty")]
        public int Qty { get; set; }
        [Display(Name = "Total Net.Wg.")]
        public double TotalNetWg { get; set; }
        [Display(Name = "Uom")]
        public string TotalNetWg_Uom { get; set; }
        [Display(Name = "Production.Dt")]
        public string Production_Dt { get; set; }
        public string? Production_Tm { get; set; }
        [Display(Name = "Exp.Dt")]
        public string Exp_Dt { get; set; }
        [Display(Name = "Production Id")]
        public string Production_Id { get; set; }
        [Display(Name = "DispatchReady Flag")]
        public int? DispatchReady_Flag { get; set; } = 0;
        [Display(Name = "Packaging Date.")]
        public string? Packaging_Date { get; set; }
        [Display(Name = "Packaging Time.")]
        public string? Packaging_Time { get; set; }
        [Display(Name = "Reciept Id.")]
        public string? Reciept_Id { get; set; }


        public int? Dispatch_Flag { get; set; } = 0;
        public string? DispatchReady_Date { get; set; }
        public string? DispatchReady_Time { get; set; } 
        public string? DCNo { get; set; }

        [NotMapped]
        public string Category { get; set; }

        [NotMapped]
        public string Status { get; set; }

        public double sellingRs { get; set; }
        //[NotMapped]
        public double mrpRs { get; set; }
        public string? user { get; set; }

    }
}
