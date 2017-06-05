using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Hqv.CSharp.Common.Extensions;
using Hqv.CSharp.Common.Interfaces;
using Hqv.CSharp.Common.Web.Api;
using Microsoft.Data.Sqlite;
using WebApiPattern.Data.Sqlite.Models;
using WebApiPattern.Domain;
using WebApiPattern.Domain.Entities;

namespace WebApiPattern.Data.Sqlite
{
    public class ProductRepository : IProductRepository
    {
        private readonly Setting _setting;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;

        public class Setting
        {
            public Setting(string connectionString)
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("DbRepository connection string cannot be null or empty string");
                }
                ConnectionString = connectionString;
            }

            public string ConnectionString { get; }
        }

        public ProductRepository(Setting setting, IMapper mapper, IPropertyMappingService propertyMappingService)
        {
            _setting = setting;
            _mapper = mapper;
            _propertyMappingService = propertyMappingService;
        }       

        public async Task<bool> DoesProductExist(long id)
        {
            using (var connection = new SqliteConnection(_setting.ConnectionString))
            {
                var command = $@"SELECT COUNT(*) FROM Products WHERE ID = {id}";
                var result = await connection.ExecuteScalarAsync<int>(command);
                return result >= 1;
            }
        }

        /// <summary>
        /// Uses EF instead of Dapper so we can use IQueryable and only query the database once. 
        /// </summary>
        /// <param name="resourceParameters"></param>
        /// <param name="filterParameters"></param>
        /// <returns></returns>
        public Task<PagedList<ProductEntity>> GetProducts(ResourceParameters resourceParameters, ProductFilterParameters filterParameters)
        {
            using (var context = new WebApiPatternContext(_setting.ConnectionString))
            {
                // Sort
                var queryable =
                    context.Products.ApplySort(resourceParameters.OrderBy,
                        _propertyMappingService.GetPropertyMapping<ProductEntity, ProductModel>());

                // Filter
                if (!string.IsNullOrEmpty(filterParameters.ProductType))
                {
                    var query = filterParameters.ProductType.Trim().ToLowerInvariant();
                    queryable = queryable.Where(p => p.ProductType != null && p.ProductType.ToLowerInvariant() == query);
                }

                // Search
                if (!string.IsNullOrEmpty(resourceParameters.SearchQuery))
                {
                    var query = resourceParameters.SearchQuery.Trim().ToLowerInvariant();
                    queryable = queryable.Where(p =>
                        p.Name.ToLowerInvariant().Contains(query)
                        || p.Vendor.ToLowerInvariant().Contains(query)
                        || (p.Description != null && p.Description.ToLowerInvariant().Contains(query))
                    );
                }

                // Page
                var count = queryable.Count();
                var models =
                    queryable.Skip((resourceParameters.PageNumber - 1) * resourceParameters.PageSize)
                        .Take(resourceParameters.PageSize)
                        .ToList();

                var entities = _mapper.Map<IEnumerable<ProductEntity>>(models).ToList();
                return Task.FromResult(new PagedList<ProductEntity>(entities, count, resourceParameters.PageNumber,
                    resourceParameters.PageSize));
            }
        }

        public async Task<ProductEntity> GetProduct(long id)
        {
            using (var connection = new SqliteConnection(_setting.ConnectionString))
            {
                var command = $"SELECT ID, Vendor, Name, Description, Asin, Upc, ProductType FROM Products WHERE ID = {id}";
                var result = await connection.QueryFirstOrDefaultAsync<ProductModel>(command);
                var product = _mapper.Map<ProductEntity>(result);      
                return product;
            }
        }

        public async Task CreateProduct(ProductEntity product)
        {
            using (var connection = new SqliteConnection(_setting.ConnectionString))
            {
                const string command = "INSERT INTO Products(ID, Vendor, Name, Description, Asin, Upc, ProductType) " +
                                       "VALUES (NULL, @Vendor, @Name, @Description, @Asin, @Upc, @ProductType); " +
                                       "SELECT LAST_INSERT_ROWID() AS id";
                product.Id = await connection.ExecuteScalarAsync<int>(command, product);
            }
        }

        public async Task UpdateProduct(ProductEntity product)
        {
            using (var connection = new SqliteConnection(_setting.ConnectionString))
            {
                const string command =
                    "UPDATE Products SET Vendor = @Vendor, Name = @Name, Description = @Description, " +
                    "Asin = @Asin, Upc = @Upc, ProductType = @ProductType " +
                    "WHERE Id = @Id";
                var result = await connection.ExecuteAsync(command, product);
            }
        }

        public async Task RemoveProduct(ProductEntity product)
        {
            using (var connection = new SqliteConnection(_setting.ConnectionString))
            {
                const string command =
                    "DELETE FROM Products WHERE Id = @Id";
                var result = await connection.ExecuteAsync(command, product);
            }
        }
    }
}
