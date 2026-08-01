using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace HyRest.OnCmd;

public class UI
{
    internal static CanvasImage Logo => new CanvasImage("Ternary-teal.png").MaxWidth(15);
    internal static FigletText Figlet => new FigletText("OnCmd").Color(Color.FromHex("#12d399")).Fitted();
    internal static FigletText MenuText(string text) => new FigletText(text).Color(Color.FromHex("#12d399")).Smushed();
    public static Color PrimaryColor => Color.FromHex("#12d399");
    public static Color SecondaryColor => Color.FromHex("#124cd3");
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
        => new SelectionPrompt<T>().Title(ToStyle(title, color, true)).HighlightStyle(new Style(PrimaryColor));
    public static Panel NewPanel(string header)
        => new Panel("").Header($"[#12d399]{header}[/]").RoundedBorder();
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
}

