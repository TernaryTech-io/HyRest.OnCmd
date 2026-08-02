using HyRest.OnCmd.Configuration;
using HyRest.OnCmd.UserInterface;
using Microsoft.Extensions.Logging;
using System.Text;

namespace HyRest.OnCmd;

public class CliHost
{
    private readonly HylandClientOptions _options;
    private readonly ILoggerFactory _logFactory;
    private readonly ILogger<CliHost> _logger;
    private OnBaseApp _app;
    public CliHost(ILoggerFactory logFactory, HylandClientOptions options)
    {
        _logFactory = logFactory;
        _options = options;
        _logger = _logFactory.CreateLogger<CliHost>();

        //Makes it prettier
        Console.InputEncoding = Encoding.Unicode;
        Console.OutputEncoding = Encoding.Unicode;        
        Console.CancelKeyPress += ConsoleCancelHandeler;
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
                    return OnBaseApp.Create(_logFactory.CreateLogger<OnBaseApp>(), creds, _options);
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



