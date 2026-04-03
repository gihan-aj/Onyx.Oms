using FluentAssertions;
using Onyx.Oms.Core.Domain.Entities;
using Onyx.Oms.Core.Domain.ValueObjects;
using Xunit;

namespace Onyx.Oms.UnitTests.Domain.Customers;

public class CustomerTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenInputsAreValid()
    {
        // Arrange
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var name = "Jane Doe";
        var email = "jane@example.com";
        var phone = "555-0199";
        var address = new Address("123 Main St", "Colombo", "Western", "00100", "Sri Lanka");

        // Act
        var result = Customer.Create(tenantId, name, email, phone, null, address, "Test Notes");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Email.Should().Be(email);
        result.Value.Address.Should().Be(address);
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenNameIsEmpty()
    {
        // Arrange
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Act
        var result = Customer.Create(tenantId, "", "email@test.com", "123", null, null, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.NameRequired");
    }
    
    [Fact]
    public void Create_ShouldReturnFailure_WhenPrimaryPhoneIsEmpty()
    {
        // Arrange
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Act
        var result = Customer.Create(tenantId, "Name", "email@test.com", "", null, null, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Customer.PrimaryPhoneRequired");
    }

    [Fact]
    public void Address_IsEmpty_ShouldReturnTrue_WhenAllFieldsAreEmpty()
    {
        // Arrange
        var address = new Address("", "", "", "", "");

        // Act & Assert
        address.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var customer = Customer.Create(tenantId, "Name", "e@e.com", "123", null, null, null).Value;
        customer.Deactivate();

        // Act
        customer.Activate();

        // Assert
        customer.IsActive.Should().BeTrue();
    }
}
