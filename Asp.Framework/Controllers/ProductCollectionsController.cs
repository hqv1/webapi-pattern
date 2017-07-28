using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Hqv.CSharp.Common.Map;
using Hqv.CSharp.Common.Web.Api;
using WebApiPattern.Asp.Framework.Helpers;
using WebApiPattern.Asp.Shared.Models;
using WebApiPattern.Asp.Shared.Validators;
using WebApiPattern.Domain;
using WebApiPattern.Domain.Entities;

namespace WebApiPattern.Asp.Framework.Controllers
{
    [RoutePrefix("api/v1/productcollections")]
    public class ProductCollectionsController : MyController
    {
        private readonly IMapper _mapper;
        private readonly IProductRepository _productRepository;

        public ProductCollectionsController(IMapper mapper, IProductRepository productRepository)
        {
            _mapper = mapper;
            _productRepository = productRepository;
        }

        [HttpGet, HttpHead, Route("{ids}", Name = "GetProductCollection")]
        public async Task<IHttpActionResult> GetProductCollection(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<long> ids)
        {
            if (ids == null)
                return BadRequest(ExceptionMessageModelFactory.BadRequestBody());

            ids = ids.ToList();
            var tasks = ids.Select(id => _productRepository.GetProduct(id));
            var products = (await Task.WhenAll(tasks)).Where(x => x != null).ToList();

            if (ids.Count() != products.Count)
            {
                return NotFound(ExceptionMessageModelFactory.ResourceNotFound(
                    $"Expected to retrieve {ids.Count()} but only able to retrieve {products.Count}"));
            }

            var models = _mapper.Map<IEnumerable<ProductForGetModel>>(products);
            return Ok(models);           
        }


        [HttpPost, Route]
        public async Task<IHttpActionResult> CreateProductCollections([FromBody] IEnumerable<ProductForCreationModel> models)
        {
            if (models == null)
                return BadRequest(ExceptionMessageModelFactory.BadRequestBody());

            var validator = new ProductForCreationModelValidator();
            foreach (var model in models)
            {
                ValidateModel(validator, model);
            }
            //todo: error message does not sure exactly which model caused the error.
            if (!ModelState.IsValid)
                return BadRequest(ExceptionMessageModelFactory
                    .BadRequestModelStateInvalid(ModelState.Values.SelectMany(x => x.Errors)));

            var products = _mapper.Map<IEnumerable<ProductEntity>>(models);
            var tasks = products.Select(product => _productRepository.CreateProduct(product));
            await Task.WhenAll(tasks);

            var modelResults = _mapper.Map<IEnumerable<ProductForGetModel>>(products).ToList();
            var ids = string.Join(",", modelResults.Select(model => model.Id));
            return CreatedAtRoute("GetProductCollection", new { ids }, modelResults);            
        }

        [HttpOptions, Route]
        public HttpResponseMessage GetOptions()
        {
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent("");
            response.Content.Headers.Add("Allow", "GET,OPTIONS,POST");
            return response;
        }
    }
}