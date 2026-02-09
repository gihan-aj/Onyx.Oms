using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.ProductCategories.GetProductCategoryTree;

public record GetProductCategoryTreeQuery(bool? IsActive = null) : IQuery<List<ProductCategoryTreeDto>>;
