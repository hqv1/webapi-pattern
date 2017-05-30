namespace WebApiPattern.Data.Sqlite.Models
{
    /// <summary>
    /// Seller entity
    /// </summary>
    public class SellerModel
    {
        public long ID { get; set; }
        public long ProductID { get; set; }
        public string Name { get; set; }  
        public double Quantity { get; set; }
        public decimal Price { get; set; }
        public string Url { get; set; }
    }
}