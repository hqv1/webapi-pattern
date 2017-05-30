namespace WebApiPattern.Asp.Shared.Models
{
    public abstract class ProductForManipulationModel
    {
        public string Vendor { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Asin { get; set; }
        public string Upc { get; set; }
        public string ProductType { get; set; }       
    }
}