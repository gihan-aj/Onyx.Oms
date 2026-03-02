using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.ProductCategories.GetProductCategoryById;

public class GetProductCategoryByIdHandler : IQueryHandler<GetProductCategoryByIdQuery, ProductCategoryResponse>
{
    private readonly IApplicationDbContext _context;

    public GetProductCategoryByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProductCategoryResponse>> Handle(GetProductCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _context.ProductCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure<ProductCategoryResponse>(Error.NotFound("ProductCategory.NotFound", "Product Category not found."));
        }

        var response = new ProductCategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.ParentCategoryId,
            category.Level,
            category.Path,
            category.NamePath,
            category.IsActive,
            category.DisplayOrder,
            category.IconUrl,
            category.Color,
            category.Specifications.ToList()
        );

        return Result.Success(response);
    }
}
