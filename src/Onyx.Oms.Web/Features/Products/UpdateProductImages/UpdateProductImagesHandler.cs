using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.UpdateProductImages
{
    public class UpdateProductImagesHandler : ICommandHandler<UpdateProductImagesCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateProductImagesHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateProductImagesCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product is null)
                return Result.Failure(Error.NotFound("Product.NotFound", "Product not found."));

            var imageData = request.Images.Select(i => (i.Id, i.Url, i.DisplayOrder, i.IsMain, i.OptionName, i.OptionValue));
            
            var updateResult = product.UpdateImages(imageData);
            if (updateResult.IsFailure)
                return updateResult;

            var newImages = updateResult.Value;
            if(newImages != null && newImages.Count > 0)
            {
                _context.ProductImages.AddRange(newImages);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
