using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class SaveProduction
    {
        public int Id { get; set; }
        [Display(Name = "Production Product")]
        public string ProductName { get; set; }
        [Display(Name = "Product Gross.Wt.")]
        public double ProductGrossWg { get; set; }
        [Display(Name = "Product Gross.Wt.")]
        public string ProductGrossWg_Uom { get; set; }
        [NotMapped]
        [Display(Name = "Gross Wt.")]
        public string ProductGrossWgValue {get; set; }
        [Display(Name = "Dial Shape")]
        [NotMapped]
        public string DialShape { get; set; }
        [Display(Name = "Dial Tare Wt.")]
        public double DialTierWg { get; set; }
        [Display(Name = "Dial Tare Uom")]
        public string DialTierWg_Uom { get; set; }
        [NotMapped]
        [Display(Name = "Tare Wt.")]
        public string DialTierWgValue { get; set; }
        [Display(Name = "Dial Code")]
        public string DialCode { get; set; }
        [Display(Name = "Total Net.Wt.")]
        public double TotalNetWg { get; set; }
        [Display(Name = "Total NetWt.Uom")]
        public string TotalNetWg_Uom { get; set; }
        [Display(Name = "Production Qty")]
        public int Qty { get; set; }
        [Display(Name = "Save Date")]
        public string SaveProduction_Date { get; set; }
        [Display(Name = "Save Time")]
        public string SaveProduction_Time { get; set; }
        public string? User { get; set; }

        [NotMapped]
        public string generatesticker { get; set; }
        [Display(Name = "Save Date")]
        public string Exp_Date { get; set; }
        [Display(Name = "Box No")]
        public string? Box_No { get; set; }
        [Display(Name = "Packaging Dt.")]
        public string? Packaging_Date { get; set; }
        [Display(Name = "Packaging Time.")]
        public string? Packaging_Time { get; set; }
        public int? Packaging_Flag { get; set; } = 0;
        public string Production_Id {  get; set; }
        [NotMapped]
        public string Status { get; set; }
        [NotMapped]
        public List<DialDetailViewModel> DialDetails { get; set; }
        //[NotMapped]
        public string NetWg { get; set; }
        //[NotMapped]
        public string NetWg_uom { get; set; }
        //[NotMapped]
        public double sellingRs { get; set; }
        //[NotMapped]
        public double mrpRs { get; set; }

        public string? outlet { get; set; }
    }
}
