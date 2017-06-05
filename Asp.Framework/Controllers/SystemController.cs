using System.Net;
using System.Web.Http;
using WebApiPattern.Domain;

namespace WebApiPattern.Asp.Framework.Controllers
{
    [RoutePrefix("api/v1/system")]
    public class SystemController : MyController
    {
        private readonly IDbRepository _dbRepository;

        public SystemController(IDbRepository dbRepository)
        {
            _dbRepository = dbRepository;
        }
        
        [HttpPost, Route("create")]
        public IHttpActionResult CreateDatabase()
        {
            _dbRepository.CreateDb();
            return NoContent();
        }

        [HttpPost, Route("repopulate")]
        public IHttpActionResult RepopulateData()
        {
            _dbRepository.RepopulateDb();
            return NoContent();
        }
    }
}
