using System.ComponentModel;

namespace AuthTools.Constants;

public static class RoleNames
{
    public const string Admin = "admin";
    public const string Manager = "manager";
    public const string User = "user";
}

public enum UserRole
{
    [Description("admin")] Admin,
    [Description("manager")] Manager,
    [Description("user")] User
}