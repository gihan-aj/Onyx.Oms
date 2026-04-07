using FluentAssertions;
using NSubstitute;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.ValueObjects;
using Onyx.Oms.UnitTests.Common.Mocks;
using Onyx.Oms.Web.Features.Customers.GetCustomerById;
using Xunit;

namespace Onyx.Oms.UnitTests.Features.Customers.GetCustomerById;

public class GetCustomerByIdHandlerTests
{
    private readonly IApplicationDbContext _context;
    private readonly GetCustomerByIdHandler _handler;

    public GetCustomerByIdHandlerTests()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _handler = new GetCustomerByIdHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnCustomer_WhenExists()
    {
        // Arrange
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var customerId = Guid.NewGuid();
        var address = new Address("Street", "City", "District", "State", "Zip", "Country");
        var customer = Customer.Create(tenantId, "Name", "email@test.com", "123", null, address, "Notes").Value;
        
        // Reflection to set ID since it's private set in real usage usually, 
        // but our Entity base allows init or has private setter. 
        // For testing purposes with EF Core, we rely on EF to set it, but for Unit Tests we might need to set it via reflection or constructor if visible.
        // In our Entity class, Id is public set or init? Let's assume we can set it for test or it's generated in Create.
        // Actually Customer.Create generates a new GUID. We need to mock the DbSet to return *this* customer when queried by ID.
        
        // Wait, standard MockDbSet doesn't support 'Find' or complex queries easily with NSubstitute without the Async wrapper we built.
        // But FirstOrDefaultAsync uses the IQueryable provider.
        // We need to ensure the customer in the mock set has the ID we are looking for.
        // Since `Customer.Create` assigns a random GUID, we need to capture that ID.
        
        var query = new GetCustomerByIdQuery(customer.Id);
        
        var dbSet = MockDbSet.Create(customer);
        _context.Customers.Returns(dbSet);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(customer.Id);
        result.Value.Name.Should().Be("Name");
        result.Value.CreatedDate.Should().Be(customer.CreatedOnUtc);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenDoesNotExist()
    {
        // Arrange
        var dbSet = MockDbSet.Create<Customer>(); // Empty
        _context.Customers.Returns(dbSet);
        
        var query = new GetCustomerByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.NotFound");
    }
}
