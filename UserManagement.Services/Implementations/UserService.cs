using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using static UserManagement.Services.Domain.Interfaces.IAuditLogger;

namespace UserManagement.Services.Domain.Implementations;

public class UserService : IUserService
{
    private readonly IDataContext _dataAccess;
    private readonly IAuditLogger _auditLogger;

    public UserService(IDataContext dataAccess, IAuditLogger auditLogger)
    {
        _dataAccess = dataAccess;
        _auditLogger = auditLogger;
    }

    /// <summary>
    /// Return users by active state
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    public IEnumerable<User> FilterByActive(bool isActive)
    {
        return GetAll().Where(user => user.IsActive == isActive);
    }

    public IEnumerable<User> GetAll() => _dataAccess.GetAll<User>();

    public IEnumerable<UserAuditLog> GetAllLogs() => _dataAccess.GetAll<UserAuditLog>();

    /// <summary>
    /// Creates a new User DB record using the supplied model and logs an entry in the audit
    /// that holds the initial values for the user.
    /// </summary>
    public void CreateUser(User user)
    {
        _dataAccess.Create(user);

        var initialValues = GetInitialValues(user);

        // Convert initial values to Json for storing in audit log.
        var details = JsonSerializer.Serialize(initialValues, new JsonSerializerOptions { WriteIndented = true });

        _auditLogger.Log(LogActions.Create, user.Id, details, user);
    }

    /// <summary>
    /// Gets a dictionary containing the initial values for the supplied user.
    /// </summary>
    public static Dictionary<string, FieldChange> GetInitialValues(User newUser)
    {
        var initialValues = new Dictionary<string, FieldChange>();

        var props = typeof(User).GetProperties()
            .Where(p => p.CanRead && p.GetGetMethod() != null);

        foreach (var prop in props)
        {
            var newValue = prop.GetValue(newUser);

            initialValues[prop.Name] = new FieldChange
            {
                OldValue = string.Empty,
                NewValue = newValue
            };
        }

        return initialValues;
    }

    /// <summary>
    /// Updates an existing User DB record using the supplied model and logs an entry in the audit
    /// that holds the changed values for the user.
    /// </summary>
    public void UpdateUser(User updatedUser)
    {
        if (_dataAccess.Users == null)
        {
            return;
        }

        var originalUser = _dataAccess.Users.AsNoTracking()
                .FirstOrDefault(u => u.Id == updatedUser.Id);

        if (originalUser != null)
        {
            _dataAccess.Update(updatedUser);

            var changes = GetChangedValues(originalUser, updatedUser);

            if (changes.Any())
            {
                // Convert changed values to Json for storing in audit log.
                var details = JsonSerializer.Serialize(changes, new JsonSerializerOptions { WriteIndented = true });

                _auditLogger.Log(LogActions.Update, updatedUser.Id, details, updatedUser);
            }
        }
    }

    /// <summary>
    /// Gets a dictionary containing the changed values for the supplied user.
    /// </summary>
    public static Dictionary<string, FieldChange> GetChangedValues(User oldUser, User newUser)
    {
        var changes = new Dictionary<string, FieldChange>();

        if (oldUser == null || newUser == null)
        {
            return changes;
        }

        var props = typeof(User).GetProperties()
            .Where(p => p.CanRead && p.GetGetMethod() != null);

        foreach (var prop in props)
        {
            var oldValue = prop.GetValue(oldUser);
            var newValue = prop.GetValue(newUser);

            if (!Equals(oldValue, newValue))
            {
                changes[prop.Name] = new FieldChange
                {
                    OldValue = oldValue,
                    NewValue = newValue
                };
            }
        }

        return changes;
    }

    /// <summary>
    /// Deletes an existing User DB record for the supplied model and logs an entry in the audit
    /// that holds the final values for the user.
    /// </summary>
    public void DeleteUser(User user)
    {
        _dataAccess.Delete(user);

        var finalValues = GetFinalValues(user);

        // Convert final values to Json for storing in audit log.
        var details = JsonSerializer.Serialize(finalValues, new JsonSerializerOptions { WriteIndented = true });

        _auditLogger.Log(LogActions.Delete, user.Id, details, user);
    }

    /// <summary>
    /// Gets a dictionary containing the final values for the supplied user.
    /// </summary>
    public static Dictionary<string, FieldChange> GetFinalValues(User deletedUser)
    {
        var finalValues = new Dictionary<string, FieldChange>();

        var props = typeof(User).GetProperties()
            .Where(p => p.CanRead && p.GetGetMethod() != null);

        foreach (var prop in props)
        {
            var oldValue = prop.GetValue(deletedUser);

            finalValues[prop.Name] = new FieldChange
            {
                OldValue = oldValue,
                NewValue = string.Empty
            };
        }

        return finalValues;
    }

    public User? GetUserByID(long id)
    {
        return _dataAccess.GetEntityByID<User>(id);
    }

    // /// <summary>
    // /// Creates an entry in the audit log using the supplied values.
    // /// </summary>
    // /// <param name="action">The action that was performed (i.e. Create, Edit, Delete)</param>
    // /// <param name="userId">The ID of the user that data was changed for</param>
    // /// <param name="details">Json string containing changed values</param>
    // /// <param name="user">Model of user that is being modified</param>
    // public void Log(string action, long userId, string details, User user)
    // {
    //     var log = new UserAuditLog
    //     {
    //         Action = action,
    //         UserId = userId,
    //         Details = details,
    //         Timestamp = DateTime.UtcNow,
    //         User = $"{user.Forename} {user.Surname}"
    //     };

    //     _dataAccess.Create<UserAuditLog>(log);
    // }

    public UserAuditLog? GetUserLogByID(long id)
    {
        return _dataAccess.GetEntityByID<UserAuditLog>(id);
    }
    
    public List<UserAuditLog> GetUserLogsByUserID(long userID)
    {
        return _dataAccess.UserAuditLogs
            .Where(log => log.UserId == userID)
            .ToList();
    }
}
