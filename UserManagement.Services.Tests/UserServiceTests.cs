using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;
using UserManagement.Services.Domain.Interfaces;
using static UserManagement.Services.Domain.Interfaces.IAuditLogger;

namespace UserManagement.Data.Tests;

public class UserServiceTests
{
    private readonly Mock<IDataContext> _dataContext = new();
    private readonly Mock<IAuditLogger> _auditLogger = new();
    private UserService CreateService() => new(_dataContext.Object, _auditLogger.Object);

    [Fact]
    public void GetAll_WhenContextReturnsEntities_MustReturnSameEntities()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateService();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = service.GetAll();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().BeSameAs(users);
    }

    private IQueryable<User> SetupUsers(
        string forename = "Johnny",
        string surname = "User",
        string email = "juser@example.com",
        bool isActive = true,
        string dateOfBirth = "1984-05-23")
    {
        var users = new[]
        {
            new User
            {
                Forename = forename,
                Surname = surname,
                Email = email,
                IsActive = isActive,
                DateOfBirth = DateOnly.Parse(dateOfBirth)
            }
        }.AsQueryable();

        _dataContext
            .Setup(s => s.GetAll<User>())
            .Returns(users);

        return users;
    }

    [Fact]
    public void GetInitialValues_ShouldReturnAllPropertiesWithCorrectValues()
    {
        // Arrange
        var dateOfBirth = new DateOnly(1999, 2, 8);

        var user = new User
        {
            Id = 3,
            Forename = "John",
            Surname = "Smith",
            Email = "john@example.com",
            DateOfBirth = dateOfBirth,
            IsActive = true
        };

        // Act
        var result = UserService.GetInitialValues(user);

        // Assert
        result.Should().NotBeNull();
        result[nameof(User.Id)].OldValue.Should().Be(string.Empty);
        result[nameof(User.Id)].NewValue.Should().Be(3);
        result[nameof(User.Forename)].OldValue.Should().Be(string.Empty);
        result[nameof(User.Forename)].NewValue.Should().Be("John");
        result[nameof(User.Surname)].OldValue.Should().Be(string.Empty);
        result[nameof(User.Surname)].NewValue.Should().Be("Smith");
        result[nameof(User.Email)].OldValue.Should().Be(string.Empty);
        result[nameof(User.Email)].NewValue.Should().Be("john@example.com");
        result[nameof(User.DateOfBirth)].OldValue.Should().Be(string.Empty);
        result[nameof(User.DateOfBirth)].NewValue.Should().Be(dateOfBirth);
        result[nameof(User.IsActive)].OldValue.Should().Be(string.Empty);
        result[nameof(User.IsActive)].NewValue.Should().Be(true);
    }

    [Fact]
    public void CreateUser_ShouldCallDataAccessCreateAndLogWithSerializedInitialValues()
    {
        // Arrange
        var userService = CreateService();
        var dateOfBirth = new DateOnly(1999, 2, 8);

        var user = new User
        {
            Id = 3,
            Forename = "John",
            Surname = "Smith",
            Email = "john@example.com",
            DateOfBirth = dateOfBirth,
            IsActive = true
        };

        string capturedJson = string.Empty;

        _auditLogger.Setup(l => l.Log(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<User>()))
                   .Callback<string, long, string, User>((action, id, details, u) => capturedJson = details);

        // Act
        userService.CreateUser(user);

        // Assert
        _dataContext.Verify(d => d.Create(user), Times.Once);
        _auditLogger.Verify(l => l.Log(LogActions.Create, user.Id, It.IsAny<string>(), user), Times.Once);

        // Deserialize and check the initial values
        var dict = JsonSerializer.Deserialize<Dictionary<string, FieldChange>>(capturedJson);
        dict.Should().ContainKey(nameof(User.Id));
        NormalizeJsonValue(dict[nameof(User.Id)].NewValue).Should().Be(3);
        NormalizeJsonValue(dict[nameof(User.Forename)].NewValue).Should().Be("John");
        NormalizeJsonValue(dict[nameof(User.Surname)].NewValue).Should().Be("Smith");
        NormalizeJsonValue(dict[nameof(User.Email)].NewValue).Should().Be("john@example.com");
        //NormalizeJsonValue(dict[nameof(User.DateOfBirth)].NewValue).Should().Be(dateOfBirth);
        NormalizeJsonValue(dict[nameof(User.IsActive)].NewValue).Should().Be(true);
    }

    private static object? NormalizeJsonValue(object? value)
    {
        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.Number => TryGetNumericValue(jsonElement),
                JsonValueKind.String => jsonElement.GetString(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => jsonElement.ToString()
            };
        }

        return value;
    }

    private static object? TryGetNumericValue(JsonElement element)
    {
        if (element.TryGetInt32(out var intValue))
            return intValue;
        if (element.TryGetInt64(out var longValue))
            return longValue;
        if (element.TryGetDecimal(out var decValue))
            return decValue;
        if (element.TryGetDouble(out var dblValue))
            return dblValue;

        return element.ToString(); // fallback
    }
}
