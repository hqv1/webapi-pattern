using Microsoft.EntityFrameworkCore;
using WebApiPattern.Data.Sqlite.Models;

namespace WebApiPattern.Data.Sqlite
{
    /// <summary>
    /// Entity Framework DB Context
    /// </summary>
    public class WebApiPatternContext : DbContext
    {
        private readonly string _connectionString;

        public WebApiPatternContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<ProductModel> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }
    }
}