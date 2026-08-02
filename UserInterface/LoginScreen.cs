namespace HyRest.OnCmd.UserInterface;

public class LoginScreen : Screen
{
    public static MenuResult Go() => new LoginScreen().RunScreen();
    public LoginScreen() : base (null)
    {

    }
    public override MenuResult<string[]> RunScreen()
    {
        UI.Write(LoginPanel);
        var choice = UI.Prompt(
            UI.NewEnumPrompt<LoginOptions>("Welcome! Select an Option:"));
        return RouteChoice(choice);
    }

    protected override MenuResult<string[]> RouteChoice<TOption>(TOption choice) 
        => choice switch
    {
        LoginOptions.Login => MenuResult<string[]>.Create(Login()),
        LoginOptions.Exit => MenuResult<string[]>.Create(Exit())
    };
    internal Layout LoginLayout => new Layout("login")
        .SplitColumns(
            new Layout("left").Size(4),
            new Layout("center").Size(12),
            new Layout("right").Size(4)
        );
    internal Panel LoginPanel
        => new Panel(LoginGrid)
        .Padding(4, 2)
        .BorderColor(UI.PrimaryColor)
        .RoundedBorder()
        .Expand();
    internal Grid FigletGrid => new Grid()
        .AddColumn(new GridColumn())
        .AddRow(UI.Figlet)
        .AddRow(new Text("A Terminal Client for OnBase Powered by HyRest"));
    internal Grid LoginGrid => new Grid()
        .AddColumns(
            new GridColumn().Centered().Padding(2, 2),
            new GridColumn().LeftAligned().Padding(4, 2)
        ).AddEmptyRow()
        .AddRow(UI.Logo, FigletGrid)
        .AddEmptyRow()
        .Expand();

    public string[] Login()
    {
        UI.Write(UI.InStyle("Enter your Username and Password at the prompts", UI.PrimaryColor, true));
        UI.WriteLine();
        var username = UI.Prompt(UI.TextPrompt<string>("Username"));
        var password = UI.Prompt(UI.SecretPrompt<string>("Password"));
        return [username, password];
    }
    public string[] Exit()
    {
        UI.Write(Align.Center(UI.InStyle("Goodbye!", UI.PrimaryColor, true)));
        Environment.Exit(0);
        return [string.Empty,string.Empty];
    }
}


public enum LoginOptions
{
    Login,
    Exit
}