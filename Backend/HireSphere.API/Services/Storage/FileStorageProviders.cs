using System.Security.Cryptography;
using System.Text.RegularExpressions;
using HireSphere.API.Data;
using HireSphere.API.Models;
using HireSphere.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HireSphere.API.Services.Storage;

public sealed class StorageProviderStatusDto
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "NotConfigured";
    public string? Detail { get; set; }
    public DateTime? LastCheckedUtc { get; set; }
    public int? QuarantinedDocumentCount { get; set; }
}

public sealed class StoredObjectResult
{
    public string StorageKey { get; set; } = string.Empty;
    public string SanitizedFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string ChecksumSha256 { get; set; } = string.Empty;
    public string ValidationStatus { get; set; } = "Clean";
    public string ScanStatus { get; set; } = "NotConfigured";
    public string Provider { get; set; } = "LocalDevelopment";
}

public interface IFileStorageProvider
{
    string Name { get; }
    StorageProviderStatusDto GetStatus();
    Task<(bool Ok, string? Error, StoredObjectResult? Result)> SaveAsync(
        Stream content,
        string originalFileName,
        string? contentType,
        string logicalCategory,
        int? organizationId,
        int candidateId,
        CancellationToken ct = default);
    Task<(bool Ok, string? Error, Stream? Content, string? ContentType)> OpenReadAsync(
        string storageKey,
        CancellationToken ct = default);
    Task DeleteAsync(string storageKey, CancellationToken ct = default);
}

public interface IAntivirusScanner
{
    StorageProviderStatusDto GetStatus();
}

public sealed class NotConfiguredAntivirusScanner : IAntivirusScanner
{
    public StorageProviderStatusDto GetStatus() => new()
    {
        Name = "Antivirus",
        Status = "NotConfigured",
        Detail = "No real antivirus scanner is configured. File-type and signature validation still apply."
    };
}

