using DocumentFormat.OpenXml.Spreadsheet;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models.Tally_Models
{
    public class POorderVoucher
    {
        public string EntryType { get; set; }
        public string destination { get; set; }
        public string dispatchby { get; set; }
        public string otherref { get; set; }
        public string pono { get; set; }
        public string suppliername { get; set; }
        public string gstinno { get; set; }
        public string contactno { get; set; }
        public string address { get; set; }
        public string podate { get; set; }
        public string status { get; set; }
        public string state { get; set; }
        public string paymentterm { get; set; }
        public string termofdilivery { get; set; }
        public string remark { get; set; }
        public float fright { get; set; }
        public float totalamount { get; set; }
        public float FreighAmount { get; set; }
        public float BaseAmount { get; set; }
        public float SGST { get; set; }
        public float CGST { get; set; }
        public float IGST { get; set; }
        public float FinalAmount { get; set; }
        public string gst_type { get; set; }
        public string gstno { get; set; }
        public string country { get; set; }
        public string pincode { get; set; }
        public string VoucherType { get; set; }
        public string Freight_type { get; set; }
        public List<POorderItemDetails> POorderItemDetails { get; set; }  // List of items

    }
    public class POorderItemDetails
    {
        public string productcode { get; set; }
        public string description { get; set; }
        public int quantity { get; set; }
        public string uom { get; set; }
        public string warranty { get; set; }
        public float amount { get; set; } 
        public float rate { get; set; }
        public string cgst { get; set; }
        public string sgst { get; set; }
        public string igst { get; set; }
        public string HSN { get; set; }
    }
}
