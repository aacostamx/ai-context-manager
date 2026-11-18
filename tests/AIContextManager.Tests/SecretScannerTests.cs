using AIContextManager;

namespace AIContextManager.Tests;

public class SecretScannerTests
{
    // Built by concatenation so this test file itself does not look like it contains secrets.
    private static readonly string FakeAwsKey = "AKIA" + "ABCDEFGHIJKLMNOP";
    private static readonly string FakeGitHubToken = "ghp_" + new string('a', 36);

    private static string Context(string file, string code) => $"# header\n\n## File: {file}\n```\n{code}\n```\n";

    [Fact]
    public void Clean_code_has_no_findings()
    {
        var md = Context("src/Program.cs", "var x = 1;\nconst string ApiKeyHeader = \"X-Api-Key\";");

        Assert.Empty(SecretScanner.Scan(md));
    }

    [Fact]
    public void Finds_aws_key_with_file_and_line_but_never_the_value()
    {
        var findings = SecretScanner.Scan(Context("config/aws.json", $"{{\n  \"key\": \"{FakeAwsKey}\"\n}}"));

        var f = Assert.Single(findings);
        Assert.Equal("config/aws.json", f.File);
        Assert.Equal(2, f.Line);
        Assert.DoesNotContain(FakeAwsKey, f.ToString());
    }

    [Theory]
    [InlineData("-----BEGIN RSA PRIVATE KEY-----")]
    [InlineData("\"ConnectionStrings\": { \"Db\": \"Server=sql;User Id=app;Password=S3cr3tValue!;\" }")]
    [InlineData("\"ClientSecret\": \"q8F~kd93jfLs02\"")]
    [InlineData("api_key = 'f8a7d6c5b4a39281'")]
    public void Detects_common_secret_shapes(string line) =>
        Assert.NotEmpty(SecretScanner.Scan(Context("appsettings.json", line)));

    [Fact]
    public void Detects_github_token() =>
        Assert.NotEmpty(SecretScanner.Scan(Context("ci.yml", $"token: {FakeGitHubToken}")));

    [Theory]
    [InlineData("\"Password\": \"\"")]
    [InlineData("\"Password\": \"#{DbPassword}#\"")]
    [InlineData("\"ClientSecret\": \"${CLIENT_SECRET}\"")]
    [InlineData("\"ApiKey\": \"<your-api-key>\"")]
    [InlineData("Server=sql;Password=${DB_PASSWORD};")]
    [InlineData("password: \"changeme\"")]
    public void Ignores_placeholders(string line) =>
        Assert.Empty(SecretScanner.Scan(Context("appsettings.json", line)));

    [Theory]
    [InlineData(".env", true)]
    [InlineData("src/.env.production", true)]
    [InlineData(".env.example", false)]
    [InlineData("certs/server.pfx", true)]
    [InlineData("keys/id_rsa", true)]
    [InlineData(".npmrc", true)]
    [InlineData("src/Api/appsettings.Development.local.json", true)]
    [InlineData("src/Api/appsettings.json", false)]
    [InlineData("src/Api/Program.cs", false)]
    public void Sensitive_files_are_excluded_from_snapshots(string path, bool sensitive) =>
        Assert.Equal(sensitive, SensitiveFiles.IsSensitive(path));
}
