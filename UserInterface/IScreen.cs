using System;
using System.Collections.Generic;
using System.Text;

namespace HyRest.OnCmd.UserInterface;


public abstract class Screen : IScreen
{
    private readonly IScreen _returnScreen;
    public Screen(IScreen returnScreen)
    {
        _returnScreen = returnScreen;
    }
    public IScreen ReturnScreen => _returnScreen;
    public MenuResult Return() => _returnScreen.RunScreen();
    public abstract MenuResult RunScreen();
    protected abstract MenuResult RouteChoice<TOption>(TOption choice);
}

public interface IScreen
{
    MenuResult Return();
    MenuResult RunScreen();
}



public class MenuResult<T> : MenuResult
    where T : class
{
    public MenuResult(T result) : base (result)  {  }
    public MenuResult(Exception ex) : base(ex) { }
    public override T? Result => base.TryGetResult<T>(out T result) ? result : default(T);
    public static MenuResult<T> Create(T result) => new MenuResult<T>(result);
    public static MenuResult<T> Create(Exception ex) => new MenuResult<T>(ex);
}

public abstract class MenuResult : IMenuResult
{
    private readonly object? _result;
    public MenuResult(object? result)
    {
        if(result is Exception ex)
            Exception = ex;
        _result = result;
    }
    public virtual object? Result { get; }
    public Exception? Exception { get; }
    protected virtual bool TryGetResult<TResult>(out TResult? result)
    {
        result = default;
        if (_result is TResult success)
        {
            result = success;
            return true;
        }
        else
            return false;
    }
    public static MenuResult<T> Create<T>(T result) where T : class
        => new MenuResult<T>(result);
    public static MenuResult<T> Create<T>(Exception ex) where T : class
        => new MenuResult<T>(ex);
}

public interface IMenuResult
{
    object? Result { get; }
    Exception? Exception { get; }
}


