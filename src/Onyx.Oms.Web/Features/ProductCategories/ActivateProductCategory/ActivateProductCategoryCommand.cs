using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.ProductCategories.ActivateProductCategory;

public record ActivateProductCategoryCommand(Guid Id) : ICommand;
