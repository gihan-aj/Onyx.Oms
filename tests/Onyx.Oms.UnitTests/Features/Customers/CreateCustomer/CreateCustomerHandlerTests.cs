using FluentAssertions;
using NSubstitute;
using Onyx.Oms.Core.Common.Interfaces;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.UnitTests.Common.Mocks;
using Onyx.Oms.Web.Features.Customers.CreateCustomer;
using Xunit;

namespace Onyx.Oms.UnitTests.Features.Customers.CreateCustomer;

public class CreateCustomerHandlerTests
{
    private readonly IApplicationDbContext _context;
    private readonly CreateCustomerHandler _handler;

    public CreateCustomerHandlerTests()
    {
        _context = Substitute.For<IApplicationDbContext>();
        _handler = new CreateCustomerHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCustomerIsValid()
    {
        // Arrange
        var command = new CreateCustomerCommand(
            "John Doe", "john@example.com", "123456", null, "Street", "City", "State", "Zip", "Country", "Notes");
        
        var dbSet = MockDbSet.Create<Customer>();
        _context.Customers.Returns(dbSet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEmailIsTaken()
    {
        // Arrange
        var email = "john@example.com";
        var existingCustomer = Customer.Create("Existing", email, "000000", null, null, null).Value;
        
        var command = new CreateCustomerCommand(
            "New User", email, "123456", null, "Street", "City", "State", "Zip", "Country", "Notes");

        var dbSet = MockDbSet.Create(existingCustomer);
        _context.Customers.Returns(dbSet);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.EmailNotUnique");
        await _context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
