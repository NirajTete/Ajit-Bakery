using System.ComponentModel.DataAnnotations;

namespace Ajit_Bakery.Models
{
    public class ReceiptIds
    {
        [Key]
        public int id { get; set; }
        public string ProductionId { get; set; }
        public string date { get; set; } = DateTime.Now.ToString("dd-mm-YYYY HH:mm:ss");
    }
}
