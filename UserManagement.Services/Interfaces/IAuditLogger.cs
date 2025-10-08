using UserManagement.Models;

namespace UserManagement.Services.Domain.Interfaces;

public interface IAuditLogger
{
    public static class LogActions
    {
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Delete = "Delete";
    }
    
    void Log(string action, long userId, string details, User user);
}
