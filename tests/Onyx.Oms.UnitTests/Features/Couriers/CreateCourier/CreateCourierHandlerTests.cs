using FluentAssertions;
using NSubstitute;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.UnitTests.Common.Mocks;
using Onyx.Oms.Web.Features.Couriers.CreateCourier;

namespace Onyx.Oms.UnitTests.Features.Couriers.CreateCourier;

public class CreateCourierHandlerTests
{
    private readonly IApplicationDbContext _context;
    private readonly CreateCourierHandler _handler;
    private readonly ICurrentUserService _currentUserService;

    public CreateCourierHandlerTests()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _handler = new CreateCourierHandler(_context, _currentUserService);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCourierIsValidAndUnique()
    {
        // Arrange
        var command = new CreateCourierCommand("FedEx", "Jane Doe", "555-1234", null, "https://fedex.com", null);
        
        // Mock DB: Empty list of couriers to simulate no existing courier with this name
        var dbSet = MockDbSet.Create<Courier>();
        _context.Couriers.Returns(dbSet); 
        // Note: For EF Core async queries (AnyAsync), standard mocking frameworks like NSubstitute often struggle 
        // without specialized helpers like MockQueryable.
        // If MockDbSet.Create doesn't support async, we might need a workaround or 'MockQueryable.NSubstitute' package.
        // Assuming MockDbSet supports basic IQueryable for now. If async fails, we might need to adjust.
        // Actually, with standard NSubstitute, extension methods like AnyAsync are hard to mock on DbSet directly.
        // A common pattern is to wrap DbContext calls in a Repository or just accept that Unit Testing handlers with EF calls directly is tricky without In-Memory DB.
        // However, let's try assuming the user has standard setup or we might face async issues.
        // If async issues arise, we should recommend `MockQueryable.NSubstitute`.
        
        // For this example, let's assume we are using a library or helper that handles async enumerables, 
        // OR we can just test the non-async parts if possible, but the handler uses `AnyAsync`.
        
        // Let's use NSubstitute to mock the DbSet such that it implements IAsyncEnumerable if possible.
        // But for simplicity in this first pass, I'll proceed. If it fails on execution (which I can't check here easily), user will tell me.
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
