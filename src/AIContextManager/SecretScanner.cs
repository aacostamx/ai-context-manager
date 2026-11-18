using System.Text.RegularExpressions;

namespace AIContextManager;

/// <summary>Where a possible secret was found. The value itself is never kept.</summary>
public sealed record SecretFinding(string Rule, string File, int Line);

/// <summary>
/// Fail-closed scan of a generated context before it is published. Any finding blocks publication.
/// It deliberately does not redact: a blocked file is never published, the user excludes the file
/// (.repomixignore / ExtraIgnorePatterns) or removes the secret from the repository.
/// </summary>
public static partial class SecretScanner
{
    private static readonly (string Rule, Regex Pattern, bool CheckValue)[] Rules =
    [
        ("Private key", PrivateKey(), false),
        ("AWS access key", AwsAccessKey(), false),
        ("AWS secret key", AwsSecretKey(), false),
        ("GitHub token", GitHubToken(), false),
        ("Slack token", SlackToken(), false),
        ("Azure storage/service bus key", AzureKey(), false),
        ("JSON Web Token", Jwt(), false),
        ("Password in connection string", ConnectionStringPassword(), true),
        ("Hard-coded credential", KeyValueSecret(), true),
    ];

    private static readonly HashSet<string> PlaceholderWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "changeme", "change-me", "password", "secret", "dummy", "example", "sample", "placeholder", "redacted",
        "null", "none", "empty", "test", "todo", "string", "your-password", "yourpassword", "your-secret", "notset",
    };

    public static IReadOnlyList<SecretFinding> Scan(string markdown)
    {
        var findings = new List<SecretFinding>();
        var currentFile = "(header)";
        var lineInFile = 0;

        using var reader = new StringReader(markdown);
        while (reader.ReadLine() is { } line)
        {
            lineInFile++;
            if (line.StartsWith("## File: ", StringComparison.Ordinal))
            {
                currentFile = line["## File: ".Length..].Trim();
                lineInFile = -1; // the opening ``` fence follows; first code line becomes 1
                continue;
            }

            foreach (var (rule, pattern, checkValue) in Rules)
            {
                foreach (Match m in pattern.Matches(line))
                {
                    if (checkValue && IsPlaceholder(m.Groups["value"].Value)) continue;
                    findings.Add(new SecretFinding(rule, currentFile, Math.Max(lineInFile, 1)));
                    break;
                }
            }
        }

        return findings;
    }

    internal static bool IsPlaceholder(string value)
    {
        var v = value.Trim().Trim('"', '\'');
        if (v.Length < 6) return true;
        if (v.StartsWith("${") || v.StartsWith("{{") || v.StartsWith("#{") || v.StartsWith("$(") ||
            v.StartsWith('<') || v.StartsWith('%') || v.StartsWith('[') || v.StartsWith("__")) return true;
        if (v.All(c => c is '*' or 'x' or 'X' or '.' or '-')) return true;
        if (PlaceholderWords.Contains(v)) return true;
        return v.StartsWith("your", StringComparison.OrdinalIgnoreCase) || v.Contains("example", StringComparison.OrdinalIgnoreCase);
    }

    [GeneratedRegex(@"-----BEGIN (?:RSA |DSA |EC |OPENSSH |PGP |ENCRYPTED )?PRIVATE KEY(?: BLOCK)?-----")]
    private static partial Regex PrivateKey();

    [GeneratedRegex(@"\b(?:AKIA|ASIA)[0-9A-Z]{16}\b")]
    private static partial Regex AwsAccessKey();

    [GeneratedRegex(@"(?i)aws_?secret_?access_?key[""']?\s*[:=]\s*[""']?[A-Za-z0-9/+=]{40}\b")]
    private static partial Regex AwsSecretKey();

    [GeneratedRegex(@"\b(?:ghp|gho|ghu|ghs|ghr)_[A-Za-z0-9]{36}\b|\bgithub_pat_[A-Za-z0-9_]{50,}")]
    private static partial Regex GitHubToken();

    [GeneratedRegex(@"\bxox[abprs]-[A-Za-z0-9-]{10,}")]
    private static partial Regex SlackToken();

    [GeneratedRegex(@"(?i)(?:AccountKey|SharedAccessKey)\s*=\s*[A-Za-z0-9+/]{30,}={0,2}")]
    private static partial Regex AzureKey();

    [GeneratedRegex(@"\beyJ[A-Za-z0-9_-]{10,}\.eyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}")]
    private static partial Regex Jwt();

    [GeneratedRegex(@"(?i)\b(?:password|pwd)\s*=\s*(?<value>[^;""'\s]+)")]
    private static partial Regex ConnectionStringPassword();

    [GeneratedRegex(@"(?i)[""']?\b(?:password|passwd|secret|client_?secret|api_?key|access_?token|auth_?token|private_?key)[""']?\s*[:=]\s*[""'](?<value>[^""'\s]{6,})[""']")]
    private static partial Regex KeyValueSecret();
}
