using System;
using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using WebApiPattern.Domain;
using WebApiPattern.Domain.Entities;

namespace WebApiPattern.Data.Sqlite
{
    public class DbRepository : IDbRepository
    {
        private readonly Setting _setting;

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
        
        public DbRepository(Setting setting)
        {
            _setting = setting;
        }

        public void CreateDb()
        {            
            using (var connection = new SqliteConnection(_setting.ConnectionString))
            {
                CreateProductTable(connection);
                CreateSellerTable(connection);
            }         
        }

        private static void CreateProductTable(IDbConnection connection)
        {
            connection.Execute(@"DROP TABLE IF EXISTS Products");

            const string command = @"CREATE TABLE Products (
                        ID              INTEGER PRIMARY KEY AUTOINCREMENT,
                        Vendor          varchar(100) not null,
                        Name            varchar(100) not null,
                        Description     varchar(250),
                        Asin            varchar(20),
                        Upc             varchar(20),
                        ProductType     varchar(50)
                    );";
            connection.Execute(command);
        }

        private static void CreateSellerTable(IDbConnection connection)
        {
            connection.Execute(@"DROP TABLE IF EXISTS Sellers");

            const string command = @"CREATE TABLE Sellers (
                        ID            INTEGER PRIMARY KEY AUTOINCREMENT,
                        ProductID     INTEGER not null,
                        Name          varchar(100) not null,
                        Quantity      NUMERIC not null,
                        Price         DECIMAL(12,3)
                    );";
            connection.Execute(command);
        }

        public void RepopulateDb()
        {
            using (var connection = new SqliteConnection(_setting.ConnectionString))
            {
                CreateProductTable(connection);
                CreateSellerTable(connection);
            }

            CreateIntialProduct(new ProductEntity
            {
                Id = 1,
                Vendor = "Cellucor",
                Name = "C4 Original Explosive Pre-Workout Supplement",
                Description = "Fruit Punch, 6.3 Ounce",
                Asin = "B01N272UAI",
                Upc = "810390028399",
                ProductType = "Perishable",
            });

            CreateIntialProduct(new ProductEntity
            {
                Id = 2,
                Vendor = "Ozetti",
                Name = "Manual Coffee Grinder",
                Description = "Brushed Stainless Steel Espresso Grinder",
                Asin = "B01KKEHGX4",
                ProductType = "Non-Perishable",
            });

            CreateIntialProduct(new ProductEntity
            {
                Id = 3,
                Vendor = "Logitech",
                Name = "G602 Gaming Wireless Mouse",
                Asin = "B00E4MQODC",
                ProductType = "Non-Perishable",
            });

            CreateInitialSeller(1, new SellerEntity
            {
                Id = 1,
                Name = "Amazon",
                Url = @"https://www.amazon.com/Cellucor-Original-Explosive-Pre-Workout-Supplement/dp/B01N272UAI/",
                Quantity = 1,
                Price = 30.30M
            });

            CreateInitialSeller(1, new SellerEntity
            {
                Id = 2,
                Name = "GNC",
                Url = @"http://www.gnc.com/nitric-oxide-1/CellucorC4.html",
                Quantity = 1,
                Price = 49.99M
            });

            CreateInitialSeller(1, new SellerEntity
            {
                Id = 3,
                Name = "Walmart",
                Url = @"https://www.walmart.com/ip/C4/153289153",
                Quantity = 1,
                Price = 35.99M
            });

            CreateInitialSeller(2, new SellerEntity
            {
                Id = 4,
                Name = "Amazon",
                Url = @"https://www.amazon.com/Manual-Coffee-Grinder-Precision-Stainless/dp/B01KKEHGX4/",
                Quantity = 1,
                Price = 17.78M
            });

            CreateInitialSeller(2, new SellerEntity
            {
                Id = 5,
                Name = "Bonanza",
                Url = @"https://www.bonanza.com/listings/Ozetti-Manual-Coffee-Grinder-Ozetti-Precision-Burr-Mill-Gourmet-Brewing-Brush/",
                Quantity = 1,
                Price = 36.42M
            });

            CreateInitialSeller(3, new SellerEntity
            {
                Id = 6,
                Name = "Amazon",
                Url = @"https://www.amazon.com/Logitech-Gaming-Wireless-Mouse-Battery",
                Quantity = 1,
                Price = 39.99M
            });
        }

        private void CreateIntialProduct(ProductEntity product)
        {
            using (var connection = new SqliteConnection(_setting.ConnectionString))
            {
                var command = $@"INSERT INTO Products (Id, Vendor, Name, Description, Asin, Upc, ProductType) VALUES 
                        ({product.Id}, '{product.Vendor}', '{product.Name}', '{product.Description}', 
                            '{product.Asin}', '{product.Upc}', '{product.ProductType}')";
                connection.Execute(command);
            }
        }

        private void CreateInitialSeller(long productId, SellerEntity seller)
        {
            using (var connection = new SqliteConnection(_setting.ConnectionString))
            {
                var command = $@"INSERT INTO Sellers (Id, ProductId, Name, Quantity, Price) VALUES 
                        ({seller.Id}, {productId}, '{seller.Name}', {seller.Quantity}, {seller.Price} )";
                connection.Execute(command);
            }
        }
    }
}