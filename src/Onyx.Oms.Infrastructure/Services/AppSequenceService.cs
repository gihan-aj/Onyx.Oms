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

    public AppSequenceService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GetNextNumberAsync(string sequenceId, string prefix, CancellationToken ct = default)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
        try
        {
            var currentValNullable = await _dbContext.Database.SqlQueryRaw<long?>(
                "SELECT CAST(CurrentValue AS bigint) AS Value FROM AppSequences WITH (UPDLOCK) WHERE Id = {0}", sequenceId)
                .SingleOrDefaultAsync(ct);

            long nextValue;
            if (currentValNullable == null)
            {
                nextValue = 1;
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "INSERT INTO AppSequences (Id, CurrentValue) VALUES ({0}, {1})", 
                    new object[] { sequenceId, nextValue }, ct);
            }
            else
            {
                nextValue = currentValNullable.Value + 1;
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "UPDATE AppSequences SET CurrentValue = {0} WHERE Id = {1}", 
                    new object[] { nextValue, sequenceId }, ct);
            }

            await transaction.CommitAsync(ct);

            return $"{prefix}-{nextValue:D4}";
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<long?> GetCurrentValueAsync(string sequenceId, CancellationToken ct = default)
    {
        var sequence = await _dbContext.AppSequences
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sequenceId, ct);

        return sequence?.CurrentValue;
    }

    public async Task<Result> UpdateCurrentValueAsync(string sequenceId, long newValue, CancellationToken ct = default)
    {
        var sequence = await _dbContext.AppSequences.FirstOrDefaultAsync(s => s.Id == sequenceId, ct);
        
        if (sequence == null)
        {
            sequence = new AppSequence { Id = sequenceId, CurrentValue = newValue };
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
