using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class OutletMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        //[Column(TypeName = "varchar()")]
        public string OutletCode { get; set; }
        //[Column(TypeName = "varchar()")]
        public string OutletName { get; set; }
        //[Column(TypeName = "varchar()")]
        public string OutletAddress { get; set; }
        //[Column(TypeName = "varchar()")]
        public string OutletContactNo { get; set; }
        //[Column(TypeName = "varchar()")]
        public string OutletContactPerson { get; set; }
        //ADDED
        public string? CreateDate { get; set; }
        public string? Createtime { get; set; }
        public string? ModifiedDate { get; set; }
        public string? Modifiedtime { get; set; }
        public string? User { get; set; }
    }
}
