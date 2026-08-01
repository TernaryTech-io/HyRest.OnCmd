using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace HyRest.OnCmd;

internal class MainMenu
{
    internal static Task LoadMainMenu()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(UI.MenuHeader("Main Menu"));
        var choice = AnsiConsole.Prompt(UI.NewPrompt<MainMenuItem>("Main Menu").AddChoices(
            MainMenuItem.Document_Retrieval,
            MainMenuItem.Document_Import,
            MainMenuItem.Document_Queries,
            MainMenuItem.Log_Out));
        return RouteChoice(choice);
    }

    internal static Task RouteChoice(MainMenuItem item) => item switch
    {
        _ => Task.Run(() => AnsiConsole.Markup(UI.ToStyle("Cool", UI.SecondaryColor, true)))
    };

}
public enum MainMenuItem
{
    Main_Menu,
    Document_Retrieval,
    Document_Import,
    Document_Queries,
    Log_Out
}