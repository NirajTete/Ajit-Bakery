using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class BoxMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
        public string? CreateDate { get; set; }
        public string? Createtime { get; set; }
        public string? ModifiedDate { get; set; }
        public string? Modifiedtime { get; set; }
        [NotMapped]
        public string? area { get; set;}
        public string? User { get; set;}
    }
}
