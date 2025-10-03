using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;
using UserManagement.WebMS.Controllers;

namespace UserManagement.Data.Tests;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userService = new();

    private UsersController CreateController() => new(_userService.Object);

    [Fact]
    public void List_WhenServiceReturnsAllUsers_ModelMustContainAllUsers()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var allUsers = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.List();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Model
            .Should().BeOfType<UserListViewModel>()
            .Which.Items.Should().BeEquivalentTo(allUsers)
            .And.HaveCount(11);
    }

    [Fact]
    public void ListActive_WhenServiceReturnsActiveUsers_ModelMustContainActiveUsers()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var allUsers = SetupUsers();

        var activeUsers = allUsers
                            .Where(user => user.IsActive)
                            .ToArray();

        _userService
            .Setup(s => s.FilterByActive(true))
            .Returns(activeUsers);

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.ListActive();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Model
            .Should().BeOfType<UserListViewModel>()
            .Which.Items.Should().BeEquivalentTo(activeUsers)
            .And.HaveCount(7);
    }

    [Fact]
    public void ListNonActive_WhenServiceReturnsNonActiveUsers_ModelMustContainNonActiveUsers()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var allUsers = SetupUsers();

        var nonActiveUsers = allUsers
                                .Where(user => !user.IsActive)
                                .ToArray();

        _userService
            .Setup(s => s.FilterByActive(false))
            .Returns(nonActiveUsers);

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.ListNonActive();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Model
            .Should().BeOfType<UserListViewModel>()
            .Which.Items.Should().BeEquivalentTo(nonActiveUsers)
            .And.HaveCount(4);
    }

    private User[] SetupUsers()
    {
        var allUsers = new[]
        {
            new User { Id = 1, Forename = "Peter", Surname = "Loew", Email = "ploew@example.com", IsActive = true, DateOfBirth = new DateOnly(1985, 6, 12) },
            new User { Id = 2, Forename = "Benjamin Franklin", Surname = "Gates", Email = "bfgates@example.com", IsActive = true, DateOfBirth = new DateOnly(1950, 5, 8) },
            new User { Id = 3, Forename = "Castor", Surname = "Troy", Email = "ctroy@example.com", IsActive = false, DateOfBirth = new DateOnly(1964, 9, 22) },
            new User { Id = 4, Forename = "Memphis", Surname = "Raines", Email = "mraines@example.com", IsActive = true, DateOfBirth = new DateOnly(1967, 11, 3) },
            new User { Id = 5, Forename = "Stanley", Surname = "Goodspeed", Email = "sgodspeed@example.com", IsActive = true, DateOfBirth = new DateOnly(1992, 3, 7) },
            new User { Id = 6, Forename = "H.I.", Surname = "McDunnough", Email = "himcdunnough@example.com", IsActive = true, DateOfBirth = new DateOnly(2004, 8, 25) },
            new User { Id = 7, Forename = "Cameron", Surname = "Poe", Email = "cpoe@example.com", IsActive = false, DateOfBirth = new DateOnly(1978, 6, 18) },
            new User { Id = 8, Forename = "Edward", Surname = "Malus", Email = "emalus@example.com", IsActive = false, DateOfBirth = new DateOnly(2009, 12, 30) },
            new User { Id = 9, Forename = "Damon", Surname = "Macready", Email = "dmacready@example.com", IsActive = false, DateOfBirth = new DateOnly(1983, 4, 19) },
            new User { Id = 10, Forename = "Johnny", Surname = "Blaze", Email = "jblaze@example.com", IsActive = true, DateOfBirth = new DateOnly(1977, 10, 15) },
            new User { Id = 11, Forename = "Robin", Surname = "Feld", Email = "rfeld@example.com", IsActive = true, DateOfBirth = new DateOnly(1999, 2, 8) },
        };

        _userService
            .Setup(s => s.GetAll())
            .Returns(allUsers);

        return allUsers;
    }

    [Fact]
    public void Create_Get_ReturnsCreateView()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.Create();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.ViewName.Should().BeNull(); // Means the default view for action is being used (i.e. "Create")

        result.Model
            .Should().BeOfType<UserFormViewModel>();
    }

    [Fact]
    public void Create_Post_ModelIsValid_RedirectsToList()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();

        var model = new UserFormViewModel
        {
            Id = 1,
            Forename = "Peter",
            Surname = "Loew",
            Email = "ploew@example.com",
            DateOfBirth = new DateOnly(1985, 6, 12),
            IsActive = true
        };

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.Create(model);

        // Assert: Verifies that the action of the method under test behaves as expected.
        controller.ModelState.IsValid.Should().BeTrue();

        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName
            .Should().Be(nameof(UsersController.List));
    }

    [Fact]
    public void Create_Post_ModelIsNotValid_ReturnsSameViewAndModel()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();

        var model = new UserFormViewModel
        {
            Id = 1,
            Forename = "Peter",
            Surname = "Loew",
            Email = "",
            DateOfBirth = new DateOnly(1985, 6, 12),
            IsActive = true
        };

        // Add an error to simulate invalid model.
        controller.ModelState.AddModelError("Email", "Email is required");

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.Create(model);

        // Assert: Verifies that the action of the method under test behaves as expected.
        controller.ModelState.IsValid.Should().BeFalse();

        result.Should().BeOfType<ViewResult>();

        var resultAsViewResult = (ViewResult)result;
        resultAsViewResult.ViewName
            .Should().BeNull(); // Means the default view for action is being used (i.e. "Create")
        resultAsViewResult.Model
            .Should().BeOfType<UserFormViewModel>()
            .And.BeEquivalentTo(model);
    }
}
