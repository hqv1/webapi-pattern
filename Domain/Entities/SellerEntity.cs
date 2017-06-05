namespace WebApiPattern.Domain.Entities
{
    /// <summary>
    /// Seller entity
    /// </summary>
    public class SellerEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }  
        public double Quantity { get; set; }
        public decimal Price { get; set; }
        public string Url { get; set; }
    }
}