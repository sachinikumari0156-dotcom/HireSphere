using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using HireSphere.API.Models;
using UglyToad.PdfPig;

namespace HireSphere.API.Services.Ai;

public sealed class ProviderMetadataDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Deterministic";
    public string Status { get; set; } = "NotConfigured";
    public string? Detail { get; set; }
}

public sealed class ParsedResumeResult
{
    public string Provider { get; set; } = "Deterministic";
    public string ProviderType { get; set; } = "Deterministic";
    public string ProviderVersion { get; set; } = "deterministic-parse-v1";
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Summary { get; set; }
    public int? EstimatedYearsExperience { get; set; }
    public IReadOnlyList<ParsedSkillItem> Skills { get; set; } = Array.Empty<ParsedSkillItem>();
    public string AnalysisSummary { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public string? FallbackNote { get; set; }
}

public sealed class ParsedSkillItem
{
    public string RawName { get; set; } = string.Empty;
    public string CanonicalName { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public string? SourceEvidence { get; set; }
}

public interface IResumeParsingProvider
{
    string ProviderName { get; }
    string ProviderType { get; }
    ProviderMetadataDto GetStatus();
    Task<ParsedResumeResult> ParseAsync(Stream content, string fileName, string contentType, CancellationToken ct = default);
}

public interface ISkillExtractionProvider
{
    IReadOnlyList<ParsedSkillItem> ExtractAndNormalize(string text, IReadOnlyList<string> catalogue);
}

public interface IRecruitmentInsightProvider
{
    string ProviderName { get; }
    string InsightKind { get; }
}

public static class SkillNormalizer
{
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["JS"] = "JavaScript",
        ["Javascript"] = "JavaScript",
        ["TS"] = "TypeScript",
        ["Typescript"] = "TypeScript",
        ["C Sharp"] = "C#",
        ["Csharp"] = "C#",
        ["ASP.NET"] = "ASP.NET Core",
        ["AspNet"] = "ASP.NET Core",
        ["ReactJS"] = "React",
        ["React.js"] = "React",
        ["Node"] = "Node.js",
        ["NodeJS"] = "Node.js",
        ["Postgres"] = "PostgreSQL",
        ["MSSQL"] = "SQL Server",
        ["SQLServer"] = "SQL Server"
    };

