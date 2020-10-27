using Emgu.CV;
using Emgu.CV.Structure;
using PoeBot.Core.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoeBot.Core
{
    public class Tests
    {
        public static void FindAcceptIcon()
        {
            try
            {
                Bitmap template = new Bitmap("Assets/UI_Fragments_free/accept_tradewindow.png"); // Image A

                // Load image
                Bitmap screen = new Bitmap($"Assets/UI_Fragments_free/Trade-Test2.png");
                var ticks = DateTime.Now.Ticks;
                if (!OpenCV_Service.Match(screen, template, 0.80f))
                {
                    screen.Save($@"C:\Users\Ruben\Desktop\tests\PoeTests\{ticks}.png");
                }
            }
            catch (Exception ex)
            {

            }
        }
        public static void GetCurrencies()
        {
            try
            {
                int widht = 38;
                int heigth = 38;

                Bitmap template = new Bitmap("Assets/UI_Fragments_free/empty_cel2.png"); // Image A

                // Load image
                Bitmap screen = new Bitmap($"Assets/UI_Fragments_free/Trade-Test2.png");
                var ticks = DateTime.Now.Ticks;
                for (int i = 0; i < 12; i++)
                {
                    for(int j = 0; j < 5; j++)
                    {
                        Bitmap source = CropImage(screen, new Rectangle { X = 221+(widht*i)-i/2, Y = 145 + (heigth * j)-j/2, Width = widht, Height = heigth });
                        if(!OpenCV_Service.Match(source, template,0.80f))
                        {
                            source.Save($@"C:\Users\Ruben\Desktop\tests\PoeTests\{ticks}-{i}-{j}.png");
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        public static void TradeisGreen()
        {
            Bitmap src = new Bitmap("Assets/UI_Fragments_free/trade_test_ok.jpg");
            Bgr low = new Bgr(12,40,0);
            Bgr high = new Bgr(50,70,41);
            OpenCV_Service.InColorRange(src, low, high);
        }
        private static Bitmap CropImage(Bitmap src, Rectangle cropRect)
        {
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }
            return target;
        }
    }
}
