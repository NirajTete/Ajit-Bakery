using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class UserMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        //[Column(TypeName = "varchar()")]
        public string UserName { get; set; }
        public string Email { get; set; }
        public string UserPassward { get; set; }
        //[Column(TypeName = "varchar()")]
        public string UserCode { get; set; }
        //[Column(TypeName = "varchar()")]
        public string UserDept { get; set; }
        //[Column(TypeName = "varchar()")]
        //public string UserDesignation { get; set; }
        //[Column(TypeName = "varchar()")]
        public string UserContactNo { get; set; }
        //[Column(TypeName = "varchar()")]
        public string UserRole { get; set; }
        public bool KeepLoogedIn { get; set; } = false;
        //ADDED
        public string? CreateDate { get; set; }
        public string? Createtime { get; set; }
        public string? ModifiedDate { get; set; }
        public string? Modifiedtime { get; set; }

    }
}
