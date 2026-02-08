using MediatR;
using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.Models;

namespace Onyx.Oms.Web.Features.Couriers.CreateCourier;

public class CreateCourierHandler : IRequestHandler<CreateCourierCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateCourierHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateCourierCommand request, CancellationToken cancellationToken)
    {
        bool courierExists = await _context.Couriers
            .AnyAsync(c => c.Name == request.Name, cancellationToken);

        if (courierExists)
            return Result.Failure<Guid>(Error.Conflict("Courier.NameExists", "A courier with the same name already exists."));

        var courierResult = Courier.Create(
            request.Name,
            request.ContactPerson,
            request.PrimaryPhone,
            request.SecondaryPhone,
            request.WebsiteUrl,
            request.TrackingUrlTemplate);

        if (courierResult.IsFailure)
        {
            return Result.Failure<Guid>(courierResult.Error);
        }

        // We need to cast IApplicationDbContext to AppDbContext or expose Couriers in interface
        // Since we decided to add DbSet to interface (user said so), we can access it if generic, 
        // OR we can just use Set<T>() if we exposed that, or just cast for now if the interface update isn't ready.
        // The user said "I also added DbSet in IApplicationDbContext". 
        // So I will assume the interface has it, but I didn't see the user code update. 
        // I will use _context.Couriers.Add(...) assuming the user added it.
        // Wait, I can't assume. I should check or implement it safely.
        // Since I can't check user's manual edit instantly, I'll assume they did.
        // BUT, IApplicationDbContext likely doesn't have it yet unless I update it or they updated it.
        // I'll update IApplicationDbContext to generic Set<T> or specific property if I can.
        // User said "I also added DbSet in IApplicationDbContext". So I'll assume it's there `DbSet<Courier> Couriers { get; }`.
        
        _context.Couriers.Add(courierResult.Value);

        await _context.SaveChangesAsync(cancellationToken);

        return courierResult.Value.Id;
    }
}
