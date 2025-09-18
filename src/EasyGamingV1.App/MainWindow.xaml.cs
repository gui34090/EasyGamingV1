using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using EasyGamingV1.Overlay;
using EasyGamingV1.Services;
using EasyGamingV1.Logging;
using EasyGamingV1.Core;

namespace EasyGamingV1;

public sealed partial class MainWindow : Window
{
    private SafeOverlay? _overlay;
    private bool _antiCapture;

    public MainWindow()
    {
        this.InitializeComponent();
        Title = "EasyGamingV1 (Portable)";
        RawInput.AttachToWindow(this);
        Status("Ready.");
    }

    private void Status(string msg) { StatusText.Text = msg; Etw.Log.App_Info(msg); }

    private void ToggleOverlayBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_overlay == null)
        {
            _overlay = new SafeOverlay();
            _overlay.Show();
            Status("Overlay: ON (Safe Mode).");
        }
        else
        {
            _overlay.Close();
            _overlay = null;
            Status("Overlay: OFF.");
        }
    }

    private void AntiCaptureBtn_Click(object sender, RoutedEventArgs e)
    {
        _antiCapture = !_antiCapture;
        _overlay?.SetAntiCapture(_antiCapture);
        (sender as Button)!.Content = $"Anti-capture: {(_antiCapture ? "On" : "Off")}";
    }

    private async void OpenLogsBtn_Click(object sender, RoutedEventArgs e)
    {
        var path = Paths.LogsFolder;
        Directory.CreateDirectory(path);
        try { Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true }); }
        catch { await new ContentDialog{Title="Info", Content=path, PrimaryButtonText="OK", XamlRoot = this.Content.XamlRoot}.ShowAsync(); }
    }

    private async void ValidateDataBtn_Click(object sender, RoutedEventArgs e)
    {
        bool ok = await DataValidator.ValidateAsync();
        Status(ok ? "Data OK." : "Data invalid.");
    }

    private void ComputeCm360_Click(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(DpiBox.Text, out var dpi) && double.TryParse(SensBox.Text, out var sens))
        {
            var cm = Converter.CmPer360(dpi, sens);
            Cm360Out.Text = $"{cm:F2} cm/360°";
        }
    }
}

internal static class RawInput
{
    private static delegate* unmanaged[Stdcall]<HWND, uint, nuint, nint, nint> _oldWndProc;
    private static WNDPROC? _newProc;
    private static HWND _hwnd;

    public static void AttachToWindow(Window window)
    {
        _hwnd = new HWND(WinRT.Interop.WindowNative.GetWindowHandle(window));
        RegisterMouse(_hwnd);
        _newProc = WndProc;
        _oldWndProc = (delegate* unmanaged[Stdcall]<HWND, uint, nuint, nint, nint>)Windows.Win32.PInvoke.SetWindowLongPtr(_hwnd, WINDOW_LONG_PTR_INDEX.GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newProc));
    }

    private static unsafe nint WndProc(HWND hwnd, uint msg, nuint wParam, nint lParam)
    {
        if (msg == (uint)WINDOW_MESSAGE.WM_INPUT)
        {
            // Minimal sample: acknowledge Raw Input traffic
        }
        return Windows.Win32.PInvoke.CallWindowProc(new Windows.Win32.UI.WindowsAndMessaging.WNDPROC(_oldWndProc), hwnd, msg, wParam, lParam);
    }

    private static unsafe void RegisterMouse(HWND hwnd)
    {
        var rid = new Windows.Win32.UI.Input.KeyboardAndMouse.RAWINPUTDEVICE
        {
            usUsagePage = 0x01,
            usUsage = 0x02, // mouse
            dwFlags = 0x00000000, // RIDEV_INPUTSINK optional
            hwndTarget = hwnd
        };
        if (!Windows.Win32.PInvoke.RegisterRawInputDevices(new[] { rid }, 1, (uint)Marshal.SizeOf<Windows.Win32.UI.Input.KeyboardAndMouse.RAWINPUTDEVICE>()))
        {
            Etw.Log.App_Error("RegisterRawInputDevices failed.");
        }
    }
}
