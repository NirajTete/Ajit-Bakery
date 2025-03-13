using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class TransportMaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string DriverName { get; set; }
        public string DriverContactNo { get; set; }
        public string VehicleNo { get; set; }
        public string VehicleOwn { get; set; }
        //public string VehicleType { get; set; }
        //public string? VehicleNoOfTyre { get; set; }
        //public string? VehicleCapacity { get; set; }
        //public string? VehicleVolume { get; set; }
        //ADDED
        public string? CreateDate { get; set; }
        public string? Createtime { get; set; }
        public string? ModifiedDate { get; set; }
        public string? Modifiedtime { get; set; }
        public string? User { get; set; }
    }
}
