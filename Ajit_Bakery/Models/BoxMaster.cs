using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class BoxMaster
    {
        [Key]
        public int Id { get; set; }
        //[Column(TypeName = "varchar()")]
        public string BoxNumber { get; set; }
        //[Column(TypeName = "varchar()")]
        public double BoxLength { get; set; }
        //[Column(TypeName = "varchar()")]
        public double BoxBreadth { get; set; }
        //[Column(TypeName = "varchar()")]
        public double BoxHeight { get; set; }
        //[Column(TypeName = "varchar()")]
        public string BoxUom { get; set; }
        //[Column(TypeName = "varchar()")]
        public string? BoxArea { get; set; }
        //ADDED
        public DateTime? CreateDate { get; set; }
        public DateTime? Createtime { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? Modifiedtime { get; set; }
        public string? User { get; set; }
    }
}
