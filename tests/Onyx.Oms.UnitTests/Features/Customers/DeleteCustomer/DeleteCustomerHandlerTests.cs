using FluentAssertions;
using NSubstitute;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.UnitTests.Common.Mocks;
using Onyx.Oms.Web.Features.Customers.DeleteCustomer;
using Xunit;

namespace Onyx.Oms.UnitTests.Features.Customers.DeleteCustomer;

public class DeleteCustomerHandlerTests
{
    private readonly IApplicationDbContext _context;
    private readonly DeleteCustomerHandler _handler;

    public DeleteCustomerHandlerTests()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _handler = new DeleteCustomerHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCustomerExists()
    {
        // Arrange
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var customer = Customer.Create(tenantId, "Name", "e@e.com", "123", null, null, null).Value;
        var command = new DeleteCustomerCommand(customer.Id);

        var dbSet = MockDbSet.Create(customer);
        _context.Customers.Returns(dbSet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _context.Customers.Received(1).Remove(customer);
        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var dbSet = MockDbSet.Create<Customer>();
        _context.Customers.Returns(dbSet);
        
        var command = new DeleteCustomerCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.NotFound");
    }
}
