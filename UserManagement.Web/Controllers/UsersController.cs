using System.Linq;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;

namespace UserManagement.WebMS.Controllers;

[Route("users/{action=List}")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet]
    public ViewResult List()
    {
        var allUsers = _userService.GetAll();

        var model = GetModelForListView(allUsers);

        return View(model);
    }

    [HttpGet]
    public ViewResult ListActive()
    {
        var activeUsers = _userService.FilterByActive(true);

        var model = GetModelForListView(activeUsers);

        return View("List", model);
    }

    [HttpGet]
    public ViewResult ListNonActive()
    {
        var nonActiveUsers = _userService.FilterByActive(false);

        var model = GetModelForListView(nonActiveUsers);

        return View("List", model);
    }

    /// <summary>
    /// Gets the model for the List view using the supplied collection of User objects.
    /// </summary>
    private UserListViewModel GetModelForListView(IEnumerable<Models.User> users)
    {
        var items = users.Select(user => new UserListItemViewModel
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            Email = user.Email,
            IsActive = user.IsActive
        });

        var model = new UserListViewModel
        {
            Items = items.ToList()
        };

        return model;
    }
}
