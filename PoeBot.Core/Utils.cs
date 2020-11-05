using PoeBot.Core.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PoeBot.Core.Services.Win32;

namespace PoeBot.Core
{
    public static class Utils
    {
        public static double GetNumber(int begin, string target)
        {
            double result = 0;
            string buf = string.Empty;

            for (int i = begin; i < begin + 5; i++)
            {
                if (target[i] != ' ' && target[i] != ')')
                {
                    if (target[i] != ',')
                        buf += target[i];
                    else buf += '.';
                }
                else
                {
                    begin = i + 1;
                    break;
                }
            }

            return result = Convert.ToDouble(buf);
        }
        public static Bitmap CropImage(Bitmap src, Rectangle cropRect)
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

        private static int DefaultHeight = 1080;
        private static int DefaultWidth = 1920;
        public static Rectangle StashRectangle = new Rectangle(20, 125, 630, 630);
        public static Rectangle TradeRectangle = new Rectangle(310, 205, 630, 265);
        public static Rectangle InventoryRectangle = new Rectangle(1275, 585, 630, 265);

        public static Point ZeroStash()
        {
            return ZeroPoint(StashRectangle.X, StashRectangle.Y);
        }
        public static Point ZeroInventory()
        {
            return ZeroPoint(InventoryRectangle.X, InventoryRectangle.Y);
        }
        public static Point ZeroTrade()
        {
            return ZeroPoint(TradeRectangle.X, TradeRectangle.Y);
        }
        public static int WidthHeightTab()
        {
            return 38;
        }
        private static Point ZeroPoint(int X,int Y)
        {
            Point p = new Point();
            p.X = 200;
            p.Y = 200;
            var rect = Win32.GetWindowRectangle();
            if (rect.Width == DefaultWidth && rect.Height == DefaultHeight)
                return p;
            else
            {
                p.X = (p.X * rect.Width) / DefaultWidth;
                p.Y = (p.Y * rect.Height) / DefaultHeight;
                return p;
            }
        }
    }
}
