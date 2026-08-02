using DotNetEnv;
using HyRest.Identity.Credentials;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HyRest.OnCmd.Configuration;

public class CliHostBuilder
{
    private readonly HostApplicationBuilder _builder;
    private CliHostBuilder(HostApplicationBuilder builder)
    {
        _builder = builder;
    }
    public IServiceCollection Services => _builder.Services;
    public CliHost Build()
    {
        Env.Load();
        var logFactory = LoggerFactory.Create(config =>
        {
            config//.AddColorConsole()
            .SetMinimumLevel(LogLevel.Information);
        });
        _builder.Services.AddSingleton(logFactory);
        _builder.Services.AddSingleton(Options);
        var obLogger = logFactory.CreateLogger<OnBaseApp>();
        _builder.Services.AddSingleton<CliHost>();
        var host = _builder.Build();
        return host.Services.GetRequiredService<CliHost>();
    }
    public HylandClientOptions Options => new HylandClientOptions
    {
        ApiBaseUrl = Environment.GetEnvironmentVariable("HYREST_APIURL"),
        IdsBaseUrl = Environment.GetEnvironmentVariable("HYREST_IDSURL"),  
        UseQueryMetering = bool.Parse(Environment.GetEnvironmentVariable("HYREST_USE_QUERY_LIC"))
    };
    internal static IAuthenticationCredentials GetCredentials(string username, string password)
    {
        
        var clientId = Environment.GetEnvironmentVariable("HYREST_CLIENTID");
        var clientsecret = Environment.GetEnvironmentVariable("HYREST_CLIENTSECRET");
        return AuthenticationCredentials
        .CreateUserCredentials(
            username,
            password,
            clientId,
            clientsecret
        );
    }
    public static CliHostBuilder Create(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        return new CliHostBuilder(builder);
    }
}

