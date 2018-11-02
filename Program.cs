using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Globalization;
using System.IO;

namespace WallpaperDynamicApp
{

    class Program
    {
        public enum SPIF
        {
            None = 0x00,
            /// <summary>Writes the new system-wide parameter setting to the user profile.</summary>
            SPIF_UPDATEINIFILE = 0x01,
            /// <summary>Broadcasts the WM_SETTINGCHANGE message after updating the user profile.</summary>
            SPIF_SENDCHANGE = 0x02,
            /// <summary>Same as SPIF_SENDCHANGE.</summary>
            SPIF_SENDWININICHANGE = 0x02
        }

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);
        private delegate bool EventHandler();
        static EventHandler _handler;


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern Int32 SystemParametersInfo(UInt32 uiAction, UInt32 uiParam, String pvParam, UInt32 fWinIni);
        private static readonly UInt32 SPI_SETDESKWALLPAPER = 20;
        private static readonly UInt32 SPIF_UPDATEINIFILE = 0x1;
        private static readonly String imageFileName =  Path.GetTempPath()+ "test.jpg";
        private static String defaultWallpaper;
        private static void DisplayPicture(string file_name)
        {
            // Set the desktop background to this file.
            int status = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, file_name, SPIF_UPDATEINIFILE);
        }
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, StringBuilder pvParam, SPIF fWinIni);

        public static string GetPathOfWallpaper()
        {
            const uint SPI_GETDESKWALLPAPER = 0x0073;
            StringBuilder sb = new StringBuilder(500);
            if (!SystemParametersInfo(SPI_GETDESKWALLPAPER, (uint)sb.Capacity, sb, (uint)SPIF.None))
                return "";
            return sb.ToString();
        }
                 
        private static bool OnProcessExiting()
        {
            DisplayPicture(defaultWallpaper);
            Console.WriteLine("Process is exiting, bye.");
            return true;
        }


        static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var screen = Screen.PrimaryScreen;
            Rectangle bounds = screen.Bounds;
            Bitmap result;

            //on exit event for default wallpaper setup
            _handler += OnProcessExiting;
            SetConsoleCtrlHandler(_handler, true);

            defaultWallpaper = GetPathOfWallpaper();
           // string fullPath; 
            while (true)
            {
                using (result = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(result))
                    {
                        g.CopyFromScreen(Screen.PrimaryScreen.Bounds.Location, Point.Empty, bounds.Size);
                    }
                    var gaussianBlur = new SuperfastBlur.GaussianBlur(result);
                    var final_result = gaussianBlur.Process(20);
                    final_result.Save(Path.GetTempPath()+"test.jpg", ImageFormat.Jpeg);
                    
                }
               // fullPath = Path.GetFullPath(imageFileName);
                DisplayPicture(imageFileName);
                Thread.Sleep(300);
            }
        }
    }
}
