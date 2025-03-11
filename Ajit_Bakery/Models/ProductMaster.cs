using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class ProductMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        //[Column(TypeName ="varchar()")]
        public string ProductName { get; set; }
        //[Column(TypeName = "varchar()")]
        public int Qty { get; set; }
        //[Column(TypeName = "varchar()")]
        public int Unitqty { get; set; }
        //[Column(TypeName = "varchar()")]
        public string Uom { get; set; }
        //[Column(TypeName = "varchar()")]
        public string Category { get; set; } = "NA";
        //[Column(TypeName = "varchar()")]
        public string Type { get; set; } = "NA";
        //[Column(TypeName = "varchar()")]
        public string PerGmRate { get; set; } = "NA";
        //[Column(TypeName = "varchar()")]
        public double MRP { get; set; } = 0;
        //[Column(TypeName = "varchar()")]
        public string? DialCode1 { get; set; } = "NA";
        //[Column(TypeName = "varchar()")]
        public string? DialCode2 { get; set; } = "NA";
        //ADDED
        public DateTime? CreateDate { get; set; }
        public DateTime? Createtime { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? Modifiedtime { get; set; }
        public string? User { get; set; }
    }
}
