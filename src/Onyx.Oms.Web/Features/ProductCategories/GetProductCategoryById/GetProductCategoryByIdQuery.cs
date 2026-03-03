using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.ProductCategories.GetProductCategoryById;

public record GetProductCategoryByIdQuery(Guid Id, bool IncludeParentSpecs = false) : IQuery<ProductCategoryResponse>;
