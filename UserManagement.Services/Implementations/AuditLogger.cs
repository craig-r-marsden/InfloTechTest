using System;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;

namespace UserManagement.Services.Domain.Implementations;

public class AuditLogger : IAuditLogger
{
    private readonly IDataContext _dataAccess;
    public AuditLogger(IDataContext dataAccess) => _dataAccess = dataAccess;

    /// <summary>
    /// Creates an entry in the audit log using the supplied values.
    /// </summary>
    /// <param name="action">The action that was performed (i.e. Create, Edit, Delete)</param>
    /// <param name="userId">The ID of the user that data was changed for</param>
    /// <param name="details">Json string containing changed values</param>
    /// <param name="user">Model of user that is being modified</param>
    public void Log(string action, long userId, string details, User user)
    {
        var log = new UserAuditLog
        {
            Action = action,
            UserId = userId,
            Details = details,
            Timestamp = DateTime.UtcNow,
            User = $"{user.Forename} {user.Surname}"
        };

        _dataAccess.Create<UserAuditLog>(log);
    }
}
