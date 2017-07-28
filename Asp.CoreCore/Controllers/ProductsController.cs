using System.Collections.Generic;
using System.Threading.Tasks;
using Hqv.CSharp.Common.Map;
using Hqv.CSharp.Common.Ordering;
using Hqv.CSharp.Common.Utilities;
using Hqv.CSharp.Common.Web.Api;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using WebApiPattern.Asp.CoreCore.Filters;
using WebApiPattern.Asp.Shared.Models;
using WebApiPattern.Domain;
using WebApiPattern.Domain.Entities;

namespace WebApiPattern.Asp.CoreCore.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/v1/products")]
    [ResponseCache(Duration = 30)]
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ITypeHelper _typeHelper;
        private readonly IPropertyMappingService _propertyMappingService;
        private readonly IPagedListHelper _pagedListHelper;
        
        public ProductsController(IProductRepository productRepository, IMapper mapper, 
            ITypeHelper typeHelper,
            IPropertyMappingService propertyMappingService,
            IPagedListHelper pagedListHelper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _typeHelper = typeHelper;
            _propertyMappingService = propertyMappingService;
            _pagedListHelper = pagedListHelper;
        }

        /// <summary>
        /// Get a list of products.
        /// 
        /// This method implements a few patterns.
        /// 
        /// It implements paging. It uses an X-Pagination header to include pagination data
        /// It implements filtering by property
        /// It implements searching 
        /// It implements ordering
        /// It implements shaping
        /// </summary>
        /// <param name="resourceParameters">Paging information</param>
        /// <param name="filterParameters"></param>
        /// <returns></returns>
        [HttpGet(Name = "GetProducts")]
        [HttpHead]
        public async Task<IActionResult> GetProducts(ResourceParameters resourceParameters,
            ProductFilterParameters filterParameters)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<ProductEntity, Data.Sqlite.Models.ProductModel>(resourceParameters.OrderBy))
            {
                return BadRequest(ExceptionMessageModelFactory.BadRequstOrderByParameters());
            }

            if (!_typeHelper.TypeHasProperties<ProductForGetModel>(resourceParameters.Fields))
            {
                return BadRequest(ExceptionMessageModelFactory.BadRequstFieldsParameters());
            }

            var products = await _productRepository.GetProducts(resourceParameters, filterParameters);
            AddHeaders();
            var modelResults = _mapper.Map<IEnumerable<ProductForGetModel>>(products);
            return Ok(modelResults.ShapeData(resourceParameters.Fields));

            // Local method to get create headers. Is this overkill? Probably.
            void AddHeaders() => Response.Headers.Add("X-Pagination",
                _pagedListHelper.AddPaginationMetadataToResponseHeader(products, resourceParameters, filterParameters,
                    "GetProducts"));
        }

        /// <summary>
        /// Pattern to get a single resource.
        /// 
        /// Get the resource from the repository. No need to access the logic layer if there's 
        /// no work done there. Always use async.
        /// 
        /// If product isn't found, return a 404 with additional data.
        /// 
        /// Map the entity to a model. 
        /// Return the model with the OK status code.
        /// 
        /// It implements shaping
        /// 
        /// It returns ETags and will return Not Modified status codes.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetProduct")]
        [HttpHead("{id}")]
        [ETagFilter]
        public async Task<IActionResult> GetProduct(long id, [FromQuery] string fields)
        {
            if (!_typeHelper.TypeHasProperties<ProductForGetModel>(fields))
            {
                return BadRequest(ExceptionMessageModelFactory.BadRequstFieldsParameters());
            }

            var product = await _productRepository.GetProduct(id);
            if (product == null)
                return NotFound(ExceptionMessageModelFactory.ResourceNotFound());

            var model = _mapper.Map<ProductForGetModel>(product);
            return Ok(model.ShapeData(fields));
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductForCreationModel model)
        {
            if (model == null)
                return BadRequest(ExceptionMessageModelFactory.BadRequestBody());           

            if (!ModelState.IsValid)
                return BadRequest(ExceptionMessageModelFactory
                    .BadRequestModelStateInvalid(new SerializableError(ModelState)));

            var product = _mapper.Map<ProductEntity>(model);
            await _productRepository.CreateProduct(product);

            var modelResult = _mapper.Map<ProductForGetModel>(product);
            return CreatedAtRoute("GetProduct", new { id = product.Id }, modelResult);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(long id, [FromBody] ProductForUpdateModel model)
        {
            if (model == null)
                return BadRequest(ExceptionMessageModelFactory.BadRequestBody());

            if (!ModelState.IsValid)
                return BadRequest(ExceptionMessageModelFactory
                    .BadRequestModelStateInvalid(new SerializableError(ModelState)));

            var product = await _productRepository.GetProduct(id);
            if (product == null)
                return BadRequest(ExceptionMessageModelFactory.ResourceNotFound());

            _mapper.Map(model, product);
            await _productRepository.UpdateProduct(product);
            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateProduct(long id, 
            [FromBody] JsonPatchDocument<ProductForUpdateModel>  patchDoc)
        {           
            if (patchDoc == null)
                return BadRequest(ExceptionMessageModelFactory.BadRequestBody());

            var product = await _productRepository.GetProduct(id);
            if (product == null)
                return BadRequest(ExceptionMessageModelFactory.ResourceNotFound());

            var productToPatch = _mapper.Map<ProductForUpdateModel>(product);
            patchDoc.ApplyTo(productToPatch, ModelState);

            TryValidateModel(productToPatch);
            if (!ModelState.IsValid)
                return BadRequest(ExceptionMessageModelFactory
                    .BadRequestModelStateInvalid(new SerializableError(ModelState)));

            var updatedProduct = _mapper.Map<ProductEntity>(product);
            _mapper.Map(productToPatch, updatedProduct);
            await _productRepository.UpdateProduct(updatedProduct);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            var product = await _productRepository.GetProduct(id);
            if (product == null)
                return BadRequest(ExceptionMessageModelFactory.ResourceNotFound());
            await _productRepository.RemoveProduct(product);
            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST,PUT,PATCH,DELETE");
            return Ok();
        }
        
    }
}