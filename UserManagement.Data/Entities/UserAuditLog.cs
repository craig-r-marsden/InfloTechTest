using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Models;

public class UserAuditLog
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public string Action { get; set; } = default!;
    public long UserId { get; set; } = default!;
    public DateTime Timestamp { get; set; }
    public string Details { get; set; } = default!;
    public string User { get; set; } = default!;
}

