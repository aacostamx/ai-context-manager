using AIContextManager;
using Microsoft.Extensions.Options;

namespace AIContextManager.Tests;

public class GroupingTests
{
    private static readonly string[] CorporateLayout =
    [
        "intsol-dna-list-matching-api", "intsol-dna-list-matching-ui",
        "intsol-biography-api", "intsol-biography-ui", "intsol-biography-sync",
        "intsol-mkt-bio-api", "intsol-mkt-bio-data-api", "intsol-mkt-bio-ui",
        "intsol-sa-rules-salesforce-api", "intsol-sa-rules-ui",
        "intsol-gitops", "netcore-helm-chart", "react-helm-chart",
    ];

    private static IReadOnlyList<RepositoryInfo> Group(params string[] names) =>
        new RepositoryGroupingService(Options.Create(new ContextManagerOptions()))
            .Group(names.Select(n => Path.Combine(@"C:\source", n)));

    [Theory]
    [InlineData("intsol-dna-list-matching-api", "intsol-dna-list-matching", "API")]
    [InlineData("intsol-dna-list-matching-ui", "intsol-dna-list-matching", "UI")]
    [InlineData("intsol-biography-api", "intsol-biography", "API")]
    [InlineData("intsol-biography-ui", "intsol-biography", "UI")]
    [InlineData("intsol-biography-sync", "intsol-biography", "Sync")]
    [InlineData("intsol-mkt-bio-data-api", "intsol-mkt-bio", "Data API")]
    [InlineData("intsol-mkt-bio-ui", "intsol-mkt-bio", "UI")]
    [InlineData("intsol-sa-rules-salesforce-api", "intsol-sa-rules", "Salesforce API")]
    [InlineData("intsol-sa-rules-ui", "intsol-sa-rules", "UI")]
    public void Groups_the_documented_examples(string repo, string solution, string component)
    {
        var info = Group(CorporateLayout).Single(r => r.Name == repo);

        Assert.Equal(solution, info.Solution);
        Assert.Equal(component, info.Component);
        Assert.False(info.IsShared);
    }

    [Theory]
    [InlineData("intsol-gitops")]
    [InlineData("netcore-helm-chart")]
    [InlineData("react-helm-chart")]
    public void Treats_gitops_and_helm_charts_as_shared_infrastructure(string repo)
    {
        var info = Group(CorporateLayout).Single(r => r.Name == repo);

        Assert.True(info.IsShared);
        Assert.Equal($"Shared-Infrastructure/{repo}-context.md", info.RelativeContextFile);
    }

    [Fact]
    public void One_context_file_per_repository_inside_its_solution_folder()
    {
        var info = Group(CorporateLayout).Single(r => r.Name == "intsol-mkt-bio-data-api");

        Assert.Equal("intsol-mkt-bio/intsol-mkt-bio-data-api-context.md", info.RelativeContextFile);
    }

    [Fact]
    public void Shared_prefix_alone_does_not_merge_repositories()
    {
        // "intsol-bio" is a prefix of "intsol-biography" as text, but not at a word boundary.
        var repos = Group("intsol-bio-api", "intsol-biography-ui");

        Assert.Equal("intsol-bio", repos.Single(r => r.Name == "intsol-bio-api").Solution);
        Assert.Equal("intsol-biography", repos.Single(r => r.Name == "intsol-biography-ui").Solution);
    }

    [Fact]
    public void Only_one_extra_word_may_join_another_solution()
    {
        var repos = Group("intsol-sa-rules-ui", "intsol-sa-rules-salesforce-sync-api");

        Assert.Equal("intsol-sa-rules-salesforce-sync", repos.Single(r => r.Name == "intsol-sa-rules-salesforce-sync-api").Solution);
    }

    [Fact]
    public void Repository_without_known_suffix_is_its_own_solution()
    {
        var info = Group("intsol-tools").Single();

        Assert.Equal("intsol-tools", info.Solution);
        Assert.Equal("Main", info.Component);
    }

    [Fact]
    public void Single_repository_with_suffix_keeps_its_own_solution()
    {
        var info = Group("intsol-lonely-api").Single();

        Assert.Equal("intsol-lonely", info.Solution);
        Assert.Equal("API", info.Component);
    }

    [Fact]
    public void Configured_shared_repository_is_shared()
    {
        var service = new RepositoryGroupingService(Options.Create(new ContextManagerOptions { SharedRepositories = ["platform-config"] }));

        Assert.True(service.Group([@"C:\source\platform-config"]).Single().IsShared);
    }
}
