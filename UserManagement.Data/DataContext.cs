using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UserManagement.Models;

namespace UserManagement.Data;

public class DataContext : DbContext, IDataContext
{
    public DataContext() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseInMemoryDatabase("UserManagement.Data.DataContext");

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<User>().HasData(new[]
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
        });
        
        model.Entity<UserAuditLog>().HasData(new[]
        {
            new UserAuditLog { Id = 1, UserId = 3, User = "Caster Troys", Action = "Create", Timestamp = DateTime.Parse("2025-10-05 13:18:39"),
                Details = """
                    {
                        "Forename": {
                            "OldValue": "",
                            "NewValue": "Caster"
                        },
                        "Surname": {
                            "OldValue": "",
                            "NewValue": "Troys"
                        },
                        "Email": {
                            "OldValue": "",
                            "NewValue": "ctroy@example.co"
                        },
                        "IsActive": {
                            "OldValue": "",
                            "NewValue": false
                        },
                        "DateOfBirth": {
                            "OldValue": "",
                            "NewValue": "1964-07-22"
                        }
                    }
                    """ },
            new UserAuditLog { Id = 2, UserId = 3, User = "Castor Troy", Action = "Update", Timestamp = DateTime.Parse("2025-10-06 09:12:54"),
                Details = """
                    {
                        "Forename": {
                            "OldValue": "Caster",
                            "NewValue": "Castor"
                        },
                        "Surname": {
                            "OldValue": "Troys",
                            "NewValue": "Troy"
                        },
                        "Email": {
                            "OldValue": "ctroy@example.co",
                            "NewValue": "ctroy@example.com"
                        },
                        "IsActive": {
                            "OldValue": false,
                            "NewValue": true
                        },
                        "DateOfBirth": {
                            "OldValue": "1964-07-22",
                            "NewValue": "1964-09-22"
                        }
                    }
                    """ },
            new UserAuditLog { Id = 3, UserId = 25, User = "John Smith", Action = "Delete", Timestamp = DateTime.Parse("2025-10-08 10:45:18"),
                Details = """
                    {
                        "Forename": {
                            "OldValue": "John",
                            "NewValue": ""
                        },
                        "Surname": {
                            "OldValue": "Smith",
                            "NewValue": ""
                        },
                        "Email": {
                            "OldValue": "johns@example.com",
                            "NewValue": ""
                        },
                        "IsActive": {
                            "OldValue": false,
                            "NewValue": ""
                        },
                        "DateOfBirth": {
                            "OldValue": "1964-05-18",
                            "NewValue": ""
                        }
                    }
                    """ },
            new UserAuditLog { Id = 4, UserId = 3, User = "Castor Troy", Action = "Update", Timestamp = DateTime.Parse("2025-10-08 10:41:05"),
                Details = """
                    {
                        "IsActive": {
                            "OldValue": true,
                            "NewValue": false
                        }
                    }
                    """ },
        });
    }

    public DbSet<User>? Users { get; set; }
    public DbSet<UserAuditLog> UserAuditLogs { get; set; }

    public IQueryable<TEntity> GetAll<TEntity>() where TEntity : class
        => base.Set<TEntity>();

    public TEntity? GetEntityByID<TEntity>(long id) where TEntity : class
    {        
        return base.Set<TEntity>().Find(id);
    }

    public void Create<TEntity>(TEntity entity) where TEntity : class
    {
        base.Add(entity);
        SaveChanges();
    }

    public new void Update<TEntity>(TEntity entity) where TEntity : class
    {
        base.Update(entity);
        SaveChanges();
    }

    public void Delete<TEntity>(TEntity entity) where TEntity : class
    {
        base.Remove(entity);
        SaveChanges();
    }
}
