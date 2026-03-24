using FluentValidation;

namespace Onyx.Oms.Web.Features.Products.UpdateProductImages
{
    public class UpdateProductImagesCommandValidator : AbstractValidator<UpdateProductImagesCommand>
    {
        public UpdateProductImagesCommandValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty();
            RuleForEach(x => x.Images).SetValidator(new UpdateProductImageDtoValidator());
        }
    }

    public class UpdateProductImageDtoValidator : AbstractValidator<UpdateProductImageDto>
    {
        public UpdateProductImageDtoValidator()
        {
            RuleFor(x => x.Url).NotEmpty();
            //RuleFor(x => x.Url).NotEmpty().Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            //    .WithMessage("Url must be a valid absolute URI.");
            RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        }
    }
}
