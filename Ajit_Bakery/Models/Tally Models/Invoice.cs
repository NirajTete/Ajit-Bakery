using DocumentFormat.OpenXml.Spreadsheet;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ajit_Bakery.Models.Tally_Models
{
    public class Invoice
    {
        public string partyname { get; set; }
        public string contactno { get; set; }
        public string address { get; set; }
        public string refno { get; set; }
        public string gstno { get; set; }
        public string country { get; set; }
        public string state { get; set; }
        public string dcno { get; set; }
        public string invoiceno { get; set; }
        public string truckno { get; set; }
        public float FinalAmount { get; set; }
        public float totalamount { get; set; }
        public float FreighAmount { get; set; }
        public float BaseAmount { get; set; }
        public string dispatchby { get; set; }
        public float fright { get; set; }
        public float cgst { get; set; }
        public float sgst { get; set; }
        public float igst { get; set; }
        public string EntryType { get; set; }
        public string destination { get; set; }
        public string paymentterm { get; set; }
        public string termofdilivery { get; set; }
        public string remark { get; set; }
        public string gst_type { get; set; }
        public string Freight_type { get; set; }
        public string pincode { get; set; }
        public string VoucherType { get; set; }
        public string agent { get; set; }
        [NotMapped]
        public string invDate { get; set; }
        [NotMapped]
        public string dcDate { get; set; }
        [NotMapped]
        public string orderDate { get; set; }
        public List<InvoiceItemDetails> InvoiceItemDetails { get; set; }  // List of items

    }
    public class InvoiceItemDetails 
    {
        public string productname { get; set; }
        public float rate { get; set; }
        public float amount { get; set; }
        public string cgst { get; set; }
        public string sgst { get; set; }
        public string igst { get; set; }
        public string hsn { get; set; }
        public string uom { get; set; }
        public int qty { get; set; }
        [NotMapped]
        public double UNIT { get; set; }
     
    }
}
