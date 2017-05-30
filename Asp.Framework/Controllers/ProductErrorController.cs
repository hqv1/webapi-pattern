using System;
using System.Web.Http;
using Newtonsoft.Json;

namespace WebApiPattern.Asp.Framework.Controllers
{
    [RoutePrefix("api/v1/products/error")]
    public class ProductErrorController : ApiController
    {
        [Route("")]
        [HttpPost]
        public IHttpActionResult CreateAnException([FromBody] object model)
        {
            try
            {
                throw new Exception("Some exception occurred", new Exception("Inner exception", new Exception("Inner inner exception")));
            }
            catch (Exception ex)
            {
                ex.Data["RequestBody"] = JsonConvert.SerializeObject(model);
                throw;
            }
        }
    }
}