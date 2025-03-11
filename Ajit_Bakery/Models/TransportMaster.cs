using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class TransportMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        //[Column(TypeName = "varchar()")]
        public string DriverName { get; set; }
        //[Column(TypeName = "varchar()")]
        public string DriverContactNo { get; set; }
        //[Column(TypeName = "varchar()")]
        public string VehicleNo { get; set; }
        //[Column(TypeName = "varchar()")]
        public string VehicleOwn { get; set; }
        //[Column(TypeName = "varchar()")]
        public string VehicleType { get; set; }
        //[Column(TypeName = "varchar()")]
        public string? VehicleNoOfTyre { get; set; }
        //[Column(TypeName = "varchar()")]
        public string? VehicleCapacity { get; set; }
        //[Column(TypeName = "varchar()")]
        public string? VehicleVolume { get; set; }
        //ADDED
        public string? CreateDate { get; set; }
        public string? Createtime { get; set; }
        public string? ModifiedDate { get; set; }
        public string? Modifiedtime { get; set; }
        public string? User { get; set; }
    }
}
