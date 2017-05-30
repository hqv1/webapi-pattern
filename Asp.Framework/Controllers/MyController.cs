using System.Net;
using System.Web.Http;
using FluentValidation;
using FluentValidation.WebApi;
using Hqv.CSharp.Common.Web.Api;

namespace WebApiPattern.Asp.Framework.Controllers
{
    public class MyController : ApiController
    {
        internal IHttpActionResult BadRequest(ExceptionMessageModel exceptionMessageModel)
        {
            return Content(HttpStatusCode.BadRequest, exceptionMessageModel);
        }

        internal IHttpActionResult NoContent()
        {
            return StatusCode(HttpStatusCode.NoContent);
        }

        internal IHttpActionResult NotFound(ExceptionMessageModel exceptionMessageModel)
        {
            return Content(HttpStatusCode.NotFound, exceptionMessageModel);
        }

        //internal IHttpActionResult UnprocessableEntity(ModelStateDictionary modelState)
        //{
        //    return Content((HttpStatusCode)422, ModelState);
        //}

        public void ValidateModel<T>(AbstractValidator<T> validator, T model)
        {
            //// Calling the validator manually because I really don't want to add attributes to Model
            //// and I don't want to spend the time to figure out how to read the assembly to load all
            //// the validation
            //var validator = new SellerForCreationModelValidator();
            var results = validator.Validate(model);
            results.AddToModelState(ModelState, null);
        }
    }
}