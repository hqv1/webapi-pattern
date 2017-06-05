namespace WebApiPattern.Domain
{
    public interface IDbRepository
    {
        void CreateDb();
        void RepopulateDb();
    }
}