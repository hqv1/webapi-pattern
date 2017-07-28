using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Hqv.CSharp.Common.Map;
using Hqv.CSharp.Common.Web.Api;
using Marvin.JsonPatch;
using WebApiPattern.Asp.Shared.Models;
using WebApiPattern.Asp.Shared.Validators;
using WebApiPattern.Domain;
using WebApiPattern.Domain.Entities;

namespace WebApiPattern.Asp.Framework.Controllers
{
    [RoutePrefix("api/v1/products/{productId}/sellers")]
    public class SellersController : MyController
    {
        private readonly IProductRepository _productRepository;
        private readonly ISellerRepository _sellerRepository;
        private readonly IMapper _mapper;

        public SellersController(IProductRepository productRepository, ISellerRepository sellerRepository,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _sellerRepository = sellerRepository;
            _mapper = mapper;
        }

        [HttpGet, HttpHead, Route(Name = "GetSellersForProduct")]
        public async Task<IHttpActionResult> GetSellersForProduct(long productId)
        {
            if (!await _productRepository.DoesProductExist(productId))
                return NotFound(ExceptionMessageModelFactory.ParentToResourceNotFound());

            var sellers = await _sellerRepository.GetSellersForProduct(productId);
            var modelResults = _mapper.Map<IEnumerable<SellerForGetModel>>(sellers);
            return Ok(modelResults);
        }

        [HttpGet, HttpHead, Route("{id}", Name = "GetSellerForProduct")]
        public async Task<IHttpActionResult> GetSellerForProduct(long productId, long id)
        {
            if (!await _productRepository.DoesProductExist(productId))
                return NotFound(ExceptionMessageModelFactory.ParentToResourceNotFound());

            var seller = await _sellerRepository.GetSellerForProduct(productId, id);
            if (seller == null)
                return NotFound(ExceptionMessageModelFactory.ResourceNotFound());

            var result = _mapper.Map<SellerForGetModel>(seller);
            return Ok(result);
        }

        [HttpPost, Route]
        public async Task<IHttpActionResult> CreateSellerForProduct(long productId,
            [FromBody] SellerForCreationModel model)
        {
            if (model == null)
                return BadRequest(ExceptionMessageModelFactory.BadRequestBody());
           
            ValidateModel(new SellerForCreationModelValidator(), model);
            if (!ModelState.IsValid)
                return BadRequest(ExceptionMessageModelFactory
                    .BadRequestModelStateInvalid(ModelState.Values.SelectMany(x => x.Errors)));

            if (!await _productRepository.DoesProductExist(productId))
                return NotFound(ExceptionMessageModelFactory.ParentToResourceNotFound());

            var seller = _mapper.Map<SellerEntity>(model);
            await _sellerRepository.CreateSeller(productId, seller);
            var modelResult = _mapper.Map<SellerForGetModel>(seller);
            return CreatedAtRoute("GetSellerForProduct", new { productId, id = modelResult.Id }, modelResult);            
        }

        
        [HttpPut, Route("{id}")]
        public async Task<IHttpActionResult> UpsertSeller(long productId, long id,
            [FromBody] SellerForUpdateModel model)
        {
            if (model == null)
                return BadRequest(ExceptionMessageModelFactory.BadRequestBody());

            ValidateModel(new SellerForUpdateModelValidator(), model);
            if (!ModelState.IsValid)
                return BadRequest(ExceptionMessageModelFactory
                    .BadRequestModelStateInvalid(ModelState.Values.SelectMany(x => x.Errors)));

            if (!await _productRepository.DoesProductExist(productId))
                return NotFound(ExceptionMessageModelFactory.ParentToResourceNotFound());

            var sellerFromRepo = await _sellerRepository.GetSellerForProduct(productId, id);
            if (sellerFromRepo == null) // New Seller
            {
                if (await _sellerRepository.DoesSellerExist(id))
                {
                    return
                        BadRequest(
                            ExceptionMessageModelFactory.BadRequestParentMismatch("Seller id exist under another product"));
                }

                var sellerToAdd = _mapper.Map<SellerEntity>(model);
                sellerToAdd.Id = id;
                await _sellerRepository.CreateSeller(productId, sellerToAdd);
                var modelResult = _mapper.Map<SellerForGetModel>(sellerToAdd);
                return CreatedAtRoute("GetSellerForProduct", new { productId, id = modelResult.Id }, modelResult);
            }

            _mapper.Map(model, sellerFromRepo);
            await _sellerRepository.UpdateSeller(sellerFromRepo);
            return NoContent();           
        }

        [HttpPatch, Route("{id}")]
        public async Task<IHttpActionResult> PartiallyUpdateSeller(long productId, long id,
            [FromBody] JsonPatchDocument<SellerForUpdateModel> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest(ExceptionMessageModelFactory.BadRequestBody());

            if (!await _productRepository.DoesProductExist(productId))
                return NotFound(ExceptionMessageModelFactory.ParentToResourceNotFound());

            var sellerFromRepo = await _sellerRepository.GetSellerForProduct(productId, id);
            if (sellerFromRepo == null) // New Seller
            {
                var sellerForUpdate = new SellerForUpdateModel();
                try
                {
                    patchDoc.ApplyTo(sellerForUpdate);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ApplyPatch", ex.Message);
                }

                ValidateModel(new SellerForUpdateModelValidator(), sellerForUpdate);
                if (!ModelState.IsValid)
                    return BadRequest(ExceptionMessageModelFactory
                        .BadRequestModelStateInvalid(ModelState.Values.SelectMany(x => x.Errors)));

                var sellerToAdd = _mapper.Map<SellerEntity>(sellerForUpdate);
                sellerToAdd.Id = id;
                await _sellerRepository.CreateSeller(productId, sellerToAdd);
                var modelResult = _mapper.Map<SellerForGetModel>(sellerToAdd);
                return CreatedAtRoute("GetSellerForProduct", new { productId, id = modelResult.Id }, modelResult);
            }

            var sellerToPatch = _mapper.Map<SellerForUpdateModel>(sellerFromRepo);
            try
            {
                patchDoc.ApplyTo(sellerToPatch);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ApplyPatch", ex.Message);
            }

            ValidateModel(new SellerForUpdateModelValidator(), sellerToPatch);
            if (!ModelState.IsValid)
                return BadRequest(ExceptionMessageModelFactory
                    .BadRequestModelStateInvalid(ModelState.Values.SelectMany(x => x.Errors)));

            var updatedSeller = _mapper.Map<SellerEntity>(sellerFromRepo);
            _mapper.Map(sellerToPatch, updatedSeller);
            await _sellerRepository.UpdateSeller(updatedSeller);
            return NoContent();
        }

        [HttpDelete, Route("{id}")]
        public async Task<IHttpActionResult> RemoveSeller(long productId, long id)
        {
            var seller = await _sellerRepository.GetSellerForProduct(productId, id);
            if (seller == null)
                return BadRequest(ExceptionMessageModelFactory.ResourceNotFound());
            await _sellerRepository.RemoveSeller(seller);
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