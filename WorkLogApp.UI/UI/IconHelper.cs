using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace WorkLogApp.UI.UI
{
    public static class IconHelper
    {
        private static Icon _appIcon;

        public static Icon GetAppIcon()
        {
            if (_appIcon != null) return _appIcon;

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                // Resource name matches DefaultNamespace.Path.Filename
                // Since app.ico is in root, it should be WorkLogApp.UI.app.ico
                using (var stream = assembly.GetManifestResourceStream("WorkLogApp.UI.app.ico"))
                {
                    if (stream != null)
                    {
                        _appIcon = new Icon(stream);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Icon resource not found: WorkLogApp.UI.app.ico");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to load icon: " + ex.Message);
            }

            return _appIcon;
        }

        public static void ApplyIcon(Form form)
        {
            if (form == null) return;
            
            var icon = GetAppIcon();
            if (icon != null)
            {
                form.Icon = icon;
            }
        }
    }
}
