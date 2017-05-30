namespace WebApiPattern.Asp.Shared.Models
{
    public class ProductForGetModel
    {
        public long Id { get; set; }
        public string Vendor { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Asin { get; set; }
        public string Upc { get; set; }
        public string ProductType { get; set; }
    }
}
