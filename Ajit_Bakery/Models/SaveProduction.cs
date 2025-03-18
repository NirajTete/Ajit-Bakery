using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class SaveProduction
    {
        public int Id { get; set; }
        [Display(Name = "Production Product")]
        public string ProductName { get; set; }
        [Display(Name = "Product Gross.Wg.")]
        public double ProductGrossWg { get; set; }
        [Display(Name = "Product Gross.Wg.")]
        public string ProductGrossWg_Uom { get; set; }
        [NotMapped]
        [Display(Name = "Gross Wg.")]
        public string ProductGrossWgValue {get; set; }
        [Display(Name = "Dial Shape")]
        [NotMapped]
        public string DialShape { get; set; }
        [Display(Name = "Dial Tier Wg.")]
        public double DialTierWg { get; set; }
        [Display(Name = "Dial Tier Uom")]
        public string DialTierWg_Uom { get; set; }
        [NotMapped]
        [Display(Name = "Tier Wg.")]
        public string DialTierWgValue { get; set; }
        [Display(Name = "Dial Code")]
        public string DialCode { get; set; }
        [Display(Name = "Total Net.Wg.")]
        public double TotalNetWg { get; set; }
        [Display(Name = "Total NetWg.Uom")]
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
    }
}
