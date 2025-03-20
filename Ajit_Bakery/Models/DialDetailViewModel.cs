namespace Ajit_Bakery.Models
{
    public class DialDetailViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int TotalQty { get; set; }
        public string OutletName { get; set; }
        public string BasicUnit { get; set; }
        public double MRP { get; set; }
        public double MRP_Rs { get; set; }
        public double Selling { get; set; }
        public double Selling_Rs { get; set; }
        public int PendingQty { get; set; }
        public int DispatchReady { get; set; }

    }
}
