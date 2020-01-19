using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace ClientDesktopUI
{
    static class WindowsHelper
    {
        [DllImport("user32")]
        private static extern int FlashWindow(IntPtr hwnd, bool bInvert);

        [DllImport("user32")]
        private static extern IntPtr GetForegroundWindow();

        private static bool isActive(Window window)
        {
            if(window == null)
            {
                return false;
            }

            return GetForegroundWindow() == new WindowInteropHelper(window).Handle;
        }

        public static bool IsApplicationActive()
        {
            foreach (var window in Application.Current.Windows.OfType<Window>())
            {
                if (isActive(window))
                {
                    return true;
                }
            }

            return false;
        }
        public static void FlashWindow(Window window)
        {
            WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
            FlashWindow(windowInteropHelper.Handle, true);
        }
    }
}
