using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class OutletMaster
    {
        [Key]
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
        public DateTime? CreateDate { get; set; }
        public DateTime? Createtime { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? Modifiedtime { get; set; }
        public string? User { get; set; }
    }
}