public static class FileUploadValidator
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".png", ".jpg", ".jpeg"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "image/png",
        "image/jpeg"
    };

    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".bat", ".cmd", ".com", ".msi", ".dll", ".scr", ".ps1", ".sh", ".js", ".vbs", ".jar",
        ".html", ".htm", ".svg", ".docm", ".xlsm", ".pptm"
    };

    public static string SanitizeDisplayName(string? fileName)
    {
        var name = Path.GetFileName(fileName ?? "document");
        name = name.Replace("..", "", StringComparison.Ordinal);
        name = Regex.Replace(name, @"[^\w\.\-\s]", "_");
        return string.IsNullOrWhiteSpace(name) ? "document.bin" : name;
    }

    public static (bool Ok, string? Error, string Extension, string ContentType) Validate(
        Stream content,
        string originalFileName,
        string? declaredContentType,
        long sizeBytes)
    {
        if (sizeBytes <= 0)
            return (false, "Zero-byte files are not allowed.", "", "");
        if (sizeBytes > MaxFileSizeBytes)
            return (false, "File exceeds the 5 MB size limit.", "", "");

        var extension = Path.GetExtension(originalFileName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(extension)
            || BlockedExtensions.Contains(extension)
            || !AllowedExtensions.Contains(extension))
        {
            return (false, "Unsupported or blocked file type.", "", "");
        }

        if (!string.IsNullOrWhiteSpace(declaredContentType)
            && !AllowedContentTypes.Contains(declaredContentType)
            && !string.Equals(declaredContentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "MIME type is not allowed.", "", "");
        }

        Span<byte> header = stackalloc byte[8];
        var read = content.Read(header);
        if (content.CanSeek) content.Position = 0;
        if (read < 2)
            return (false, "File content is empty or unreadable.", "", "");

        // MZ executable
        if (header[0] == 0x4D && header[1] == 0x5A)
            return (false, "Executable files are not allowed.", "", "");

        var ext = extension.ToLowerInvariant();
        if (ext == ".pdf" && !(header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46))
            return (false, "PDF signature mismatch.", "", "");
        if (ext is ".docx" or ".png")
        {
            // ZIP/PNG signatures — DOCX is ZIP (PK), PNG is specific
            if (ext == ".docx" && !(header[0] == 0x50 && header[1] == 0x4B))
                return (false, "DOCX signature mismatch.", "", "");
            if (ext == ".png" && !(header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47))
                return (false, "PNG signature mismatch.", "", "");
        }
        if (ext is ".jpg" or ".jpeg" && !(header[0] == 0xFF && header[1] == 0xD8))
            return (false, "JPEG signature mismatch.", "", "");

        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };

        return (true, null, ext, contentType);
    }

    public static string ComputeSha256(Stream content)
    {
        if (content.CanSeek) content.Position = 0;
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(content);
        if (content.CanSeek) content.Position = 0;
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

public sealed class LocalDevelopmentStorageProvider : IFileStorageProvider
{
    private readonly string _uploadRoot;
    private bool _verified;

    public LocalDevelopmentStorageProvider(IWebHostEnvironment environment)
    {
        _uploadRoot = Path.Combine(environment.ContentRootPath, "App_Data", "uploads");
        Directory.CreateDirectory(_uploadRoot);
    }

    public string Name => "LocalDevelopment";

    public StorageProviderStatusDto GetStatus() => new()
    {
        Name = "Local development storage",
        Status = _verified || Directory.Exists(_uploadRoot) ? "Healthy" : "Configured",
        Detail = "Private App_Data/uploads. Azure Blob cloud remains NotConfigured without credentials."
    };

    public async Task<(bool Ok, string? Error, StoredObjectResult? Result)> SaveAsync(
        Stream content,
        string originalFileName,
        string? contentType,
        string logicalCategory,
        int? organizationId,
        int candidateId,
        CancellationToken ct = default)
    {
        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);
        buffer.Position = 0;

        var validation = FileUploadValidator.Validate(buffer, originalFileName, contentType, buffer.Length);
        if (!validation.Ok)
            return (false, validation.Error, null);

        var checksum = FileUploadValidator.ComputeSha256(buffer);
        var displayName = FileUploadValidator.SanitizeDisplayName(originalFileName);
        var orgPart = organizationId is > 0 ? organizationId.Value.ToString() : "personal";
        var storageKey =
            $"tenant/{orgPart}/candidate/{candidateId}/{Sanitize(logicalCategory)}/{Guid.NewGuid():N}{validation.Extension}";
        var absolutePath = ResolveSafePath(storageKey);
        if (absolutePath is null)
            return (false, "Invalid storage path.", null);

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        buffer.Position = 0;
        await using (var output = File.Create(absolutePath))
        {
            await buffer.CopyToAsync(output, ct);
        }

        _verified = true;
        return (true, null, new StoredObjectResult
        {
            StorageKey = storageKey,
            SanitizedFileName = displayName,
            ContentType = validation.ContentType,
            SizeBytes = buffer.Length,
            ChecksumSha256 = checksum,
            ValidationStatus = "Clean",
            ScanStatus = "NotConfigured",
            Provider = Name
        });
    }

    public Task<(bool Ok, string? Error, Stream? Content, string? ContentType)> OpenReadAsync(
        string storageKey,
        CancellationToken ct = default)
    {
        var absolutePath = ResolveSafePath(storageKey);
        // Backward-compatible keys from Phase 4–7: resumes/{guid}.ext under upload root
        if (absolutePath is null || !File.Exists(absolutePath))
        {
            var legacy = ResolveLegacyPath(storageKey);
            if (legacy is null || !File.Exists(legacy))
                return Task.FromResult<(bool, string?, Stream?, string?)>((false, "File not found.", null, null));
            absolutePath = legacy;
        }

        var ext = Path.GetExtension(absolutePath).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };
        _verified = true;
        Stream stream = File.OpenRead(absolutePath);
        return Task.FromResult<(bool, string?, Stream?, string?)>((true, null, stream, contentType));
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        var path = ResolveSafePath(storageKey) ?? ResolveLegacyPath(storageKey);
        if (path != null && File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }

    private string? ResolveSafePath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey)
            || storageKey.Contains("..", StringComparison.Ordinal)
            || Path.IsPathRooted(storageKey))
            return null;

        var normalizedKey = storageKey.Replace('\\', '/').TrimStart('/');
        var absolutePath = Path.GetFullPath(Path.Combine(_uploadRoot, normalizedKey.Replace('/', Path.DirectorySeparatorChar)));
        return absolutePath.StartsWith(_uploadRoot, StringComparison.OrdinalIgnoreCase) ? absolutePath : null;
    }

    private string? ResolveLegacyPath(string storageKey) => ResolveSafePath(storageKey);

    private static string Sanitize(string value)
    {
        var sanitized = new string(value.Where(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_').ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "files" : sanitized.ToLowerInvariant();
    }
}

public sealed class AzuriteBlobStorageProvider : IFileStorageProvider
{
    private readonly IConfiguration _config;

    public AzuriteBlobStorageProvider(IConfiguration config, LocalDevelopmentStorageProvider fallback)
    {
        _config = config;
        _ = fallback;
    }

    public string Name => "Azurite";

    private bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_config["Storage:Azurite:ConnectionString"]);

    public StorageProviderStatusDto GetStatus() => new()
    {
        Name = "Azurite",
        Status = !IsConfigured ? "NotConfigured" : "Configured",
        Detail = IsConfigured
            ? "Azurite connection configured. Mark Verified only after a real blob operation."
            : "Azurite NotConfigured. Local development storage remains available."
    };

    public Task<(bool Ok, string? Error, StoredObjectResult? Result)> SaveAsync(
        Stream content, string originalFileName, string? contentType, string logicalCategory,
        int? organizationId, int candidateId, CancellationToken ct = default)
    {
        if (!IsConfigured)
            return Task.FromResult<(bool, string?, StoredObjectResult?)>((false, "Azurite NotConfigured.", null));
        // Without Azure SDK credentials exercise, refuse to claim Azurite verified — use local fallback only when explicitly selected.
        return Task.FromResult<(bool, string?, StoredObjectResult?)>((false, "Azurite adapter present but not exercised in this environment.", null));
    }

    public Task<(bool Ok, string? Error, Stream? Content, string? ContentType)> OpenReadAsync(
        string storageKey, CancellationToken ct = default)
        => Task.FromResult<(bool, string?, Stream?, string?)>((false, "Azurite NotConfigured.", null, null));

    public Task DeleteAsync(string storageKey, CancellationToken ct = default) => Task.CompletedTask;
}

