// ReSharper disable InconsistentNaming

using System.Collections.Generic;

namespace WebApiPattern.Logic.Entities
{
    /// <summary>
    /// Product entity
    /// </summary>
    public class ProductEntity
    {
        public long Id { get; set; }
        public string Vendor { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Asin { get; set; }
        public string Upc { get; set; }
        public string ProductType { get; set; }

        public IList<SellerEntity> Sellers { get; set; } = new List<SellerEntity>();
    }
}
