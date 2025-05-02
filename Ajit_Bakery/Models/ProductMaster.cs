using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class ProductMaster
    {
        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Sr.No.")]
        public int Id { get; set; }
        [Display(Name = "Product Code")]
        public string ProductCode { get; set; }//Black Forest
        [Display(Name = "Production Name")]
        public string ProductName { get; set; }//Black Forest
        [Display(Name = "Qty")]
        public int Qty { get; set; }//1
        [Display(Name = "Unit Qty in Gms")]
        public int Unitqty { get; set; }//1000
        [NotMapped]
        public string Unitqtyuom { get; set; }
        [Display(Name = "Uom")]
        public string Uom { get; set; }//kg
        [Display(Name = "Category")]
        public string Category { get; set; } = "NA";//cake
        [Display(Name = "Type")]
        public string Type { get; set; } = "NA";
        //[Display(Name = "Per Gm Rate")]
        //public string PerGmRate { get; set; } = "NA";
        [Display(Name = "MRP")]
        public double MRP { get; set; } = 0;
        //public string? DialCode1 { get; set; } = "NA";
        //public string? DialCode2 { get; set; } = "NA";
        //ADDED
        public string? CreateDate { get; set; }
        public string? Createtime { get; set; }
        public string? ModifiedDate { get; set; }
        public string? Modifiedtime { get; set; }
        public string? User { get; set; }

        [Display(Name = "Dial Codes")]
        public string? Dial { get; set; }
        [Display(Name = "Sale Rt.")]
        public double Selling { get; set; }
        [Display(Name = "Sale Rt. (Rs/Gm)")]
        public double Selling_Rs { get; set; }
        [Display(Name = "MRP (Rs/Gm)")]
        public double MRP_Rs { get; set; }

        [Display(Name ="Product Height")]
        public double? Height { get; set; }

        [Display(Name ="Height UOM")]
        public string? Height_Uom { get; set; }

        
    }
}
