using System.Collections.Generic;
using System.Threading.Tasks;
using WebApiPattern.Logic.Entities;

namespace WebApiPattern.Logic
{
    public interface ISellerRepository
    {
        Task<bool> DoesSellerExist(long id);
        Task<IEnumerable<SellerEntity>> GetSellersForProduct(long productId);
        Task<SellerEntity> GetSellerForProduct(long productId, long id);
        Task CreateSeller(long productId, SellerEntity seller);
        Task UpdateSeller(SellerEntity seller);
        Task RemoveSeller(SellerEntity seller);
    }
}