using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class OutletMaster
    {
        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Sr.No")]
        public int Id { get; set; }
        [Display(Name = "Outlet Code")]
        public string OutletCode { get; set; }
        [Display(Name = "Outlet Name")]
        public string OutletName { get; set; }
        [Display(Name = "Address")]
        public string OutletAddress { get; set; }
        [Display(Name = "Contact No.")]
        public string OutletContactNo { get; set; }
        [Display(Name = "Contact Person")]
        public string OutletContactPerson { get; set; }
        //ADDED
        public string? CreateDate { get; set; }
        public string? Createtime { get; set; }
        public string? ModifiedDate { get; set; }
        public string? Modifiedtime { get; set; }
        public string? User { get; set; }
    }
}
