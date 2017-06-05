using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hqv.CSharp.Common.Interfaces;
using Hqv.CSharp.Common.Web.Api;
using Microsoft.AspNetCore.Mvc;
using WebApiPattern.Asp.CoreCore.Helpers;
using WebApiPattern.Asp.Shared.Models;
using WebApiPattern.Domain;
using WebApiPattern.Domain.Entities;

namespace WebApiPattern.Asp.CoreCore.Controllers
{
    /// <summary>
    /// Product collection resource. 
    /// </summary>
    [Route("api/v1/productcollections")]
    public class ProductCollectionsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProductRepository _productRepository;

        public ProductCollectionsController(IMapper mapper, IProductRepository productRepository)
        {
            _mapper = mapper;
            _productRepository = productRepository;
        }

        /// <summary>
        /// Get a collection of products by ids.
        /// 
        /// An example: /api/productcollection/(1,20,33)
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpGet("({ids})", Name= "GetProductCollection")]
        [HttpHead("({ids})")]
        public async Task<IActionResult> GetProductCollection(
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

        /// <summary>
        /// Create a collection of products
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateProductCollections([FromBody] IEnumerable<ProductForCreationModel> models)
        {
            if(models == null)
                return BadRequest(ExceptionMessageModelFactory.BadRequestBody());

            if (!ModelState.IsValid)
                return BadRequest(ExceptionMessageModelFactory
                    .BadRequestModelStateInvalid(new SerializableError(ModelState)));

            var products = _mapper.Map<IEnumerable<ProductEntity>>(models);
            var tasks = products.Select(product => _productRepository.CreateProduct(product));
            await Task.WhenAll(tasks);

            var modelResults = _mapper.Map<IEnumerable<ProductForGetModel>>(products);
            var ids = string.Join(",", modelResults.Select(model => model.Id));
            return CreatedAtRoute("GetProductCollection", new {ids}, modelResults);
        }       

        /// <summary>
        /// Get options for the resource
        /// </summary>
        /// <returns></returns>
        [HttpOptions]
        public IActionResult GetOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST");
            return Ok();
        }
    }
}