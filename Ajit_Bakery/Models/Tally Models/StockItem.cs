namespace Ajit_Bakery.Models.Tally_Models
{
    public class StockItem
    {
        public string name { get; set; } //dec

        public string alias { get; set; } //ATC0002

        public string GUID { get; set; } = "NA";

        public string unit { get; set; }

        public string category { get; set; }

        public double openingrate { get; set; }
        public string openingqnty { get; set; }

        public string hsncode { get; set; }
        public string cgst { get; set; }
        public string sgst { get; set; }
        public string igst { get; set; }



    }
}
