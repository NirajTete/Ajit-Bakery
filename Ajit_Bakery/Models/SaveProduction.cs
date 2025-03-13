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
        [Display(Name = "Dial Shape")]
        [NotMapped]
        public string DialShape { get; set; }
        [Display(Name = "Dial Tier Wg.")]
        public double DialTierWg { get; set; }
        [Display(Name = "Dial Tier Uom")]
        public string DialTierWg_Uom { get; set; }
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
    }
}
