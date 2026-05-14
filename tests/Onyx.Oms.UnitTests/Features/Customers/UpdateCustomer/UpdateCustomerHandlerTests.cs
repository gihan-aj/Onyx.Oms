using FluentAssertions;
using NSubstitute;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.UnitTests.Common.Mocks;
using Onyx.Oms.Web.Features.Customers.UpdateCustomer;
using Xunit;

namespace Onyx.Oms.UnitTests.Features.Customers.UpdateCustomer;

public class UpdateCustomerHandlerTests
{
    private readonly IApplicationDbContext _context;
    private readonly UpdateCustomerHandler _handler;

    public UpdateCustomerHandlerTests()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _handler = new UpdateCustomerHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUpdateIsValid()
    {
        // Arrange
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var customer = Customer.Create(tenantId, "Old Name", "old@test.com", "111", null, null, null, null).Value;
        var command = new UpdateCustomerCommand(
            customer.Id, "New Name", "new@test.com", "222", "333", "St", "City", "District", "St", "Z", "Co", "Note", null);

        var dbSet = MockDbSet.Create(customer);
        _context.Customers.Returns(dbSet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.Name.Should().Be("New Name");
        customer.Email.Should().Be("new@test.com");
        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var dbSet = MockDbSet.Create<Customer>();
        _context.Customers.Returns(dbSet);
        
        var command = new UpdateCustomerCommand(
            Guid.NewGuid(), "Name", "email", "123", null, null,null, null, null, null, null, null, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnComparison_WhenEmailTakenByAnotherUser()
    {
        // Arrange
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var customer1 = Customer.Create(tenantId, "User 1", "user1@test.com", "111", null, null, null, null).Value;
        var customer2 = Customer.Create(tenantId, "User 2", "user2@test.com", "222", null, null, null, null).Value;
        
        // Try to update User 1 to use User 2's email
        var command = new UpdateCustomerCommand(
            customer1.Id, "User 1", "user2@test.com", "111", null, null, null, null, null, null, null, null, null);

        var dbSet = MockDbSet.Create(customer1, customer2);
        _context.Customers.Returns(dbSet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.EmailNotUnique");
    }
    
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenEmailDidNotChange()
    {
        // Arrange
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var customer = Customer.Create(tenantId, "User 1", "user1@test.com", "111", null, null, null, null).Value;
        
        // Same email, just update name
        var command = new UpdateCustomerCommand(
            customer.Id, "Updated Name", "user1@test.com", "111", null, null, null, null, null, null, null, null, null);

        var dbSet = MockDbSet.Create(customer);
        _context.Customers.Returns(dbSet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        customer.Name.Should().Be("Updated Name");
    }
}
