using Emgu.CV;
using Emgu.CV.Structure;
using PoeBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoeBot.Core.Services
{
    class OpenCV_Service
    {
        public OpenCV_Service()
        {

        }

        public static bool Match(Bitmap source, Bitmap template, float treshHold = 0.85f)
        {
            Image<Gray, byte> sourceImage = source.ToImage<Gray, byte>(); // Image B
            Image<Gray, byte> templateImage = template.ToImage<Gray, byte>(); // Image A

            //sourceImage.ToBitmap().Save($@"C:\Users\Ruben\Desktop\tests\PoeTests2\{DateTime.Now.Ticks}.png");
            //templateImage.ToBitmap().Save($@"C:\Users\Ruben\Desktop\tests\PoeTests2\{DateTime.Now.Ticks}.png");
            using (Image<Gray, float> result = sourceImage.MatchTemplate(templateImage, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;

                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.

                if (maxValues[0] > treshHold)
                {
                    //sourceImage.ToBitmap().Save($@"C:\Users\Ruben\Desktop\tests\PoeTests2\{DateTime.Now.Ticks}.png");
                    //templateImage.ToBitmap().Save($@"C:\Users\Ruben\Desktop\tests\PoeTests2\{DateTime.Now.Ticks}.png");
                    //result.ToBitmap().Save($@"C:\Users\Ruben\Desktop\tests\PoeTests2\{DateTime.Now.Ticks}.png");
                    return true;
                }
            }
            return false;
        }

        public static List<Position> FindObjects(Bitmap source_img, string path_template)
        {
            List<Position> res_pos = new List<Position>();

            Image<Bgr, byte> source = source_img.ToImage<Bgr, byte>(); // Image B
            Image<Bgr, byte> template = new Image<Bgr, byte>(path_template); // Image A

            using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                while (true)
                {
                    double[] minValues, maxValues;
                    Point[] minLocations, maxLocations;

                    result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                    // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.

                    if (maxValues[0] > 0.85)
                    {
                        // This is a match. Do something with it, for example draw a rectangle around it.
                        Rectangle match = new Rectangle(maxLocations[0], template.Size);
                        result.Draw(match, new Gray(), 3);

                        res_pos.Add(new Position
                        {
                            Left = maxLocations[0].X,
                            Top = maxLocations[0].Y,
                            Width = template.Size.Width,
                            Height = template.Size.Height
                        });
                    }
                    else
                        break;
                }
                result.ToBitmap().Save(@"C:\Users\Ruben\Desktop\tests\test\" + DateTime.Now.ToFileTime() + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }

            return res_pos;
        }

        public static Position FindObject(Bitmap source_img, string path_template)
        {
            Position res = null;

            Image<Bgr, byte> source = source_img.ToImage<Bgr, byte>(); // Image B
            Image<Bgr, byte> template = new Image<Bgr, byte>(path_template); // Image A

            using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;

                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.

                if (maxValues[0] > 0.85)
                {
                    res = new Position
                    {
                        Left = maxLocations[0].X,
                        Top = maxLocations[0].Y,
                        Width = template.Size.Width,
                        Height = template.Size.Height
                    };
                }
                var bmap = result.ToBitmap();
                string fName = DateTime.Now.Ticks + ".png";
                result.ToBitmap().Save(@"C:\Users\Ruben\Desktop\tests\test\"+fName,System.Drawing.Imaging.ImageFormat.Png);

                source.Dispose();
                template.Dispose();
                result.Dispose();
            }

            return res;
        }

        public static Position FindObject(Bitmap source_img, string path_template, double trashholder)
        {
            Position res = new Position();

            Image<Bgr, byte> source = source_img.ToImage<Bgr, byte>(); // Image B
            Image<Bgr, byte> template = new Image<Bgr, byte>(path_template); // Image A

            using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;

                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.

                if (maxValues[0] > trashholder)
                {
                    res = new Position
                    {
                        Left = maxLocations[0].X,
                        Top = maxLocations[0].Y,
                        Width = template.Size.Width,
                        Height = template.Size.Height
                    };
                }
                result.ToBitmap().Save(@"C:\Users\Ruben\Desktop\tests\test" + DateTime.Now.Ticks + ".png");

                source.Dispose();
                template.Dispose();
                result.Dispose();
            }

            return res;
        }

        public static List<Position> FindObjects(Bitmap source_img, string path_template, double trashholder)
        {
            List<Position> res_pos = new List<Position>();

            Image<Bgr, byte> source = source_img.ToImage<Bgr, byte>(); // Image B
            Image<Bgr, byte> template = new Image<Bgr, byte>(path_template); // Image A

            using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                while (true)
                {
                    double[] minValues, maxValues;
                    Point[] minLocations, maxLocations;

                    result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                    // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.

                    if (maxValues[0] > trashholder)
                    {
                        // This is a match. Do something with it, for example draw a rectangle around it.
                        Rectangle match = new Rectangle(maxLocations[0], template.Size);
                        result.Draw(match, new Gray(), 3);

                        res_pos.Add(new Position
                        {
                            Left = maxLocations[0].X,
                            Top = maxLocations[0].Y,
                            Width = template.Size.Width,
                            Height = template.Size.Height
                        });
                    }
                    else
                        break;
                }
                result.Save(@"C:\Users\Ruben\Desktop\tests\test" + DateTime.Now.ToShortDateString() + ".png");
            }

            return res_pos;
        }

        internal static List<Position> FindCurrencies(Bitmap source_img, string path_template, double trashholder)
        {
            List<Position> res_pos = new List<Position>();

            Image<Bgr, byte> source = source_img.ToImage<Bgr, byte>();// Image B
            Image<Bgr, byte> template = new Image<Bgr, byte>(path_template); // Image A

            template = template.Resize(33, 33, Emgu.CV.CvEnum.Inter.Cubic);
            template.ROI = new Rectangle(0, 11, 33, 24);

            using (Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                while (true)
                {
                    double[] minValues, maxValues;
                    Point[] minLocations, maxLocations;

                    result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                    // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.

                    if (maxValues[0] > trashholder)
                    {
                        // This is a match. Do something with it, for example draw a rectangle around it.
                        Rectangle match = new Rectangle(maxLocations[0], template.Size);
                        result.Draw(match, new Gray(), 3);

                        res_pos.Add(new Position
                        {
                            Left = maxLocations[0].X,
                            Top = maxLocations[0].Y,
                            Width = template.Size.Width,
                            Height = template.Size.Height
                        });
                    }
                    else
                        break;
                }
                result.Save(@"C:\Users\Ruben\Desktop\tests\test" + DateTime.Now.ToLongDateString() + ".png");
            }

            return res_pos;        
        }

        public static bool InColorRange(Bitmap source,Bgr low,Bgr high)
        {
            var img = source.ToImage<Bgr, byte>();
            img.Save($@"C:\Users\Ruben\Desktop\tests\PoeTests2\{DateTime.Now.Ticks}.png");
            img._SmoothGaussian(5);

            using (Image<Gray,byte> mask = img.InRange(low, high).Not())
            {
                double[] minValues, maxValues;
                Point[] minLocs, maxLocs;
                mask.MinMax(out minValues, out maxValues, out minLocs, out maxLocs);
                mask.Save($@"C:\Users\Ruben\Desktop\tests\PoeTests2\{DateTime.Now.Ticks}.png");
                return maxValues[0] > 200;
            }
            return false;
        }
    }
}
