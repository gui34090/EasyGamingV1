using Microsoft.UI.Xaml;
using System;
using Microsoft.WindowsAppRuntime.Bootstrap;
using EasyGamingV1.Logging;

namespace EasyGamingV1;

public partial class App : Application
{
    private Window? _window;

    public App()
    {
        this.InitializeComponent();
        // Init ETW early
        Etw.Log.App_Started();
        // Initialize Windows App Runtime (bootstrap if missing)
        try
        {
            Bootstrap.Initialize();
        }
        catch (Exception ex)
        {
            Etw.Log.App_Error("Bootstrap.Initialize failed: " + ex.Message);
        }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
