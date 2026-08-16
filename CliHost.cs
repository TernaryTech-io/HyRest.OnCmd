using HyRest.OnBase;
using HyRest.OnCmd.Configuration;
using HyRest.OnCmd.UserInterface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace HyRest.OnCmd;

public class CliHost
{
    private readonly HylandClientOptions _options;
    private readonly ILogger<CliHost> _logger;
    private OnBaseApp _app;
    private OnBaseAppBuilder _builder;
    public CliHost(HylandClientOptions options)
    {     
        _options = options;

        //Makes it prettier
        Console.InputEncoding = Encoding.Unicode;
        Console.OutputEncoding = Encoding.Unicode;        
        Console.CancelKeyPress += ConsoleCancelHandeler;
        _builder = OnBaseAppBuilder.Create(options);
    }
    public OnBaseApp App => _app;
    public ILogger<CliHost> Logger => _logger;

    public MenuResult Start()
    {
        try
        {
            var resp = LoginScreen.Go();
            if (resp.Result != null && resp.Result is string[] result)
            {
                var creds = CliHostBuilder.GetCredentials(result[0], result[1]);                               
                _app = UI.Execute("Logging in...", () =>
                {
                    return _builder
                    .WithCredentials(creds)
                    .Build();
                });
                return MainMenu.Go(this, new LoginScreen());
            }
        }
        catch(Exception ex)
        {
            UI.ShowError(ex);
        }
        
        return Start();
    }    
    public void Stop()
    {
        AnsiConsole.Clear();
        if (_app != null && _app.IsConnected)
            _app.Session.Disconnect();        
        AnsiConsole.Write(UI.MenuText("Bye!"));
        Environment.Exit(0);
    }
    private void ConsoleCancelHandeler(object? sender, ConsoleCancelEventArgs e) => Stop();
}



