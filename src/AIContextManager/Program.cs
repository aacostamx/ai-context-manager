using AIContextManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// Usage:
//   dotnet run --project src/AIContextManager                 -> run and keep monitoring
//   dotnet run --project src/AIContextManager -- --once       -> one pass, then exit
//   ... -- --ContextManager:SourcePath=C:\source              -> override any setting
var once = args.Any(a => a.Equals("--once", StringComparison.OrdinalIgnoreCase));
var hostArgs = args.Where(a => !a.Equals("--once", StringComparison.OrdinalIgnoreCase)).ToArray();

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = hostArgs,
    ContentRootPath = AppContext.BaseDirectory, // find appsettings.json next to the exe, whatever the current folder
});

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.TimestampFormat = "HH:mm:ss ";
});

builder.Services.AddOptions<ContextManagerOptions>()
    .Bind(builder.Configuration.GetSection(ContextManagerOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddSingleton<IPostConfigureOptions<ContextManagerOptions>, ContextManagerOptionsSetup>();
builder.Services.AddSingleton<IValidateOptions<ContextManagerOptions>, ContextManagerOptionsValidator>();

builder.Services.AddSingleton(new RunMode(once));
builder.Services.AddSingleton<RepositoryDiscoveryService>();
builder.Services.AddSingleton<GitBranchService>();
builder.Services.AddSingleton<RepomixService>();
builder.Services.AddSingleton<ContextGenerator>();
builder.Services.AddSingleton<StateStore>();
builder.Services.AddSingleton<IndexWriter>();
builder.Services.AddHostedService<ContextMonitorService>();

try
{
    await builder.Build().RunAsync();
}
catch (OptionsValidationException ex)
{
    Console.Error.WriteLine("Invalid configuration (appsettings.json, section 'ContextManager'):");
    foreach (var failure in ex.Failures) Console.Error.WriteLine($"  - {failure}");
    return 1;
}

return Environment.ExitCode;
