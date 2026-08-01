using Microsoft.Extensions.Logging;
using Color = Spectre.Console.Color;
using console = Spectre.Console.AnsiConsole;

namespace HyRest.OnCmd;

public abstract class BaseCommand
{    
    public void LogEx(Exception ex)
    {
        console.WriteException(ex, new ExceptionSettings
        {
            Format = ExceptionFormats.Default,
            Style = new ExceptionStyle
            {
                Exception = new Style(Color.Grey),
                Message = new Style(Color.White),
                Method = new Style(Color.Red),
                Path = new Style(Color.Yellow),
                LineNumber = new Style(Color.Blue),
            }
        });
    }
    public void Execute(string message, Action action)
    {
        console.Status()
                .SpinnerStyle(new Style(Color.FromHex("#12d399"))).Spinner(Spinner.Known.Dots)
                .Start(message, a =>
                {
                    action.Invoke();
                });
    }
    public void Execute<TIn>(string message, TIn input, Action<TIn> action)
    {
        console.Status()
                .SpinnerStyle(new Style(Color.FromHex("#12d399"))).Spinner(Spinner.Known.Dots)
                .Start(message, a =>
                {
                    action.Invoke(input);
                });
    }
    public void ExecuteContext(string message, Action<StatusContext> func)
    {
        console.Status()
                .SpinnerStyle(new Style(Color.FromHex("#12d399"))).Spinner(Spinner.Known.Dots)
                .Start(message, async a =>
                {
                    func.Invoke(a);
                });
    }
    public async Task ExecuteContextAsync(string message, Func<StatusContext, Task> func)
    {
        await console.Status()
               .SpinnerStyle(new Style(Color.FromHex("#12d399"))).Spinner(Spinner.Known.Dots)
               .StartAsync(message, async a =>
               {
                   await func.Invoke(a);
               });
    }
    public TOut Execute<TIn, TOut>(string message, TIn input, Func<TIn, TOut> func)
    {
        TOut output = console.Status()
                .SpinnerStyle(new Style(Color.FromHex("#12d399"))).Spinner(Spinner.Known.Dots)
                .Start(message, a =>
                {
                    return func.Invoke(input);
                });
        if (output != null)
            return output;
        else
            throw new Exception("No Output was returned from the task.");
    }

    public async Task<TOut> ExecuteAsync<TIn, TOut>(string message, TIn input, Func<TIn, Task<TOut>> func)
    {
        var output = await console.Status()
               .SpinnerStyle(new Style(Color.FromHex("#12d399"))).Spinner(Spinner.Known.Dots)
               .StartAsync(message, async a =>
               {
                   return await func.Invoke(input);
               });
        if (output != null)
            return output;
        else
            throw new Exception("No Output was returned from the task.");
    }
    public TOut Execute<TOut>(string message, Func<TOut> func)
    {        
        TOut output = console.Status()
            .SpinnerStyle(new Style(Color.FromHex("#12d399"))).Spinner(Spinner.Known.Dots)
            .Start(message, a =>
            {
                return func.Invoke();
            });
        if (output != null)
            return output;
        else
            throw new Exception("No Output was returned from the task.");
    }
    public async Task<TOut> ExecuteAsync<TOut>(string message, Func<Task<TOut>> func)
    {
        var output = await console.Status()
                .SpinnerStyle(new Style(Color.FromHex("#12d399"))).Spinner(Spinner.Known.Dots)
                .StartAsync(message, async a =>
                {
                    return await func.Invoke();
                });
        if (output != null)
            return output;
        else
            throw new Exception("No Output was returned from the task.");
    }

    public void Log(string message, LogLevel level = LogLevel.Information, bool newLine = true)
    {
        if (newLine)
            console.WriteLine();
        console.Write(new LogPill(message, level));
        if (newLine)
            console.WriteLine();
    }
    public void Log(string message, TaskStatus status, bool newLine = true)
    {
        if (newLine)
            console.WriteLine();
        console.Write(new TaskStatusPill(message, status));
        if (newLine)
            console.WriteLine();
    }
}