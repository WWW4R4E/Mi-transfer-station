using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Mibar
{
    public static class ThemeHelper
    {
        public static bool IsDarkModeEnabled()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                if (key == null) return false;

                var appsUseLightTheme = key.GetValue("AppsUseLightTheme");
                return appsUseLightTheme == null || (int)appsUseLightTheme == 0;
            }
        }

        public static Color GetSystemAccentColor()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM"))
            {
                if (key == null) return Colors.Transparent;

                var colorizationColor = key.GetValue("ColorizationColor");
                if (colorizationColor == null) return Colors.Transparent;

                int colorValue = (int)colorizationColor;
                return Color.FromArgb(
                    (byte)(colorValue >> 24),
                    (byte)(colorValue >> 16),
                    (byte)(colorValue >> 8),
                    (byte)colorValue
                );
            }
        }
    }
}
