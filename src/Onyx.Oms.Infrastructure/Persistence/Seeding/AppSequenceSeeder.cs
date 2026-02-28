using Microsoft.EntityFrameworkCore;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Infrastructure.Persistence.Entities;

namespace Onyx.Oms.Infrastructure.Persistence.Seeding;

public class AppSequenceSeeder
{
    private readonly AppDbContext _context;

    public AppSequenceSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var requiredSequences = new[] { "ORD", "PROD" };

        foreach (var sequenceId in requiredSequences)
        {
            var exists = await _context.AppSequences.AnyAsync(s => s.Id == sequenceId);
            if (!exists)
            {
                var sequence = new AppSequence
                {
                    Id = sequenceId,
                    CurrentValue = 0
                };
                
                _context.AppSequences.Add(sequence);
            }
        }

        await _context.SaveChangesAsync(CancellationToken.None);
    }
}
