using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Forms;

namespace PictureViewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            if (CheckRegistry())
            {
                AddRegistry();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length == 0)
            {
                Application.Run(new Main());
            }
            else
            {
                Application.Run(new Main(args[0]));
            }
        }

        public static bool CheckRegistry()
        {
            return Registry.CurrentUser.OpenSubKey(@"Software\Classes\Applications\PictureViewer.exe") == null;
        }

        public static void AddRegistry()
        {
            string executePath = Application.ExecutablePath;
            string installFolder = Path.GetDirectoryName(executePath);
            string icoPath = string.Format(@"{0}\pictureviewer.ico", installFolder);

            RegistryKey appReg = Registry.CurrentUser.CreateSubKey(@"Software\Classes\Applications\PictureViewer.exe");
            appReg.CreateSubKey(@"shell\open\command").SetValue("", "\"" + Application.ExecutablePath + "\" \"%1\"");
            appReg.CreateSubKey(@"shell\edit\command").SetValue("", "\"" + Application.ExecutablePath + "\" \"%1\"");
            appReg.CreateSubKey("DefaultIcon").SetValue("", icoPath);
            appReg.CreateSubKey("PhotoshopLocation").SetValue("path", "null");

            var jpgReg = Registry.CurrentUser.OpenSubKey(@"Software\Classes\.jpg\DefaultIcon", true);
            if (jpgReg == null)
            {
                jpgReg = Registry.CurrentUser.CreateSubKey(@"Software\Classes\.jpg\DefaultIcon");
            }
            jpgReg.SetValue("", icoPath);

            var bmpReg = Registry.CurrentUser.OpenSubKey(@"Software\Classes\.bmp\DefaultIcon", true);
            if (bmpReg == null)
            {
                bmpReg = Registry.CurrentUser.CreateSubKey(@"Software\Classes\.bmp\DefaultIcon");
            }
            bmpReg.SetValue("", icoPath);

            var pngReg = Registry.CurrentUser.OpenSubKey(@"Software\Classes\.png\DefaultIcon", true);
            if (pngReg == null)
            {
                pngReg = Registry.CurrentUser.CreateSubKey(@"Software\Classes\.png\DefaultIcon");
            }
            pngReg.SetValue("", icoPath);
        }
    }
}
