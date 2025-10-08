using System;

namespace UserManagement.Web.Models.Users;

public class UserLogsViewModel
{
    public List<UserLogItemViewModel> Items { get; set; } = new();
}

public class UserLogItemViewModel
{
    public long Id { get; set; }
    public string Action { get; set; } = default!;
    public long UserId { get; set; } = default!;
    public DateTime Timestamp { get; set; }
    public string Details { get; set; } = default!;
    public string User { get; set; } = default!;
}
