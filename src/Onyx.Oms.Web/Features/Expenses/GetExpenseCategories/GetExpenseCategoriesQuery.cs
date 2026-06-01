using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.Expenses.GetExpenseCategories;

public record GetExpenseCategoriesQuery : IQuery<IReadOnlyList<string>>;
