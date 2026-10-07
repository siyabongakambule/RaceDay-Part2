namespace RaceDay.Constants;

// Central place for role name strings so controllers never hard-code them.
// These values must match the RoleName rows seeded into the Roles table.
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Organiser = "Organiser";
    public const string Participant = "Participant";
}