public sealed class AzureBlobStorageProvider : IFileStorageProvider
{
    private readonly IConfiguration _config;

    public AzureBlobStorageProvider(IConfiguration config) => _config = config;

    public string Name => "AzureBlob";

    private bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_config["Storage:AzureBlob:ConnectionString"])
        || !string.IsNullOrWhiteSpace(_config["Storage:AzureBlob:AccountUrl"]);

    public StorageProviderStatusDto GetStatus() => new()
    {
        Name = "Azure Blob cloud",
        Status = "NotConfigured",
        Detail = IsConfigured
            ? "Credentials detected in configuration but cloud verification was not completed in this cycle."
            : "Azure Blob NotConfigured without secure credentials."
    };

    public Task<(bool Ok, string? Error, StoredObjectResult? Result)> SaveAsync(
        Stream content, string originalFileName, string? contentType, string logicalCategory,
        int? organizationId, int candidateId, CancellationToken ct = default)
        => Task.FromResult<(bool, string?, StoredObjectResult?)>((false, "Azure Blob NotConfigured.", null));

    public Task<(bool Ok, string? Error, Stream? Content, string? ContentType)> OpenReadAsync(
        string storageKey, CancellationToken ct = default)
        => Task.FromResult<(bool, string?, Stream?, string?)>((false, "Azure Blob NotConfigured.", null, null));

    public Task DeleteAsync(string storageKey, CancellationToken ct = default) => Task.CompletedTask;
}

