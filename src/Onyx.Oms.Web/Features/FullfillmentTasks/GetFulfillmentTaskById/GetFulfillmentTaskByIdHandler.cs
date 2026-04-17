using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Core.Messaging;

namespace Onyx.Oms.Web.Features.FullfillmentTasks.GetFulfillmentTaskById;

public class GetFulfillmentTaskByIdHandler : IQueryHandler<GetFulfillmentTaskByIdQuery, FulfillmentTaskByIdDto>
{
    private readonly IApplicationDbContext _context;

    public GetFulfillmentTaskByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<FulfillmentTaskByIdDto>> Handle(GetFulfillmentTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await (
            from t in _context.FulfillmentTasks.AsNoTracking()
            join pv in _context.ProductVariants on t.ProductVariantId equals pv.Id
            join p in _context.Products on pv.ProductId equals p.Id
            join u in _context.AppUsers on t.AssignedUserId equals u.Id into userGrp
            from u in userGrp.DefaultIfEmpty()
            
            where t.Id == request.Id

            select new FulfillmentTaskByIdDto(
                t.Id,
                t.Type,
                pv.Id,
                p.Name,
                p.HasVariants,
                pv.Attributes.Select(a => new VariantAttributeDto(a.Name, a.Value)).ToList(),
                t.RequestedQuantity,
                t.LinkedOrderItemId,
                null,
                t.Cost,
                t.AssignedUserId,
                u != null ? u.FirstName : null,
                u != null ? u.LastName : null,
                t.PurchaseOrderNumber,
                t.Notes,
                t.ExpectedCompletionDate,
                t.Priority,
                t.Status,
                t.CreatedOnUtc,
                t.StartedQuantity,
                t.CompletedQuantity,
                t.ScrappedQuantity)
        ).FirstOrDefaultAsync(cancellationToken);

        if (result is null)
            return Result.Failure<FulfillmentTaskByIdDto>(Error.NotFound("Task.NotFound", "Fulfillment task not found."));

        return result;
    }
}
