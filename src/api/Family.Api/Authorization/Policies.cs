namespace Family.Api.Authorization;

/// <summary>
/// Authorization policy constants
/// </summary>
public static class Policies
{
    /// <summary>
    /// Policy for family users
    /// </summary>
    public const string FamilyUser = "FamilyUser";

    /// <summary>
    /// Policy for family administrators
    /// </summary>
    public const string FamilyAdmin = "FamilyAdmin";
}