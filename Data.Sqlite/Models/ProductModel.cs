namespace WebApiPattern.Data.Sqlite.Models
{
    public class ProductModel
    {
        public long ID { get; set; }
        public string Vendor { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Asin { get; set; }
        public string Upc { get; set; }
        public string ProductType { get; set; }
    }
}