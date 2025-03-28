using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class BoxMaster
    {
        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Sr.No")]
        public int Id { get; set; }
        [Display(Name = "Box.No.")]
        public string BoxNumber { get; set; }
        [Display(Name = "Box Lg.")]
        public double BoxLength { get; set; }
        [Display(Name = "Box Bd.")]
        public double BoxBreadth { get; set; }
        [Display(Name = "Box Ht.")]
        public double BoxHeight { get; set; }
        [Display(Name = "Uom")]
        public string BoxUom { get; set; }
        [Display(Name = "Area")]
        public string? BoxArea { get; set; }
        [Display(Name = "Create Dt")]
        //ADDED
        public string? CreateDate { get; set; }
        [Display(Name = "Create Tm")]
        public string? Createtime { get; set; }
        [Display(Name = "Modified Dt.")]
        public string? ModifiedDate { get; set; }
        [Display(Name = "Modified Tm.")]
        public string? Modifiedtime { get; set; }
        [Display(Name = "Area")]
        [NotMapped]
        public string? area { get; set;}
        [Display(Name = "User")]
        public string? User { get; set;}

        public int Use_Flag { get; set; }
    }
}