    public static string Normalize(string raw)
    {
        var trimmed = raw.Trim();
        if (Aliases.TryGetValue(trimmed, out var mapped))
        {
            return mapped;
        }

        return trimmed;
    }
}

public sealed class DeterministicSkillExtractionProvider : ISkillExtractionProvider
{
    public IReadOnlyList<ParsedSkillItem> ExtractAndNormalize(string text, IReadOnlyList<string> catalogue)
    {
        var found = new Dictionary<string, ParsedSkillItem>(StringComparer.OrdinalIgnoreCase);
        var haystack = text ?? string.Empty;

        foreach (var skill in catalogue)
        {
            if (string.IsNullOrWhiteSpace(skill)) continue;
            var canonical = SkillNormalizer.Normalize(skill);
            if (ContainsSkill(haystack, skill) || ContainsSkill(haystack, canonical))
            {
                found[canonical] = new ParsedSkillItem
                {
                    RawName = skill,
                    CanonicalName = canonical,
                    Confidence = 0.85m,
                    SourceEvidence = TruncateEvidence(skill)
                };
            }
        }

        foreach (var alias in new[] { "JS", "TS", "C#", "React", "SQL", "Python", "Java" })
        {
            if (!ContainsSkill(haystack, alias)) continue;
            var canonical = SkillNormalizer.Normalize(alias);
            if (found.ContainsKey(canonical)) continue;
            found[canonical] = new ParsedSkillItem
            {
                RawName = alias,
                CanonicalName = canonical,
                Confidence = 0.7m,
                SourceEvidence = TruncateEvidence(alias)
            };
        }

        return found.Values
            .GroupBy(s => s.CanonicalName, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderByDescending(x => x.Confidence).First())
            .OrderBy(s => s.CanonicalName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool ContainsSkill(string text, string skill)
    {
        if (string.IsNullOrWhiteSpace(skill)) return false;
        return Regex.IsMatch(text, $@"\b{Regex.Escape(skill)}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static string TruncateEvidence(string value) =>
        value.Length <= 80 ? value : value[..80];
}

/// <summary>
/// Local deterministic resume parser. Not an external AI provider.
/// </summary>
public sealed class DeterministicResumeParsingProvider : IResumeParsingProvider
{
    public const int MaxExtractedChars = 200_000;
    private readonly ISkillExtractionProvider _skills;

    public DeterministicResumeParsingProvider(ISkillExtractionProvider skills)
    {
        _skills = skills;
    }

    public string ProviderName => "Deterministic";
    public string ProviderType => "Deterministic";

    public ProviderMetadataDto GetStatus() => new()
    {
        Name = ProviderName,
        Type = ProviderType,
        Status = "Healthy",
        Detail = "Rules-based local text extraction for PDF/DOCX."
    };

    public async Task<ParsedResumeResult> ParseAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        await Task.Yield();
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? string.Empty;
        if (extension is not (".pdf" or ".docx"))
        {
            return new ParsedResumeResult { FailureReason = "Unsupported resume type. Only PDF and DOCX are supported." };
        }

        string text;
        try
        {
            text = extension == ".pdf" ? ExtractPdf(content) : ExtractDocx(content);
        }
        catch (Exception)
        {
            return new ParsedResumeResult { FailureReason = "Could not read resume content." };
        }

        text = SanitizeUntrustedDocumentText(text);
        if (text.Length > MaxExtractedChars)
        {
            return new ParsedResumeResult { FailureReason = "Extracted resume text exceeds the maximum allowed size." };
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return new ParsedResumeResult { FailureReason = "No extractable text found in resume." };
        }

        var email = Regex.Match(text, @"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", RegexOptions.IgnoreCase).Value;
        var phone = Regex.Match(text, @"(\+?\d[\d\s().-]{7,}\d)").Value;
        var years = Regex.Match(text, @"(\d{1,2})\s*\+?\s*(years?|yrs?)\s+(of\s+)?experience", RegexOptions.IgnoreCase);
        int? estimatedYears = years.Success && int.TryParse(years.Groups[1].Value, out var y) ? y : null;

        var firstLine = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault(l => l.Length is > 2 and < 120 && !l.Contains('@'));
        var summary = text.Length <= 400 ? text : text[..400].Trim() + "…";

        var catalogue = new[]
        {
            "C#", "JavaScript", "TypeScript", "React", "SQL Server", "Python", "Java",
            "ASP.NET Core", "Node.js", "Azure", "Docker", "Kubernetes", "HTML", "CSS"
        };
        var skills = _skills.ExtractAndNormalize(text, catalogue);

        return new ParsedResumeResult
        {
            Provider = ProviderName,
            ProviderType = ProviderType,
            ProviderVersion = "deterministic-parse-v1",
            Name = firstLine,
            Email = string.IsNullOrWhiteSpace(email) ? null : email,
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            Summary = summary,
            EstimatedYearsExperience = estimatedYears,
            Skills = skills,
            AnalysisSummary = $"Deterministic extraction found {skills.Count} skill candidate(s). Review required before profile updates."
        };
    }

    private static string ExtractPdf(Stream stream)
    {
        using var doc = PdfDocument.Open(stream);
        var sb = new StringBuilder();
        foreach (var page in doc.GetPages())
        {
            sb.AppendLine(page.Text);
        }

        return sb.ToString();
    }

    private static string ExtractDocx(Stream stream)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        ms.Position = 0;
        using var word = WordprocessingDocument.Open(ms, false);
        var body = word.MainDocumentPart?.Document?.Body;
        return body?.InnerText ?? string.Empty;
    }

    /// <summary>
    /// Treat document text as untrusted. Neutralize common prompt-injection markers for display/storage summaries.
    /// </summary>
    public static string SanitizeUntrustedDocumentText(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        var sanitized = text
            .Replace('\0', ' ')
            .Replace("```", "'''");
        sanitized = Regex.Replace(sanitized,
            @"(?i)(ignore\s+(all\s+)?(previous|prior)\s+instructions|system\s*prompt|you\s+are\s+now)",
            "[filtered]");
        return sanitized;
    }
}

/// <summary>
/// External AI adapter foundation. Remains NotConfigured without real credentials and successful calls.
/// </summary>
public sealed class ExternalAiResumeParsingProvider : IResumeParsingProvider
{
    private readonly IConfiguration _configuration;

    public ExternalAiResumeParsingProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string ProviderName => "ExternalAI";
    public string ProviderType => "ExternalAI";

    public bool IsEnabled
    {
        get
        {
            var enabled = _configuration.GetValue<bool>("Ai:External:Enabled");
            var key = _configuration["Ai:External:ApiKey"]
                ?? Environment.GetEnvironmentVariable("HIRESPHERE_AI_API_KEY");
            return enabled && !string.IsNullOrWhiteSpace(key);
        }
    }

    public ProviderMetadataDto GetStatus() => new()
    {
        Name = ProviderName,
        Type = ProviderType,
        Status = IsEnabled ? "Configured" : "NotConfigured",
        Detail = IsEnabled
            ? "External AI credentials present; live call required for Healthy."
            : "No external AI credentials configured. Deterministic fallback is used."
    };

    public Task<ParsedResumeResult> ParseAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        if (!IsEnabled)
        {
            return Task.FromResult(new ParsedResumeResult
            {
                Provider = ProviderName,
                ProviderType = ProviderType,
                FailureReason = "External AI is NotConfigured."
            });
        }

        // Real HTTP integration is Phase 8 optional when credentials exist.
        // Without a verified successful schema-validated call, do not claim ExternalAI success.
        return Task.FromResult(new ParsedResumeResult
        {
            Provider = ProviderName,
            ProviderType = ProviderType,
            FailureReason = "External AI endpoint is not verified in this environment."
        });
    }
}

public sealed class DeterministicRecruitmentInsightProvider : IRecruitmentInsightProvider
{
    public string ProviderName => "Deterministic";
    public string InsightKind => "Descriptive insight";
}
