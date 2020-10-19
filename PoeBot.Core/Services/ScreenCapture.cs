using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PoeBot.Core.Services
{
    /// <summary>
    /// Provides functions to capture the entire screen, or a particular window, and save it to a file.
    /// </summary>
    public class ScreenCapture
    {

        public static Bitmap CaptureRectangle(int x, int y, int rec_width, int rec_height)
        {
            Rectangle rect = new Rectangle(x, y, rec_width, rec_height);
            Bitmap bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppRgb);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            bmp.Save(@"C:\Users\Ruben\Desktop\tests\test\" + DateTime.Now.Ticks + ".png", ImageFormat.Png);

            g.Dispose();

            return bmp;
        }
        public static Bitmap CaptureScreen()
        {
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppRgb);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
            bmp.Save(@"C:\Users\Ruben\Desktop\tests\test\" + DateTime.Now.Ticks + ".png", ImageFormat.Png);

            g.Dispose();

            return bmp;
        }
    }
}