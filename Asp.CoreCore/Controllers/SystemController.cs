using Microsoft.AspNetCore.Mvc;
using WebApiPattern.Domain;

namespace WebApiPattern.Asp.CoreCore.Controllers
{
    [Route("api/v1/system")]
    public class SystemController : Controller
    {
        private readonly IDbRepository _dbRepository;

        public SystemController(IDbRepository dbRepository)
        {
            _dbRepository = dbRepository;
        }

        [HttpPost("create")]
        public IActionResult CreateDatabase()
        {
            _dbRepository.CreateDb();
            return NoContent();
        }

        [HttpPost("repopulate")]
        public IActionResult RepopulateData()
        {
            _dbRepository.RepopulateDb();
            return NoContent();
        }
    }
}