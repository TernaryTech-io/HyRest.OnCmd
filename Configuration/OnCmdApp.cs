using DotNetEnv;
using HyRest.Relay;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using HyRest.DependencyInjection;
using HyRest.Identity.Credentials;

namespace HyRest.OnCmd.Configuration;

public class OnCmdAppBuilder
{
    private readonly HostApplicationBuilder _builder;    
    internal OnCmdAppBuilder(HostApplicationBuilder builder)
    {
        _builder = builder;
    }
    internal void RegisterServices()
    {
        
        _builder.Services.AddSingleton<CancellationTokenSource>();        
        _builder.Logging.ClearProviders();
        _builder.Logging.AddColorConsole()
            .SetMinimumLevel(LogLevel.Information);  
    }
    public OnCmdApp Build(string username, string password)
    {
        RegisterServices();
        _builder.AddSingletonHylandApp(GetCredentials(username,password), (creds, options) =>
        {
            
            options.IdsBaseUrl = Environment.GetEnvironmentVariable("HYREST_IDSURL");
            options.ApiBaseUrl = Environment.GetEnvironmentVariable("HYREST_APIURL");
            //Optional: QueryMetering is false, set to true if you have the license.
            options.UseQueryMetering = true;
            //Optional: Default Language = en-US.
            options.DefaultLanguage = "en-US";
            //optional, a default will be created if not supplied, these are the default options
            options.ClientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = true, //This will be overridden to true if not set
                UseCookies = true, //This will be overridden to true if not set
                CookieContainer = new System.Net.CookieContainer() //If cookie container is not set, one will be created.
            };
        });
        _builder.Services.AddSingleton<KeepAliveService>();
        _builder.Services.AddSingleton<UserInterface>();

        return new OnCmdApp(_builder.Build());
    }    
    internal static IAuthenticationCredentials GetCredentials(string username, string password)
    {
        Env.Load();
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
    public static OnCmdAppBuilder Create()
        => new OnCmdAppBuilder(Host.CreateApplicationBuilder());
}

public class OnCmdApp 
{
    private readonly IHost _host;
    private readonly OnBaseApp _app;
    internal OnCmdApp(IHost host)
    {
        _host = host;
        _app = _host.Services.GetRequiredService<OnBaseApp>();        
        ConfigureStop();
    }
    public OnBaseApp App => _app;
    public void ConfigureStop()
    {
        var lifetime = _host.Services.GetRequiredService<IHostApplicationLifetime>();
        var tokensource = _host.Services.GetRequiredService<CancellationTokenSource>();
        lifetime.ApplicationStopping.Register(async () =>
        {
            tokensource.Cancel();
            if (_app != null)
            {
                if (_app.IsAuthenticated && _app.Session.IsActive)
                    await _app.Session.DisconnectAsync();
            }
        });
    }
    public void RegisterEventHandlers()
    {
        Console.CancelKeyPress += ConsoleCancelHandeler;
        AppDomain.CurrentDomain.ProcessExit += ProcessExitHandler;
        AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
    }    
    private void ProcessExitHandler(object? sender, EventArgs e) => _host.StopAsync().Wait();
    private void ConsoleCancelHandeler(object? sender, ConsoleCancelEventArgs e) => _host.StopAsync().Wait();
    private void CurrentDomain_DomainUnload(object? sender, EventArgs e) => _host.StopAsync().Wait();

}