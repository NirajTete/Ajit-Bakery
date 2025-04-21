using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class UserMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Sr.No.")]
        public int Id { get; set; }
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Password")]
        public string UserPassward { get; set; }
        [Display(Name = "Code")]
        public string UserCode { get; set; }
        [Display(Name = "Department")]
        public string UserDept { get; set; }
        [Display(Name = "Contact No.")]
        public string UserContactNo { get; set; }
        [Display(Name = "Role")]
        public string UserRole { get; set; }
        public bool KeepLoogedIn { get; set; } = false;
        //ADDED
        public string? CreateDate { get; set; }
        public string? Createtime { get; set; }
        public string? ModifiedDate { get; set; }
        public string? Modifiedtime { get; set; }

    }
}
