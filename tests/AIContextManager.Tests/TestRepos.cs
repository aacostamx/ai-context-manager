using System.Diagnostics;

namespace AIContextManager.Tests;

/// <summary>Builds synthetic Git repositories (bare "origin" + working clone) in a temp folder.</summary>
internal sealed class TestRepos : IDisposable
{
    public string Root { get; } = Path.Combine(Path.GetTempPath(), "aicm-tests", Guid.NewGuid().ToString("N")[..8]);
    public string Source => Path.Combine(Root, "source");
    public string Origins => Path.Combine(Root, "origins");

    public TestRepos()
    {
        Directory.CreateDirectory(Source);
        Directory.CreateDirectory(Origins);
    }

    /// <summary>Creates origin with "main" and "develop" and clones it to source/{name}, checked out on a feature branch.</summary>
    public string CreateRepository(string name, bool withDevelop = true)
    {
        var origin = Path.Combine(Origins, name + ".git");
        var seed = Path.Combine(Root, "seed-" + name);
        Git(Root, "init", "--bare", "-b", "main", origin);
        Git(Root, "init", "-b", "main", seed);
        File.WriteAllText(Path.Combine(seed, "README.md"), $"# {name}\n");
        File.WriteAllText(Path.Combine(seed, "Program.cs"), "Console.WriteLine(\"main\");\n");
        File.WriteAllText(Path.Combine(seed, ".env"), "SECRET=do-not-copy\n");
        Commit(seed, "main commit");
        Git(seed, "remote", "add", "origin", origin);
        Git(seed, "push", "-q", "origin", "main");
        if (withDevelop)
        {
            Git(seed, "checkout", "-q", "-b", "develop");
            File.WriteAllText(Path.Combine(seed, "Program.cs"), "Console.WriteLine(\"develop\");\n");
            Commit(seed, "develop commit");
            Git(seed, "push", "-q", "origin", "develop");
        }

        var clone = Path.Combine(Source, name);
        Git(Root, "clone", "-q", origin, clone);
        Git(clone, "checkout", "-q", "-b", "feature/my-work");
        return clone;
    }

    /// <summary>Pushes a new commit to origin/{branch} from the seed clone (simulates a teammate).</summary>
    public void PushChange(string name, string branch, string file, string content)
    {
        var seed = Path.Combine(Root, "seed-" + name);
        Git(seed, "checkout", "-q", branch);
        File.WriteAllText(Path.Combine(seed, file), content);
        Commit(seed, "change " + file);
        Git(seed, "push", "-q", "origin", branch);
    }

    public static void Commit(string repo, string message)
    {
        Git(repo, "add", "-A");
        Git(repo, "-c", "user.name=test", "-c", "user.email=test@example.invalid", "commit", "-q", "-m", message);
    }

    public static string Git(string workDir, params string[] args)
    {
        var psi = new ProcessStartInfo("git") { WorkingDirectory = workDir, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var a in args) psi.ArgumentList.Add(a);
        using var p = Process.Start(psi)!;
        var stdout = p.StandardOutput.ReadToEnd();
        var stderr = p.StandardError.ReadToEnd();
        p.WaitForExit();
        if (p.ExitCode != 0) throw new InvalidOperationException($"git {string.Join(' ', args)} failed: {stderr}");
        return stdout.Trim();
    }

    public void Dispose()
    {
        try
        {
            foreach (var f in Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories)) File.SetAttributes(f, FileAttributes.Normal);
            Directory.Delete(Root, recursive: true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
