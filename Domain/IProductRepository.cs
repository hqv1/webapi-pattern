using System.Threading.Tasks;
using Hqv.CSharp.Common.Web.Api;
using WebApiPattern.Domain.Entities;

namespace WebApiPattern.Domain
{
    public interface IProductRepository
    {
        Task<bool> DoesProductExist(long id);
        Task<PagedList<ProductEntity>> GetProducts(ResourceParameters resourceParameters, ProductFilterParameters filterParameters);
        Task<ProductEntity> GetProduct(long id);
        Task CreateProduct(ProductEntity product);
        Task UpdateProduct(ProductEntity product);
        Task RemoveProduct(ProductEntity product);
    }
}