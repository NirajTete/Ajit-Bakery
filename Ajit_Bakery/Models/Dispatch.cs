using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models
{
    public class Dispatch
    {
        public int Id { get; set; }
        public string ProductionId { get; set; }
        public string ProductName { get; set; }
        public string OutletName { get; set; }
        public string ReceiptNo { get; set; }
        public string DCNo { get; set; }
        public string BoxNo { get; set; }
        public int Qty { get; set; }
        public double TotalNetWg { get; set; }
        public string TotalNetWg_Uom { get; set; }

        public string Production_Date { get; set; }
        public string Production_Time { get; set; }
        public string SaveProduction_Date { get; set; }
        public string SaveProduction_Time { get; set; }
        public string Packaging_Date { get; set; }
        public string Packaging_Time { get; set; }
        public string TransferToDispatch_Date { get; set; }
        public string TransferToDispatch_Time { get; set; }
        public string Dispatch_Date { get; set; }
        public string Dispatch_Time { get; set; }

        public string VehicleDriverContactNo { get; set; }
        public string VehicleNumber { get; set; }
        public string VehicleDriverName { get; set; }
        public string VehicleOwn { get; set; }
        public string? Status { get; set; }
        [NotMapped]
        public string category { get; set; }
        public string? user { get; set; }
        [NotMapped]
        public double amount { get; set; }
        [NotMapped]
        public double rate { get; set; }
        [NotMapped]
        public string categary { get; set; }
        [NotMapped]
        public string VoucherType { get; set; }
        public string? INNO { get; set; }
    }
}
