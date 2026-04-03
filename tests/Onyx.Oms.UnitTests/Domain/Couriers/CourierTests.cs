using FluentAssertions;
using Onyx.Oms.Core.Domain.Entities;

namespace Onyx.Oms.UnitTests.Domain.Couriers;

public class CourierTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenInputsAreValid()
    {
        // Arrange
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var name = "DHL";
        var contactPerson = "John Doe";
        var primaryPhone = "1234567890";
        var secondaryPhone = "0987654321";
        var websiteUrl = "https://dhl.com";
        var trackingUrlTemplate = "https://dhl.com/track/{0}";

        // Act
        var result = Courier.Create(tenantId, name, contactPerson, primaryPhone, secondaryPhone, websiteUrl, trackingUrlTemplate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.IsActive.Should().BeTrue(); // Default is active
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenNameIsEmpty()
    {
        // Arrange
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // Act
        var result = Courier.Create(tenantId, "", "John Doe", "123", null, null, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Courier.NameEmpty");
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateProperties()
    {
        // Arrange
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var courier = Courier.Create(tenantId, "Old Name", "Old Contact", "111", null, null, null).Value;
        var newName = "New Name";

        // Act
        courier.UpdateDetails(newName, "New Contact", "222", "333", "http://new.com", "http://track/{0}");

        // Assert
        courier.Name.Should().Be(newName);
        courier.SecondaryPhone.Should().Be("333");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var courier = Courier.Create(tenantId, "Test", "Test", "123", null, null, null).Value;

        // Act
        courier.Deactivate();

        // Assert
        courier.IsActive.Should().BeFalse();
    }
    
    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var courier = Courier.Create(tenantId, "Test", "Test", "123", null, null, null).Value;
        courier.Deactivate(); // Start as inactive

        // Act
        courier.Activate();

        // Assert
        courier.IsActive.Should().BeTrue();
    }
}
