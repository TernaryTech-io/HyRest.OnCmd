namespace HyRest.OnCmd.UserInterface;

public class MainMenu : Screen
{
    private CliHost _host;
    public MainMenu(CliHost host, IScreen returnScreen) : base(returnScreen)
    {
        _host = host;
    }

    public static MenuResult Go(CliHost host, IScreen returnScreen) => new MainMenu(host, returnScreen).RunScreen();
    public override MenuResult RunScreen()
    {
        UI.Clear();
        UI.Write(UI.MenuHeader("Main Menu"));
        var choice = UI.Prompt(UI.NewEnumPrompt<MainMenuOptions>("Main Menu"));
        return RouteChoice(choice);
    }

    protected override MenuResult RouteChoice<TOption>(TOption choice) => choice switch
    {
        MainMenuOptions.Document_Retrieval => DocumentRetrieval.Go(_host, this),
        MainMenuOptions.Document_Import => DocumentImport.Go(_host,this),
        MainMenuOptions.Back => LogOut()
    };
    private MenuResult LogOut()
    {
        Console.Clear();
        if(_host.App.IsConnected)
            _host.App.Session.Disconnect();
        UI.Write(UI.MenuText("Bye!"));
        return _host.Start();
    }
}
public enum MainMenuOptions
{
    Document_Retrieval,
    Document_Import,
    Back
}