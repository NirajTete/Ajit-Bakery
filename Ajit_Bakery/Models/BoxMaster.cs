using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class BoxMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string BoxNumber { get; set; }
        public double BoxLength { get; set; }
        public double BoxBreadth { get; set; }
        public double BoxHeight { get; set; }
        public string BoxUom { get; set; }
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
