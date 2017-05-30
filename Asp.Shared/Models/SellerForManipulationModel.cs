namespace WebApiPattern.Asp.Shared.Models
{
    public abstract class SellerForManipulationModel
    {
        public string Name { get; set; }
        public double Quantity { get; set; }
        public decimal Price { get; set; }
        public string Url { get; set; }
    }
}