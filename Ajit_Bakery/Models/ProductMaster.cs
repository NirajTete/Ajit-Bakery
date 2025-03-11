using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class ProductMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string ProductName { get; set; }//Black Forest
        public int Qty { get; set; }//1
        public int Unitqty { get; set; }//1000
        public string Uom { get; set; }//kg
        public string Category { get; set; } = "NA";//cake
        public string Type { get; set; } = "NA";
        public string PerGmRate { get; set; } = "NA";
        public double MRP { get; set; } = 0;
        public string? DialCode1 { get; set; } = "NA";
        public string? DialCode2 { get; set; } = "NA";
        //ADDED
        public string? CreateDate { get; set; }
        public string? Createtime { get; set; }
        public string? ModifiedDate { get; set; }
        public string? Modifiedtime { get; set; }
        public string? User { get; set; }
    }
}
