using System.Collections.Generic;
using System.Threading.Tasks;
using Hqv.CSharp.Common.Interfaces;
using Hqv.CSharp.Common.Web.Api;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using WebApiPattern.Asp.Shared.Models;
using WebApiPattern.Domain;
using WebApiPattern.Domain.Entities;

namespace WebApiPattern.Asp.CoreCore.Controllers
{
    [Route("api/v1/products/{productId}/sellers")]
    public class SellersController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ISellerRepository _sellerRepository;
        private readonly IMapper _mapper;

        public SellersController(IProductRepository productRepository, ISellerRepository sellerRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _sellerRepository = sellerRepository;
            _mapper = mapper;
        }

        [HttpGet(Name = "GetSellersForProduct")]
        [HttpHead]
        public async Task<IActionResult> GetSellersForProduct(long productId)
        {
            if (!await _productRepository.DoesProductExist(productId))           
                return NotFound(ExceptionMessageModelFactory.ParentToResourceNotFound());
            
            var sellers = await _sellerRepository.GetSellersForProduct(productId);
            var modelResults = _mapper.Map<IEnumerable<SellerForGetModel>>(sellers);
            return Ok(modelResults);
        }

        [HttpGet("{id}", Name = "GetSellerForProduct")]
        [HttpHead("{id}")]
        public async Task<IActionResult> GetSellerForProduct(long productId, long id)
        {
            if (!await _productRepository.DoesProductExist(productId))
                return NotFound(ExceptionMessageModelFactory.ParentToResourceNotFound());

            var seller = await _sellerRepository.GetSellerForProduct(productId, id);
            if (seller == null)
                return NotFound(ExceptionMessageModelFactory.ResourceNotFound());

            var result = _mapper.Map<SellerForGetModel>(seller);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSellerForProduct(long productId,
            [FromBody] SellerForCreationModel model)
        {
            if (model == null)
                return BadRequest(ExceptionMessageModelFactory.BadRequestBody());

            if (!ModelState.IsValid)
                return BadRequest(ExceptionMessageModelFactory
                    .BadRequestModelStateInvalid(new SerializableError(ModelState)));

            if (!await _productRepository.DoesProductExist(productId))
                return NotFound(ExceptionMessageModelFactory.ParentToResourceNotFound());

            var seller = _mapper.Map<SellerEntity>(model);
            await _sellerRepository.CreateSeller(productId, seller);
            var modelResult = _mapper.Map<SellerForGetModel>(seller);
            return CreatedAtRoute("GetSellerForProduct", new {productId, id = modelResult.Id}, modelResult);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpsertSeller(long productId, long id,
            [FromBody] SellerForUpdateModel model)
        {
            if (model == null)
                return BadRequest(ExceptionMessageModelFactory.BadRequestBody());

            if (!ModelState.IsValid)
                return BadRequest(ExceptionMessageModelFactory
                    .BadRequestModelStateInvalid(new SerializableError(ModelState)));

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

        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateSeller(long productId, long id,
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
                patchDoc.ApplyTo(sellerForUpdate, ModelState);
                
                TryValidateModel(sellerForUpdate);
                if (!ModelState.IsValid)
                    return BadRequest(ExceptionMessageModelFactory
                        .BadRequestModelStateInvalid(new SerializableError(ModelState)));

                var sellerToAdd = _mapper.Map<SellerEntity>(sellerForUpdate);
                sellerToAdd.Id = id;
                await _sellerRepository.CreateSeller(productId, sellerToAdd);
                var modelResult = _mapper.Map<SellerForGetModel>(sellerToAdd);
                return CreatedAtRoute("GetSellerForProduct", new {productId, id = modelResult.Id}, modelResult);
            }

            var sellerToPatch = _mapper.Map<SellerForUpdateModel>(sellerFromRepo);
            patchDoc.ApplyTo(sellerToPatch, ModelState);
           
            TryValidateModel(sellerToPatch);
            if (!ModelState.IsValid)
                return BadRequest(ExceptionMessageModelFactory
                    .BadRequestModelStateInvalid(new SerializableError(ModelState)));

            var updatedSeller = _mapper.Map<SellerEntity>(sellerFromRepo);
            _mapper.Map(sellerToPatch, updatedSeller);
            await _sellerRepository.UpdateSeller(updatedSeller);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveSeller(long productId, long id)
        {
            var seller = await _sellerRepository.GetSellerForProduct(productId, id);
            if (seller == null)
                return BadRequest(ExceptionMessageModelFactory.ResourceNotFound());
            await _sellerRepository.RemoveSeller(seller);
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