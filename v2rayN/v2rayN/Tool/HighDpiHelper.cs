using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace v2rayN.Tool
{
    public static class HighDpiHelper
    {
        public const int DefaultDpi = 96;
        private const int CCM_FIRST = 0x2000;
        private const int CCM_DPISCALE = CCM_FIRST + 0x000C;
        private delegate uint GetDpiForWindowDelegate(IntPtr hwnd);

        public static int GetControlDeviceDpi(Control control)
        {
            try
            {
                if (control != null)
                {
                    return control.DeviceDpi;
                }
            }
            catch { }

            return DefaultDpi;
        }

        public static int GetEffectiveDpi(Control control)
        {
            int controlDpi = GetControlDeviceDpi(control);
            int windowDpi = GetWindowDpi(control);
            int graphicsDpi = GetGraphicsDpi(control);
            return ResolveEffectiveDpi(controlDpi, windowDpi, graphicsDpi);
        }

        public static int ResolveEffectiveDpi(int controlDpi, int windowDpi, int graphicsDpi)
        {
            int best = DefaultDpi;

            foreach (int dpi in new[] { windowDpi, controlDpi, graphicsDpi })
            {
                if (dpi > best)
                {
                    best = dpi;
                }
            }

            return best > 0 ? best : DefaultDpi;
        }

        public static int ScaleLogicalValue(int logicalValue, int deviceDpi)
        {
            if (logicalValue <= 0)
            {
                return logicalValue;
            }
            if (deviceDpi <= 0)
            {
                return logicalValue;
            }

            int scaled = (int)Math.Round(logicalValue * deviceDpi / (double)DefaultDpi, MidpointRounding.AwayFromZero);
            return Math.Max(1, scaled);
        }

        public static Size ScaleLogicalSize(Size logicalSize, int deviceDpi)
        {
            return new Size(
                ScaleLogicalValue(logicalSize.Width, deviceDpi),
                ScaleLogicalValue(logicalSize.Height, deviceDpi));
        }

        public static Font NormalizeFontToPoints(Font sourceFont)
        {
            using Font logicalFont = GetLogicalUiFont(sourceFont);
            return new Font(logicalFont.FontFamily, logicalFont.SizeInPoints, logicalFont.Style, GraphicsUnit.Point, logicalFont.GdiCharSet, logicalFont.GdiVerticalFont);
        }

        public static bool AreFontsEquivalent(Font left, Font right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return string.Equals(left.FontFamily.Name, right.FontFamily.Name, StringComparison.OrdinalIgnoreCase)
                   && left.Style == right.Style
                   && left.Unit == right.Unit
                   && Math.Abs(left.SizeInPoints - right.SizeInPoints) < 0.01f
                   && left.GdiCharSet == right.GdiCharSet
                   && left.GdiVerticalFont == right.GdiVerticalFont;
        }

        public static Font GetLogicalUiFont(Font sourceFont)
        {
            if (sourceFont == null)
            {
                return SystemFonts.MessageBoxFont;
            }

            if (ShouldUseSystemUiFont(sourceFont))
            {
                Font systemFont = SystemFonts.MessageBoxFont;
                return new Font(systemFont.FontFamily, systemFont.SizeInPoints, systemFont.Style, GraphicsUnit.Point, systemFont.GdiCharSet, systemFont.GdiVerticalFont);
            }

            if (sourceFont.Unit == GraphicsUnit.Point)
            {
                return new Font(sourceFont.FontFamily, sourceFont.SizeInPoints, sourceFont.Style, GraphicsUnit.Point);
            }

            float sizeInPoints = PixelsToPoints(sourceFont.Size, DefaultDpi);

            return new Font(sourceFont.FontFamily, sizeInPoints, sourceFont.Style, GraphicsUnit.Point);
        }

        public static float PixelsToPoints(float pixelSize, int dpi)
        {
            if (pixelSize <= 0)
            {
                return pixelSize;
            }

            int resolvedDpi = dpi > 0 ? dpi : DefaultDpi;
            return pixelSize * 72f / resolvedDpi;
        }

        public static void EnableListViewDpiScaling(ListView listView)
        {
            try
            {
                if (listView == null || !listView.IsHandleCreated)
                {
                    return;
                }

                SendMessage(listView.Handle, CCM_DPISCALE, new IntPtr(1), IntPtr.Zero);
            }
            catch { }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private static int GetWindowDpi(Control control)
        {
            try
            {
                if (control == null || !control.IsHandleCreated)
                {
                    return 0;
                }

                IntPtr module = GetModuleHandle("user32.dll");
                if (module == IntPtr.Zero)
                {
                    return 0;
                }

                IntPtr proc = GetProcAddress(module, "GetDpiForWindow");
                if (proc == IntPtr.Zero)
                {
                    return 0;
                }

                var getDpiForWindow = (GetDpiForWindowDelegate)Marshal.GetDelegateForFunctionPointer(proc, typeof(GetDpiForWindowDelegate));
                return (int)getDpiForWindow(control.Handle);
            }
            catch
            {
                return 0;
            }
        }

        private static int GetGraphicsDpi(Control control)
        {
            try
            {
                if (control == null)
                {
                    return 0;
                }

                using (Graphics graphics = control.CreateGraphics())
                {
                    return (int)Math.Round(graphics.DpiX, MidpointRounding.AwayFromZero);
                }
            }
            catch
            {
                return 0;
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        private static bool ShouldUseSystemUiFont(Font sourceFont)
        {
            return string.Equals(sourceFont.FontFamily.Name, "Microsoft Sans Serif", StringComparison.OrdinalIgnoreCase);
        }

    }
}
