using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Common.Models;
using Onyx.Oms.Core.Domain.Models;
using Onyx.Oms.Infrastructure.Persistence;
using Onyx.Oms.Infrastructure.Persistence.Entities;

namespace Onyx.Oms.Infrastructure.Services;

public class AppSequenceService : IAppSequenceService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public AppSequenceService(AppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<string>> GetNextNumberAsync(string prefix, CancellationToken cancellationToken = default)
    {       
        int maxRetries = 3;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var sequence = await _dbContext.AppSequences
                    .FirstOrDefaultAsync(s => s.Prefix == prefix, cancellationToken);

                if (sequence == null)
                {
                    sequence = new AppSequence
                    {
                        Id = Guid.NewGuid(),
                        TenantId = _currentUserService.ActiveTenantId,
                        Prefix = prefix,
                        CurrentValue = 0
                    };

                    _dbContext.AppSequences.Add(sequence);
                }

                sequence.CurrentValue++;
                await _dbContext.SaveChangesAsync(cancellationToken);

                return $"{prefix}-{sequence.CurrentValue:D6}";
            }
            catch(DbUpdateConcurrencyException ex)
            {
                if (attempt == maxRetries - 1)
                {
                    //throw new Exception($"System is currently experiencing high traffic. Could not generate sequence for {prefix}. Please try again.");
                    return Result.Failure<string>(Error.Failure("AppSequence.Concurrency", $"System is currently experiencing high traffic. Could not generate sequence for {prefix}. Please try again."));
                }
                foreach (var entry in ex.Entries)
                {
                    if (entry.Entity is AppSequence)
                    {
                        // This updates the local 'sequence' variable with the newly incremented value
                        await entry.ReloadAsync(cancellationToken);
                    }
                }
            }
        }

        return Result.Failure<string>(Error.Failure("AppSequence.Concurrency", "Sequence generation failed."));
    }

    public async Task<long?> GetCurrentValueAsync(string prefix, CancellationToken ct = default)
    {
        var sequence = await _dbContext.AppSequences
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Prefix == prefix, ct);

        return sequence?.CurrentValue;
    }

    public async Task<List<AppSequence>> GetCurrentValuesAsync(CancellationToken ct = default)
    {
        var sequence = await _dbContext.AppSequences
            .AsNoTracking()
            .ToListAsync(ct);

        return sequence;
    }

    public async Task<Result> UpdateCurrentValueAsync(string prefix, long newValue, CancellationToken ct = default)
    {
        var sequence = await _dbContext.AppSequences.FirstOrDefaultAsync(s => s.Prefix == prefix, ct);
        
        if (sequence == null)
        {
            sequence = new AppSequence 
            { 
                Id = Guid.NewGuid(), 
                TenantId = _currentUserService.ActiveTenantId, 
                Prefix = prefix, 
                CurrentValue = newValue 
            };
            _dbContext.AppSequences.Add(sequence);
        }
        else
        {
            sequence.CurrentValue = newValue;
        }

        await _dbContext.SaveChangesAsync(ct);
        
        return Result.Success();
    }
}
