using System.Linq;
using UserManagement.Models;
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

        return View(nameof(List), model);
    }

    [HttpGet]
    public ViewResult ListNonActive()
    {
        var nonActiveUsers = _userService.FilterByActive(false);

        var model = GetModelForListView(nonActiveUsers);

        return View(nameof(List), model);
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
            DateOfBirth = user.DateOfBirth,
            IsActive = user.IsActive
        });

        var model = new UserListViewModel
        {
            Items = items.ToList()
        };

        return model;
    }

    [HttpGet]
    public ViewResult Create()
    {
        return View(new UserFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(UserFormViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new User
            {
                Forename = model.Forename ?? string.Empty,
                Surname = model.Surname ?? string.Empty,
                Email = model.Email ?? string.Empty,
                DateOfBirth = model.DateOfBirth,
                IsActive = model.IsActive
            };

            _userService.CreateUser(user);

            return RedirectToAction(nameof(List));
        }

        return View(model);
    }

    [HttpGet("{id:long}")]
    public IActionResult Details(long id)
    {
        var user = _userService.GetUserByID(id);

        if (user == null)
        {
            return NotFound();
        }

        var model = new UserDetailsViewModel
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            IsActive = user.IsActive
        };

        return View(model);
    }

    [HttpGet("{id:long}")]
    public IActionResult Edit(long id)
    {
        var user = _userService.GetUserByID(id);

        if (user == null)
        {
            return NotFound();
        }

        var model = new UserFormViewModel
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            IsActive = user.IsActive
        };

        return View(model);
    }

    [HttpPost("{id:long}")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(long id, UserFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            var user = new User
            {
                Id = model.Id,
                Forename = model.Forename ?? string.Empty,
                Surname = model.Surname ?? string.Empty,
                Email = model.Email ?? string.Empty,
                DateOfBirth = model.DateOfBirth,
                IsActive = model.IsActive
            };

            _userService.UpdateUser(user);

            return RedirectToAction(nameof(List));
        }

        return View(model);
    }

    [HttpGet("{id:long}")]
    public IActionResult Delete(long id)
    {
        var user = _userService.GetUserByID(id);

        if (user == null)
        {
            return NotFound();
        }

        var model = new UserDetailsViewModel
        {
            Id = user.Id,
            Forename = user.Forename,
            Surname = user.Surname,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            IsActive = user.IsActive
        };

        return View(model);
    }
    
    [HttpPost("{id:long}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(long id)
    {
        var user = _userService.GetUserByID(id);

        if (user == null)
        {
            return NotFound();
        }

        _userService.DeleteUser(user);

        // Use this to display confirmation on List screen after redirect.
        TempData["DeleteSuccessMessage"] = $"User '{user.Forename} {user.Surname} ({user.Email})' was successfully deleted.";

        return RedirectToAction(nameof(List));
    }
}
