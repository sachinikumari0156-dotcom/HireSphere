namespace HireSphere.API.Services;

public record StoredFileResult(string StorageKey, string OriginalFileName);

public interface ILocalFileStorageService
{
    Task<(bool Ok, string? Error, StoredFileResult? Result)> SaveAsync(
        IFormFile file,
        string category);

    Task<(bool Ok, string? Error, Stream? Content, string? ContentType, string? FileName)> OpenReadAsync(
        string storageKey);

    Task DeleteAsync(string storageKey);
}

public sealed class LocalFileStorageService : ILocalFileStorageService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "image/png",
        "image/jpeg"
    };

    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".bat", ".cmd", ".com", ".msi", ".dll", ".scr", ".ps1", ".sh", ".js", ".vbs", ".jar"
    };

    private readonly string _uploadRoot;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _uploadRoot = Path.Combine(environment.ContentRootPath, "App_Data", "uploads");
        Directory.CreateDirectory(_uploadRoot);
    }

    public async Task<(bool Ok, string? Error, StoredFileResult? Result)> SaveAsync(
        IFormFile file,
        string category)
    {
        if (file == null || file.Length == 0)
        {
            return (false, "File is required.", null);
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return (false, "File exceeds the 5 MB size limit.", null);
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension)
            || !AllowedExtensions.Contains(extension)
            || BlockedExtensions.Contains(extension))
        {
            return (false, "Unsupported file type.", null);
        }

        if (!string.IsNullOrWhiteSpace(file.ContentType)
            && !AllowedContentTypes.Contains(file.ContentType))
        {
            return (false, "Unsupported file content type.", null);
        }

        var safeCategory = SanitizeCategory(category);
        var storageKey = $"{safeCategory}/{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var absolutePath = ResolveSafePath(storageKey);
        if (absolutePath == null)
        {
            return (false, "Invalid storage path.", null);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        await using var input = file.OpenReadStream();
        if (ContainsExecutableSignature(input))
        {
            return (false, "Executable files are not allowed.", null);
        }

        input.Position = 0;
        await using var output = File.Create(absolutePath);
        await input.CopyToAsync(output);

        return (true, null, new StoredFileResult(storageKey, Path.GetFileName(file.FileName)));
    }

    public Task<(bool Ok, string? Error, Stream? Content, string? ContentType, string? FileName)> OpenReadAsync(
        string storageKey)
    {
        var absolutePath = ResolveSafePath(storageKey);
        if (absolutePath == null || !File.Exists(absolutePath))
        {
            return Task.FromResult<(bool, string?, Stream?, string?, string?)>(
                (false, "File not found.", null, null, null));
        }

        var extension = Path.GetExtension(absolutePath);
        var contentType = extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };

        Stream content = File.OpenRead(absolutePath);
        var fileName = Path.GetFileName(absolutePath);
        return Task.FromResult<(bool, string?, Stream?, string?, string?)>(
            (true, null, content, contentType, fileName));
    }

    public Task DeleteAsync(string storageKey)
    {
        var absolutePath = ResolveSafePath(storageKey);
        if (absolutePath != null && File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }

    private string? ResolveSafePath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey)
            || storageKey.Contains("..", StringComparison.Ordinal)
            || Path.IsPathRooted(storageKey))
        {
            return null;
        }

        var normalizedKey = storageKey.Replace('\\', '/').TrimStart('/');
        var absolutePath = Path.GetFullPath(Path.Combine(_uploadRoot, normalizedKey.Replace('/', Path.DirectorySeparatorChar)));

        if (!absolutePath.StartsWith(_uploadRoot, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return absolutePath;
    }

    private static string SanitizeCategory(string category)
    {
        var sanitized = new string(category
            .Where(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_')
            .ToArray());

        return string.IsNullOrWhiteSpace(sanitized) ? "general" : sanitized.ToLowerInvariant();
    }

    private static bool ContainsExecutableSignature(Stream stream)
    {
        Span<byte> header = stackalloc byte[2];
        if (stream.Read(header) < 2)
        {
            return false;
        }

        // MZ header for Windows executables
        return header[0] == 0x4D && header[1] == 0x5A;
    }
}
