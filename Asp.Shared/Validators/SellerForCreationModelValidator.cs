using FluentValidation;
using WebApiPattern.Asp.Shared.Models;

namespace WebApiPattern.Asp.Shared.Validators
{
    /// <summary>
    /// Validation for SellerForCreation
    /// </summary>
    public class SellerForCreationModelValidator : AbstractValidator<SellerForCreationModel>
    {
        public SellerForCreationModelValidator()
        {
            RuleFor(x => x.Name).NotEmpty().Length(1, 100);
            RuleFor(x => x.Quantity).NotEmpty().GreaterThanOrEqualTo(0);
            RuleFor(x => x.Price).NotEmpty().GreaterThanOrEqualTo(0);
            RuleFor(x => x.Url).Length(0, 250);
        }        
    }
}