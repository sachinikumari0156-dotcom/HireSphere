namespace HireSphere.API.Services;

public interface IPasswordService
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);

    bool MeetsPolicy(string password, out string error);
}

public sealed class PasswordService : IPasswordService
{
    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string passwordHash) =>
        BCrypt.Net.BCrypt.Verify(password, passwordHash);

    public bool MeetsPolicy(string password, out string error)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            error = "Password must be at least 8 characters.";
            return false;
        }

        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
        {
            error = "Password must include at least one letter and one digit.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
