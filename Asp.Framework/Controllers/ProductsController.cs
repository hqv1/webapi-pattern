using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;
using Hqv.CSharp.Common.Map;
using Hqv.CSharp.Common.Ordering;
using Hqv.CSharp.Common.Utilities;
using Hqv.CSharp.Common.Web.Api;
using Marvin.JsonPatch;
using WebApiPattern.Asp.Framework.Helpers;
using WebApiPattern.Asp.Shared.Models;
using WebApiPattern.Asp.Shared.Validators;
using WebApiPattern.Domain;
using WebApiPattern.Domain.Entities;

namespace WebApiPattern.Asp.Framework.Controllers
{
    [RoutePrefix("api/v1/products")]
    public class ProductsController : MyController
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ITypeHelper _typeHelper;
        private readonly IPropertyMappingService _propertyMappingService;

        public ProductsController(IProductRepository productRepository,
            IMapper mapper,
            ITypeHelper typeHelper,
            IPropertyMappingService propertyMappingService)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _typeHelper = typeHelper;
            _propertyMappingService = propertyMappingService;
        }

        /// <summary>
        /// Return a HttpResponseMessage instead of IHttpActionResult because we are adding custom headers 
        /// and Request.CreateResponse returns a HttpResponseMessage.
        /// </summary>
        /// <param name="resourceParameters"></param>
        /// <param name="filterParameters"></param>
        /// <returns></returns>
        [HttpGet, HttpHead, Route(Name = "GetProducts")]
        public async Task<HttpResponseMessage> GetProducts([FromUri] ResourceParameters resourceParameters,
            [FromUri] ProductFilterParameters filterParameters)
        {
            filterParameters = filterParameters ?? new ProductFilterParameters();
            resourceParameters = resourceParameters ?? new ResourceParameters();
            if (
                !_propertyMappingService.ValidMappingExistsFor<ProductEntity, Data.Sqlite.Models.ProductModel>(
                    resourceParameters.OrderBy))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                    ExceptionMessageModelFactory.BadRequstOrderByParameters());
            }

            if (!_typeHelper.TypeHasProperties<ProductForGetModel>(resourceParameters.Fields))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                    ExceptionMessageModelFactory.BadRequstFieldsParameters());
            }

            var products = await _productRepository.GetProducts(resourceParameters, filterParameters);

            // UrlHelper requires the Request to get
            var pagedListHelper = new PagedListHelper(new UrlHelper(Request));

            var modelResults = _mapper.Map<IEnumerable<ProductForGetModel>>(products);
            var response = Request.CreateResponse(HttpStatusCode.OK, modelResults.ShapeData(resourceParameters.Fields));
            response.Headers.Add("X-Pagination",
                pagedListHelper.AddPaginationMetadataToResponseHeader(products, resourceParameters, filterParameters,
                    "GetProducts"));
            return response;
        }

        [HttpGet, HttpHead, Route("{id}", Name = "GetProduct")]
        public async Task<IHttpActionResult> GetProduct(long id, [FromUri] string fields = null)
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

        [HttpPost, Route]
        public async Task<IHttpActionResult> CreateProduct([FromBody] ProductForCreationModel model)
        {
            if (model == null)
                return BadRequest(ExceptionMessageModelFactory.BadRequestBody());

            
            ValidateModel(new ProductForCreationModelValidator(), model);
            if (!ModelState.IsValid)
                return BadRequest(ExceptionMessageModelFactory
                    .BadRequestModelStateInvalid(ModelState.Values.SelectMany(x => x.Errors)));

            var product = _mapper.Map<ProductEntity>(model);
            await _productRepository.CreateProduct(product);

            var modelResult = _mapper.Map<ProductForGetModel>(product);
            return CreatedAtRoute("GetProduct", new {id = product.Id}, modelResult);
        }

        [HttpPut, Route("{id}")]
        public async Task<IHttpActionResult> UpdateProduct(long id, [FromBody] ProductForUpdateModel model)
        {
            if (model == null)
                return BadRequest(ExceptionMessageModelFactory.BadRequestBody());
           
            ValidateModel(new ProductForUpdateModelValidator(), model);
            if (!ModelState.IsValid)
                return BadRequest(ExceptionMessageModelFactory
                    .BadRequestModelStateInvalid(ModelState.Values.SelectMany(x => x.Errors)));

            var product = await _productRepository.GetProduct(id);
            if (product == null)
                return Content(HttpStatusCode.BadRequest, ExceptionMessageModelFactory.ResourceNotFound());

            _mapper.Map(model, product);
            await _productRepository.UpdateProduct(product);
            return NoContent();
        }

        [HttpPatch, Route("{id}")]
        public async Task<IHttpActionResult> PartiallyUpdateProduct(long id,
            [FromBody] JsonPatchDocument<ProductForUpdateModel> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest(ExceptionMessageModelFactory.BadRequestBody());

            var product = await _productRepository.GetProduct(id);
            if (product == null)
                return BadRequest(ExceptionMessageModelFactory.ResourceNotFound());

            var productToPatch = _mapper.Map<ProductForUpdateModel>(product);
            try
            {
                patchDoc.ApplyTo(productToPatch);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ApplyPatch", ex.Message);
            }

            ValidateModel(new ProductForUpdateModelValidator(), productToPatch);
            if (!ModelState.IsValid)
                return BadRequest(ExceptionMessageModelFactory
                    .BadRequestModelStateInvalid(ModelState.Values.SelectMany(x => x.Errors)));

            var updatedProduct = _mapper.Map<ProductEntity>(product);
            _mapper.Map(productToPatch, updatedProduct);
            await _productRepository.UpdateProduct(updatedProduct);
            return NoContent();
        }

        [HttpDelete, Route("{id}")]
        public async Task<IHttpActionResult> DeleteProduct(long id)
        {
            var product = await _productRepository.GetProduct(id);
            if (product == null)
                return BadRequest(ExceptionMessageModelFactory.ResourceNotFound());
            await _productRepository.RemoveProduct(product);
            return NoContent();
        }

        [HttpOptions, Route]
        public HttpResponseMessage GetOptions()
        {
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent("");
            response.Content.Headers.Add("Allow", "GET,OPTIONS,POST,PUT,PATCH,DELETE");
            return response;
        }
    }

}