using System;
using Microsoft.AspNetCore.Mvc;

namespace WebApiPattern.Asp.CoreCore.Controllers
{
    /// <summary>
    /// Product Error resource.
    /// 
    /// Used to generate an unhandled exception. 
    /// </summary>
    [Route("api/v1/products/error")]
    public class ProductErrorController
    {   
        /// <summary>
        /// On a POST, throw an unhandled exception. 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CreateAnException([FromBody] object model)
        {
            try
            {              
                throw new Exception("Some exception occurred", new Exception("Inner exception", new Exception("Inner inner exception")));
            }
            catch (Exception ex)
            {
                ex.Data["RequestBody"] = model;
                throw;
            }
        }
    }
}