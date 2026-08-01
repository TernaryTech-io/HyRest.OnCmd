using DotNetEnv;
using HyRest.OnCmd.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using HyRest.Identity.Credentials;

namespace HyRest.OnCmd;

internal partial class UserInterface : BaseCommand
{
    private readonly OnCmdAppBuilder _builder;
    private OnCmdApp _onCmd { get; set; }
    public UserInterface(OnCmdAppBuilder builder)
    {
        _builder = builder;
    }
    public OnCmdApp OnCmd => _onCmd;
    public OnBaseApp App => _onCmd.App;
    public async Task InitLogin()
    {
        var selection = LoginScreen.Init();
        if (selection == LoginOption.Login)
        {
            var (username, password) = LoginScreen.Login();
            _onCmd = _builder.Build(username, password);
            try
            {
                await App.AuthenticateAsync();
                MainMenu.LoadMainMenu();
            }
            catch(Exception ex)
            {
                Log("Failed to Authenticate", LogLevel.Error);
                LogEx(ex);
            }
        }
        else
            LoginScreen.Exit();
    }
    public static async Task Start()
    {
        var ui = new UserInterface(OnCmdAppBuilder.Create());
        await ui.InitLogin();
    }
}

