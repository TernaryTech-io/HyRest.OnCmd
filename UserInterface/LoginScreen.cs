using System;
using System.Collections.Generic;
using System.Text;

namespace HyRest.OnCmd;

public class LoginScreen
{
    internal static Layout LoginLayout => new Layout("login")
        .SplitColumns(
            new Layout("left").Size(4),
            new Layout("center").Size(12),
            new Layout("right").Size(4)
        );
    internal static Panel LoginPanel
        => new Panel(LoginGrid)
        .Padding(4, 2)
        .BorderColor(UI.PrimaryColor)
        .RoundedBorder()
        .Expand();
    internal static Grid FigletGrid => new Grid()
        .AddColumn(new GridColumn())
        .AddRow(UI.Figlet)
        .AddRow(new Text("A Terminal Client for OnBase Powered by HyRest"));
    internal static Grid LoginGrid => new Grid()
        .AddColumns(
            new GridColumn().Centered().Padding(2, 2),
            new GridColumn().LeftAligned().Padding(4, 2)
        ).AddEmptyRow()
        .AddRow(UI.Logo, FigletGrid)
        .AddEmptyRow()
        .Expand();

    public static LoginOption Init()
    {
        AnsiConsole.Write(LoginPanel);
        return AnsiConsole.Prompt(
            UI.NewPrompt<LoginOption>("Select and Option")
            .AddChoices(LoginOption.Login, LoginOption.Exit));
    }
    public static (string, string) Login()
    {
        AnsiConsole.Write(UI.InStyle("Enter your Username and Password at the prompts", UI.PrimaryColor, true));
        AnsiConsole.WriteLine();
        var username = AnsiConsole.Prompt(UI.TextPrompt<string>("Username"));
        var password = AnsiConsole.Prompt(UI.SecretPrompt<string>("Password"));
        return (username, password);
    }
    public static void Exit()
    {
        AnsiConsole.Write(Align.Center(UI.InStyle("Goodbye!", UI.PrimaryColor, true)));
        Environment.Exit(0);
    }
}

public enum LoginOption
{
    Login,
    Exit
}