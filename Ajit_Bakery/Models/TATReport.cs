namespace Ajit_Bakery.Models
{
    public class TATReport
    {
        public int id { get; set; }  
        public string ProductionId { get; set; }  
        public string outlet { get; set; }  
        public string productname { get; set; }  
        public string totalnetwg { get; set; }  
        public string totalnetwg_uom { get; set; }  
        public string order_date { get; set; }  
        public string production_date { get; set; }  
        public string packaging_date { get; set; }  
        public string transfer_date { get; set; }  
        public string dispatch_date { get; set; }  
    }
}
