using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.ProductCategories.DeleteProductCategory;

public record DeleteProductCategoryCommand(Guid Id) : ICommand;
