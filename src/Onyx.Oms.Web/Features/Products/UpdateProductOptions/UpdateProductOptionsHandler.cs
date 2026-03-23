using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Products.UpdateProductOptions
{
    public class UpdateProductOptionsHandler : ICommandHandler<UpdateProductOptionsCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public UpdateProductOptionsHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(UpdateProductOptionsCommand request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .Include(p => p.Variants) // Required for UpdateOptionValues to validate the deletion of existing variants
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product is null)
                return Result.Failure(Error.NotFound("Product.NotFound", "Product not found."));

            var options = request.Options.Select(o => new ProductOption
            {
                Name = o.Name,
                Values = o.Values,
            }).ToList();

            var validVariantMatrix = GetCombinations(request.Options);

            var userId = _currentUserService.UserId ?? "System - Option value removed";

            var updateResult = product.UpdateOptionValues(options, validVariantMatrix, userId);
            
            if (updateResult.IsFailure)
                return Result.Failure(updateResult.Error);

            var newVariants = updateResult.Value;
            if (newVariants.Count > 0)
                _context.ProductVariants.AddRange(newVariants);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        private List<List<VariantAttribute>> GetCombinations(List<UpdateProductOptionDto> options)
        {
            var result = new List<List<VariantAttribute>>();
            if (options.Count == 0)
                return result;

            void Permute(int depth, List<VariantAttribute> current)
            {
                if (depth == options.Count)
                {
                    // FIX: Create a deep copy of the current attributes 
                    // so EF Core doesn't share memory references between variants
                    var deepCopy = current.Select(a => new VariantAttribute
                    {
                        Name = a.Name,
                        Value = a.Value
                    }).ToList();

                    result.Add(deepCopy);
                    return;
                }

                var currentOption = options[depth];
                foreach (var val in currentOption.Values)
                {
                    current.Add(new VariantAttribute { Name = currentOption.Name, Value = val });
                    Permute(depth + 1, current);
                    current.RemoveAt(current.Count - 1);
                }
            }

            Permute(0, new List<VariantAttribute>());   
            return result;
        }
    }
}
