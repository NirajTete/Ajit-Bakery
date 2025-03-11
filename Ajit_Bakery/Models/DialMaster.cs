using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class DialMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        //[Column(TypeName = "varchar()")]
        public string DialCode { get; set; }
        //[Column(TypeName = "varchar()")]
        public string DialShape { get; set; }
        //[Column(TypeName = "varchar()")]
        public double DialWg { get; set; }
        // [Column(TypeName = "varchar()")]
        public string DialWgUom { get; set; }
        //[Column(TypeName = "varchar()")]
        public double DialDiameter { get; set; }
        //[Column(TypeName = "varchar()")]
        public double DialLength { get; set; }
        //[Column(TypeName = "varchar()")]
        public double DialBreadth { get; set; }
        //[Column(TypeName = "varchar()")]
        public string DialUom { get; set; }
        //[Column(TypeName = "varchar()")]
        public string? DialArea { get; set; }

        //ADDED
        public string? CreateDate { get; set; }
        public string? Createtime { get; set; }
        public string? ModifiedDate { get; set; }
        public string? Modifiedtime { get; set; }
        public string? User { get; set; }
    }
}
