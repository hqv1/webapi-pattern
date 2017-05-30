using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebApiPattern.Data.Sqlite.Test
{
    [TestClass]
    public class DbRepositoryTest
    {
        private const string ConnectionString = @"Data Source=WebApiPattern.sqlite";
        private DbRepository _dbRepository;

        [TestMethod]
        public void Should_CreateDbAndSchema_IfNotExist()
        {
            _dbRepository.CreateDb();
        }

        [TestMethod]
        public void Should_RepopulateDb()
        {
            _dbRepository.RepopulateDb();
        }

        [TestInitialize]
        public void Initialize()
        {
            _dbRepository = new DbRepository(new DbRepository.Setting(ConnectionString));
        }
    }
}
