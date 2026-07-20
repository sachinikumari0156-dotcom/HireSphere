namespace HireSphere.API.Services;

public static class AuthRegistrationRules
{
    public static bool IsPublicRegistrationAllowed(string role) =>
        string.Equals(role, "Candidate", StringComparison.OrdinalIgnoreCase);
}
