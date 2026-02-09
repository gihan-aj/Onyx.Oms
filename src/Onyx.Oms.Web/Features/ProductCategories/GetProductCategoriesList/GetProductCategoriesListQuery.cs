using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.ProductCategories.GetProductCategoriesList;

public record GetProductCategoriesListQuery(bool OnlyLeaves = false, bool? IsActive = null) : IQuery<List<ProductCategoryDto>>;
