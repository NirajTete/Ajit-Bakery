using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class UserType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "User Id")]
        public int user_id { get; set; }

        [Required]
        [Display(Name = "User Type")]
        public string usertype_name { get; set; }

        [Display(Name = "Designation")]
        public string designation { get; set; }
    }
}
