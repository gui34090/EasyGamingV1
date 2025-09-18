using System;
using System.Drawing;
using System.Runtime.InteropServices;
using EasyGamingV1.Logging;

namespace EasyGamingV1.Overlay;

// Minimal safe desktop-only overlay using a layered window + GDI.
// Default: Safe mode (no WS_EX_TRANSPARENT). Optional anti-capture via SetWindowDisplayAffinity.
public sealed class SafeOverlay : IDisposable
{
    private IntPtr _hwnd;
    private bool _shown;
    private bool _antiCapture;

    public void Show()
    {
        if (_shown) return;
        var screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
        _hwnd = Native.CreateOverlayWindow(screen.Width, screen.Height);
        DrawCrosshair(screen.Width, screen.Height);
        Native.ShowWindow(_hwnd, 8 /*SW_SHOWNA*/);
        _shown = true;
    }

    public void SetAntiCapture(bool enable)
    {
        _antiCapture = enable;
        Native.SetWindowDisplayAffinity(_hwnd, enable ? 0x00000011 /*WDA_EXCLUDEFROMCAPTURE*/ : 0x00000000);
    }

    private void DrawCrosshair(int w, int h)
    {
        using var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.Transparent);
        int cx = w / 2, cy = h / 2;
        int size = Math.Max(10, Math.Min(w, h) / 50);

        using var pen = new Pen(Color.FromArgb(255, 0, 255, 0), 2);
        g.DrawLine(pen, cx - size, cy, cx + size, cy);
        g.DrawLine(pen, cx, cy - size, cx, cy + size);

        Native.UpdateLayer(_hwnd, bmp);
    }

    public void Close()
    {
        if (!_shown) return;
        Native.DestroyWindow(_hwnd);
        _hwnd = IntPtr.Zero;
        _shown = false;
    }

    public void Dispose() => Close();

    private static class Native
    {
        [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Unicode)]
        static extern IntPtr CreateWindowExW(int exStyle, string className, string windowName, int style,
            int x, int y, int w, int h, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);

        [DllImport("user32.dll")] public static extern bool DestroyWindow(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")] public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")] public static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")] public static extern bool DeleteDC(IntPtr hdc);
        [DllImport("gdi32.dll")] public static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);
        [DllImport("gdi32.dll")] public static extern bool DeleteObject(IntPtr h);
        [DllImport("user32.dll", SetLastError=true)]
        public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst,
            ref SIZE psize, IntPtr hdcSrc, ref POINT pptSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

        [DllImport("user32.dll")] public static extern short RegisterClassEx(ref WNDCLASSEX lpwcx);
        [DllImport("user32.dll")] public static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] public static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint affinity);

        const int WS_POPUP = unchecked((int)0x80000000);
        const int WS_VISIBLE = 0x10000000;
        const int WS_EX_TOPMOST = 0x00000008;
        const int WS_EX_TOOLWINDOW = 0x00000080;
        const int WS_EX_LAYERED = 0x00080000;
        const int ULW_ALPHA = 0x00000002;
        const byte AC_SRC_OVER = 0x00;
        const byte AC_SRC_ALPHA = 0x01;

        static ushort _atom;
        static readonly string _className = "EasyGamingV1.SafeOverlay";

        static Native()
        {
            var wc = new WNDCLASSEX
            {
                cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>(),
                lpfnWndProc = DefWindowProc,
                hInstance = ProcessHelper.GetModuleHandle(),
                lpszClassName = _className
            };
            _atom = RegisterClassEx(ref wc);
        }

        public static IntPtr CreateOverlayWindow(int w, int h)
        {
            var hwnd = CreateWindowExW(WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,
                _className, "Overlay", WS_POPUP, 0, 0, w, h, IntPtr.Zero, IntPtr.Zero, ProcessHelper.GetModuleHandle(), IntPtr.Zero);
            return hwnd;
        }

        public static void UpdateLayer(IntPtr hwnd, Bitmap bmp)
        {
            var screenDc = GetDC(IntPtr.Zero);
            var memDc = CreateCompatibleDC(screenDc);
            var hBitmap = bmp.GetHbitmap(System.Drawing.Color.FromArgb(0));
            var old = SelectObject(memDc, hBitmap);

            var size = new SIZE { cx = bmp.Width, cy = bmp.Height };
            var src = new POINT { x = 0, y = 0 };
            var dst = new POINT { x = 0, y = 0 };
            var blend = new BLENDFUNCTION { BlendOp = AC_SRC_OVER, SourceConstantAlpha = 255, AlphaFormat = AC_SRC_ALPHA };

            if (!UpdateLayeredWindow(hwnd, screenDc, ref dst, ref size, memDc, ref src, 0, ref blend, ULW_ALPHA))
            {
                Etw.Log.App_Error("UpdateLayeredWindow failed.");
            }

            SelectObject(memDc, old);
            DeleteObject(hBitmap);
            DeleteDC(memDc);
            ReleaseDC(IntPtr.Zero, screenDc);
        }

        [StructLayout(LayoutKind.Sequential)] public struct POINT { public int x; public int y; }
        [StructLayout(LayoutKind.Sequential)] public struct SIZE { public int cx; public int cy; }
        [StructLayout(LayoutKind.Sequential)] public struct BLENDFUNCTION { public byte BlendOp; public byte BlendFlags; public byte SourceConstantAlpha; public byte AlphaFormat; }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WNDCLASSEX
        {
            public uint cbSize; public uint style; public IntPtr lpfnWndProc; public int cbClsExtra; public int cbWndExtra;
            public IntPtr hInstance; public IntPtr hIcon; public IntPtr hCursor; public IntPtr hbrBackground;
            public string lpszMenuName; public string lpszClassName; public IntPtr hIconSm;
        }
    }
}

internal static class ProcessHelper
{
    [DllImport("kernel32.dll")] private static extern IntPtr GetModuleHandleW(string? lpModuleName);
    public static IntPtr GetModuleHandle() => GetModuleHandleW(null);
}
