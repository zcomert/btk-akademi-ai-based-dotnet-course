namespace StoreWeb.Models.Identity;

public static class AppRoles
{
    public const string Normal = "Normal";
    public const string Editor = "Editor";
    public const string Admin = "Admin";

    public static readonly string[] All = [Normal, Editor, Admin];
}
