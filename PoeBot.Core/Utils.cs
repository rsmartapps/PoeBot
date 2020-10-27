using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
