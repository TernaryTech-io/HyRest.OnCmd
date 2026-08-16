using HyRest.OnBase.Core;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace HyRest.OnCmd;

public class UI
{
    public static void Write(string message, Color? color = null, bool bold = false)
        => AnsiConsole.Markup(message = ToStyle(message, color ?? Color.White, bold));
    public static void WriteLine(string? message = null, Color? color = null, bool bold = false)
        => AnsiConsole.MarkupLine(message = ToStyle(message ?? string.Empty, color ?? Color.White, bold));
    public static void Write(IRenderable renderable) => AnsiConsole.Write(renderable);
    public static T Prompt<T>(IPrompt<T> prompt) => AnsiConsole.Prompt(prompt);
    public static void Clear() => AnsiConsole.Clear();
    public static bool Confirm(string prompt, bool defaultValue = true) => AnsiConsole.Confirm(prompt, defaultValue);
    internal static CanvasImage Logo => new CanvasImage("Ternary-teal.png").MaxWidth(15);
    internal static FigletText Figlet => new FigletText("OnCmd").Color(Color.FromHex("#12d399")).Fitted();
    internal static FigletText MenuText(string text) => new FigletText(text).Color(Color.FromHex("#12d399")).Smushed();
    public static Color PrimaryColor => Color.FromHex("#12d399");
    public static Color SecondaryColor => Color.FromHex("#12add3");
    public static Color ErrorColor => Color.FromHex("#d3124c");
    public static Color WarnColor => Color.FromHex("#d39912");
    public static Style PrimaryStyle = new Style(PrimaryColor);
    private static string MarkUp(bool bold = false) => bold ? "[#{0} bold]{1}[/]" : "[#{0}]{1}[/]";
    
    public static string ToStyle(string message, Color? color = null, bool bold = false)
    {
            return string.Format(MarkUp(bold), color.HasValue ? color.Value.ToHex() : PrimaryColor.ToHex(), message);
    }
    public static Text InStyle(string message, Color? color = null, bool bold = false) 
        => bold ? new Text(message, new Style(color, null,Decoration.Bold)) : new Text(message, color);
    
    public static SelectionPrompt<T> NewPrompt<T>(string title, Color? color = null)
        where T : notnull
        => new SelectionPrompt<T>().Title(ToStyle(title, color, true)).HighlightStyle(new Style(PrimaryColor));

    public static MultiSelectionPrompt<T> NewMultiSelectPrompt<T>(string title, Color? color = null)
        where T : notnull
        => new MultiSelectionPrompt<T>().Title(ToStyle(title, color, true)).HighlightStyle(PrimaryStyle);
    public static SelectionPrompt<T> NewEnumPrompt<T>(string title) where T : Enum
    {
        var prompt = NewPrompt<T>(title);
        var members = Enum.GetValues(typeof(T));
        foreach(var member in members)
        {
            prompt.AddChoice((T)member);
        }
        return prompt;
    }
    public static Panel NewPanel(string header, IRenderable content)
        => new Panel(content).Header($"[#12d399]{header}[/]").RoundedBorder();
    public static Panel ErrorPanel(string header, IRenderable content)
    => new Panel(content).Header($"[#d3124c]{header}[/]").RoundedBorder().BorderColor(ErrorColor);
    public static void ShowError(Exception ex)
    {
        var header = "☠️ Error!";
        var grid = new Grid().AddColumn().Expand();
        grid.AddRow(ToStyle(Markup.Escape(ex.Message), ErrorColor));
        grid.AddRow(Markup.Escape(ex.StackTrace ?? "Oops"));
        var panel = ErrorPanel(header, grid);
        AnsiConsole.Write(panel);
    }
    public static void ShowSadPanel(string message)
    {
        var header = $"😤 Uhhhg!";
        var text = new Text(message, WarnColor);
        var panel = new Panel(text).Header(header).RoundedBorder().BorderColor(WarnColor);
        AnsiConsole.Write(panel);
    }
    public static TextPrompt<T> TextPrompt<T>(string message, Color? color = null, bool bold = false)
        => new TextPrompt<T>(ToStyle(message, color, true));
    public static TextPrompt<T> SecretPrompt<T>(string message, Color? color = null, bool bold = false)
        => new TextPrompt<T>(ToStyle(message, color, bold)).Secret('*');
    public static Layout MainLayout => new Layout()
        .SplitColumns(
            new Layout("left").Ratio(1),
            new Layout("center").Ratio(2),
            new Layout("right").Ratio(1)
        ).Size(50);
    internal static Panel MenuHeader(string name)
        => new Panel(UI.MenuText(name))
        .Padding(4, 2)
        .BorderColor(UI.PrimaryColor)
        .RoundedBorder()
        .Expand();

    public static void Execute(string message, Action action)
    {
        try
        {
            AnsiConsole.Status()
                .SpinnerStyle(PrimaryColor).Spinner(Spinner.Known.Dots)
                .Start(message, a =>
                {
                    action();
                });
        }
        catch(Exception ex)
        {
            ShowError(ex);
        }
    }
    public static async Task ExecuteAsync(string message, Func<Task> func)
    {
        try
        {
            await AnsiConsole.Status()
                .SpinnerStyle(PrimaryColor).Spinner(Spinner.Known.Dots)
                .StartAsync(message, async a =>
                {
                    await func();
                    return;
                });
        }
        catch(Exception ex)
        {
            ShowError(ex);
        }
    }
    public static TOut? Execute<TOut>(string message, Func<TOut?> func)
    {
        try
        {
            TOut? output = AnsiConsole.Status()
            .SpinnerStyle(PrimaryColor).Spinner(Spinner.Known.Dots)
            .Start(message, a =>
            {
                return func();
            });
            return output;
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
        return default(TOut);
    }

    public static async Task<TOut?> ExecuteAsync<TOut>(string message, Func<Task<TOut?>> func)
    {        
        try
        {
            TOut? output = await AnsiConsole.Status()
                .SpinnerStyle(PrimaryColor).Spinner(Spinner.Known.Dots)
                .StartAsync(message, async a =>
                {
                    return await func();
                });
            return output;
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
        return default(TOut);
    }    
}



