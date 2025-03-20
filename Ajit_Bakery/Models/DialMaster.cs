using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class DialMaster
    {
        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Sr.No")]
        public int Id { get; set; }
        [Display(Name = "Dial Code.")]
        public string DialCode { get; set; }
        [Display(Name = "Dial Shape")]
        public string DialShape { get; set; }
        [Display(Name = "Dial Wg.")]
        public double DialWg { get; set; }
        [Display(Name = "Dial Uom")]
        public string DialWgUom { get; set; }
        [Display(Name = "Diameter")]
        public double DialDiameter { get; set; }
        [Display(Name = "Length")]
        public double DialLength { get; set; }
        [Display(Name = "Breadth")]
        public double DialBreadth { get; set; }
        [Display(Name = "Unit")]
        public string LengthUom { get; set; }
        [Display(Name = "Area")]
        public string? DialArea { get; set; }

        //ADDED
        public string? CreateDate { get; set; }
        public string? Createtime { get; set; }
        public string? ModifiedDate { get; set; }
        public string? Modifiedtime { get; set; }
        public string? User { get; set; }
        [NotMapped]
        public string value { get; set; }
        [NotMapped]
        public string calvalue { get; set; }
        [NotMapped]
        public string areacalvalue { get; set; }
        [Display(Name = "Dial Used For Cakes")]
        public double DialUsedForCakes { get; set; }
        [NotMapped]
        [Display(Name = "Dial Used For Cakes")]
        public double DialUsedForCakes_value { get; set; }
        [Display(Name = "Dial Used For Cakes (Unit)")]
        public string DialUsedForCakes_Uom { get; set; }
    }
}