/// <summary>Bridges legacy ILocalFileStorageService callers to IFileStorageProvider.</summary>
public sealed class LocalFileStorageBridge : ILocalFileStorageService
{
    private readonly IFileStorageProvider _provider;
    private readonly ICurrentUserService _currentUser;

    public LocalFileStorageBridge(IFileStorageProvider provider, ICurrentUserService currentUser)
    {
        _provider = provider;
        _currentUser = currentUser;
    }

    public async Task<(bool Ok, string? Error, StoredFileResult? Result)> SaveAsync(IFormFile file, string category)
    {
        if (file is null)
            return (false, "File is required.", null);
        await using var stream = file.OpenReadStream();
        var candidateId = _currentUser.UserId ?? 0;
        var saved = await _provider.SaveAsync(stream, file.FileName, file.ContentType, category, null, candidateId);
        if (!saved.Ok || saved.Result is null)
            return (false, saved.Error, null);
        return (true, null, new StoredFileResult(saved.Result.StorageKey, saved.Result.SanitizedFileName));
    }

    public async Task<(bool Ok, string? Error, Stream? Content, string? ContentType, string? FileName)> OpenReadAsync(string storageKey)
    {
        var opened = await _provider.OpenReadAsync(storageKey);
        if (!opened.Ok)
            return (false, opened.Error, null, null, null);
        return (true, null, opened.Content, opened.ContentType, Path.GetFileName(storageKey));
    }

    public Task DeleteAsync(string storageKey) => _provider.DeleteAsync(storageKey);
}

public interface IStorageAdminService
{
    Task<IReadOnlyList<StorageProviderStatusDto>> GetStatusesAsync();
    Task<object> DryRunMigrationAsync();
}

public sealed class StorageAdminService : IStorageAdminService
{
    private readonly LocalDevelopmentStorageProvider _local;
    private readonly AzuriteBlobStorageProvider _azurite;
    private readonly AzureBlobStorageProvider _azure;
    private readonly IAntivirusScanner _antivirus;
    private readonly ApplicationDbContext _db;
    private readonly IFileStorageProvider _active;

    public StorageAdminService(
        LocalDevelopmentStorageProvider local,
        AzuriteBlobStorageProvider azurite,
        AzureBlobStorageProvider azure,
        IAntivirusScanner antivirus,
        ApplicationDbContext db,
        IFileStorageProvider active)
    {
        _local = local;
        _azurite = azurite;
        _azure = azure;
        _antivirus = antivirus;
        _db = db;
        _active = active;
    }

    public async Task<IReadOnlyList<StorageProviderStatusDto>> GetStatusesAsync()
    {
        var quarantined = await _db.CandidateDocuments.CountAsync(d => d.ValidationStatus == "Quarantined")
            + await _db.Resumes.CountAsync(r => r.ValidationStatus == "Quarantined");
        var list = new List<StorageProviderStatusDto>
        {
            _local.GetStatus(),
            _azurite.GetStatus(),
            _azure.GetStatus(),
            _antivirus.GetStatus(),
            new StorageProviderStatusDto
            {
                Name = "Active provider",
                Status = _active.GetStatus().Status,
                Detail = _active.Name,
                QuarantinedDocumentCount = quarantined
            }
        };
        foreach (var item in list)
        {
            item.LastCheckedUtc = DateTime.UtcNow;
            item.QuarantinedDocumentCount ??= quarantined;
        }
        return list;
    }

    public async Task<object> DryRunMigrationAsync()
    {
        var resumes = await _db.Resumes.AsNoTracking().CountAsync(r => !r.FilePath.StartsWith("tenant/"));
        var docs = await _db.CandidateDocuments.AsNoTracking().CountAsync(d => !d.FilePath.StartsWith("tenant/"));
        return new
        {
            mode = "dry-run",
            wouldMigrateResumes = resumes,
            wouldMigrateDocuments = docs,
            changed = false,
            note = "Dry-run only. No files moved. Execute requires explicit authorization and is not run automatically."
        };
    }
}
