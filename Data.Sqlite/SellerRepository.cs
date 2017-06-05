using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Hqv.CSharp.Common.Interfaces;
using Microsoft.Data.Sqlite;
using WebApiPattern.Data.Sqlite.Models;
using WebApiPattern.Domain;
using WebApiPattern.Domain.Entities;

namespace WebApiPattern.Data.Sqlite
{
    public class SellerRepository : ISellerRepository
    {
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

        private readonly Setting _setting;
        private readonly IMapper _mapper;

        public SellerRepository(Setting setting, IMapper mapper)
        {
            _setting = setting;
            _mapper = mapper;
        }
       
        public async Task<bool> DoesSellerExist(long id)
        {
            using (var connection = new SqliteConnection(_setting.ConnectionString))
            {
                var command = $@"SELECT COUNT(*) FROM Sellers WHERE ID = {id}";
                var result = await connection.ExecuteScalarAsync<int>(command);
                return result >= 1;
            }          
        }

        public async Task<IEnumerable<SellerEntity>> GetSellersForProduct(long productId)
        {
            using (var connection = new SqliteConnection(_setting.ConnectionString))
            {
                var command = $"SELECT ID, ProductID, Name, Quantity, Price FROM Sellers WHERE ProductID = {productId}";
                var results = await connection.QueryAsync<SellerModel>(command);
                var entities = _mapper.Map<IEnumerable<SellerEntity>>(results);
                return entities;
            }
        }

        public async Task<SellerEntity> GetSellerForProduct(long productId, long id)
        {
            using (var connection = new SqliteConnection(_setting.ConnectionString))
            {
                var command = "SELECT ID, ProductID, Name, Quantity, Price FROM Sellers " + 
                    $"WHERE ProductID = {productId} AND ID = {id}";
                var result = await connection.QuerySingleOrDefaultAsync<SellerModel>(command);
                var entity = _mapper.Map<SellerEntity>(result);
                return entity;
            }
        }

        public async Task CreateSeller(long productId, SellerEntity seller)
        {
            using (var connection = new SqliteConnection(_setting.ConnectionString))
            {              
                string command;
                if (seller.Id <= 0)
                {
                    command = "INSERT INTO Sellers(ID, ProductID, Name, Quantity, Price) " +
                              $"VALUES (NULL, {productId}, @Name, @Quantity, @Price); " +
                              "SELECT LAST_INSERT_ROWID() AS id";
                }
                else
                {
                    command = "INSERT INTO Sellers(ID, ProductID, Name, Quantity, Price) " +
                              $"VALUES ({seller.Id}, {productId}, @Name, @Quantity, @Price); " +
                              "SELECT LAST_INSERT_ROWID() AS id";
                }
                
                seller.Id = await connection.ExecuteScalarAsync<int>(command, seller);
            }
        }

        public async Task UpdateSeller(SellerEntity seller)
        {
            using (var connection = new SqliteConnection(_setting.ConnectionString))
            {
                const string command =
                    "UPDATE Sellers SET Name = @Name, Quantity = @Quantity, Price = @Price " +
                    "WHERE ID = @Id";
                var result = await connection.ExecuteAsync(command, seller);
            }
        }

        public async Task RemoveSeller(SellerEntity seller)
        {
            using (var connection = new SqliteConnection(_setting.ConnectionString))
            {
                const string command =
                    "DELETE FROM Sellers WHERE Id = @Id";
                var result = await connection.ExecuteAsync(command, seller);
            }
        }
    }
}