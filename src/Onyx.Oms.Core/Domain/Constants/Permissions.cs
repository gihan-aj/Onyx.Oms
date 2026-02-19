namespace Onyx.Oms.Core.Domain.Constants;

public static class Permissions
{
    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Create = "Permissions.Users.Create";
        public const string Edit = "Permissions.Users.Edit";
        public const string Delete = "Permissions.Users.Delete";
    }

    public static class Roles
    {
        public const string View = "Permissions.Roles.View";
        public const string Create = "Permissions.Roles.Create";
        public const string Edit = "Permissions.Roles.Edit";
        public const string Activate = "Permissions.Roles.Activate";
        public const string Deactivate = "Permissions.Roles.Deactivate";
        public const string Delete = "Permissions.Roles.Delete";
    }

    public static class Products
    {
        public const string View = "Permissions.Products.View";
        public const string Create = "Permissions.Products.Create";
        public const string Edit = "Permissions.Products.Edit";
        public const string Delete = "Permissions.Products.Delete";
    }

    public static class Couriers
    {
        public const string View = "Permissions.Couriers.View";
        public const string Create = "Permissions.Couriers.Create";
        public const string Edit = "Permissions.Couriers.Edit";
        public const string Activate = "Permissions.Couriers.Activate";
        public const string Deactivate = "Permissions.Couriers.Deactivate";
        public const string Delete = "Permissions.Couriers.Delete";
    }

    public static List<string> GetAllPermissions()
    {
        var permissions = new List<string>();
        foreach (var nestedType in typeof(Permissions).GetNestedTypes())
        {
            foreach (var field in nestedType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy))
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                {
                    if (field.GetValue(null) is string value)
                    {
                        permissions.Add(value);
                    }
                }
            }
        }
        return permissions;
    }
}
