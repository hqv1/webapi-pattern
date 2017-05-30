using FluentValidation;
using Hqv.CSharp.Common.Extensions;
using WebApiPattern.Asp.Shared.Models;

namespace WebApiPattern.Asp.Shared.Validators
{
    /// <summary>
    /// Validation for ProdcutForCreationModel
    /// </summary>
    public class ProductForCreationModelValidator : AbstractValidator<ProductForCreationModel>
    {
        public ProductForCreationModelValidator()
        {
            RuleFor(x => x.Vendor).NotEmpty().Length(1, 50);
            RuleFor(x => x.Name).NotEmpty().Length(1, 100);
            RuleFor(x => x.Description).Length(0, 200);
            RuleFor(x => x.Asin).Length(0, 12);
            RuleFor(x => x.Upc).Length(0,20).Must(StringExtensions.IsNumeric).WithMessage("Upc is in an invalid format");
        }
    }
}