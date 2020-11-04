using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using PoeBot.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace PoeBot.Core.Services
{
    class OpenCV_Service
    {
        static Tesseract ocr = null;
        static string ocrDataSetPath = @"E:\Proyectos\PoeBot\PoeBot.Core\Data";

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
        }
        
        public static List<Tuple<Rectangle, string>> GetText(Bitmap source, int threshHold)
        {
            List<Tuple<Rectangle, string>> lMatches = new List<Tuple<Rectangle, string>>();
            int wPosition = 150;
            int hPosition = 370;
            // Detect Bounding boxes of text tabs
            var cp = source.ToImage<Gray, byte>().Copy().ThresholdBinary(new Gray(threshHold), new Gray(255));
            cp.ROI = new Rectangle(wPosition, hPosition, 730, 80);
            Mat M = new Mat();
            CvInvoke.EqualizeHist(cp, M);
            //cp.Save($@"C:\Users\Ruben\Desktop\tests\PoeTests2\{DateTime.Now.Ticks}.png");

            // Sobel
            Image<Gray, byte> sobel = cp.Sobel(1, 0, 3).AbsDiff(new Gray(0.0)).Convert<Gray, byte>();
            Mat SE = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(15, 1), new Point(-1, -1));
            // Dilate
            sobel = sobel.MorphologyEx(MorphOp.Dilate, SE, new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(255));
            // Find contours
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat m = new Mat();
            CvInvoke.FindContours(sobel, contours, m, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            List<Rectangle> boundTexts = new List<Rectangle>();
            for (int i = 0; i < contours.Size; i++)
            {
                Rectangle r = CvInvoke.BoundingRectangle(contours[i]);
                boundTexts.Add(r);
            }

            // Detect all
            InitTesseract(OcrEngineMode.TesseractOnly);
            ocr.PageSegMode = PageSegMode.SparseText;
            foreach (Rectangle rect in boundTexts)
            {
                var r = rect;
                r.X += wPosition - 5;
                r.Y += hPosition - 5;
                r.Width += 10;
                r.Height += 10;
                cp.ROI = r;
                //cp.Save($@"C:\Users\Ruben\Desktop\tests\PoeTests2\{DateTime.Now.Ticks}.png");
                ocr.SetImage(cp.Copy());
                string txt = ocr.GetUTF8Text().Replace("\r\n", "");
                if (!String.IsNullOrWhiteSpace(txt))
                {
                    lMatches.Add(new Tuple<Rectangle, string>(r, txt));
                }
            }

            // Detect only numbers
            InitTesseract(OcrEngineMode.TesseractOnly, whiteList: "1234567890");
            ocr.PageSegMode = PageSegMode.SingleChar;
            foreach (Rectangle rect in boundTexts)
            {
                var r = rect;
                r.X += wPosition - 5;
                r.Y += hPosition - 5;
                r.Width += 10;
                r.Height += 10;
                cp.ROI = r;
                //cp.Save($@"C:\Users\Ruben\Desktop\tests\PoeTests2\{DateTime.Now.Ticks}.png");
                ocr.SetImage(cp.Copy());
                string txt = ocr.GetUTF8Text().Replace("\r\n", "");
                if (!String.IsNullOrWhiteSpace(txt))
                {
                    lMatches.Add(new Tuple<Rectangle, string>(r, txt));
                }
            }

            // Quitar los 

            return lMatches;
        }
        public static Position ContainsText(Bitmap source,string textToFind,int threshHold = 100)
        {
            int wPosition = 150;
            int hPosition = 370;
            // Detect Bounding boxes of text tabs
            var cp = source.ToImage<Gray, byte>().Copy().ThresholdBinary(new Gray(threshHold),new Gray(255));
            cp.ROI = new Rectangle(wPosition, hPosition, 730, 80);
            Mat M = new Mat();
            CvInvoke.EqualizeHist(cp, M);
            cp.Save($@"C:\Users\Ruben\Desktop\tests\PoeTests2\{DateTime.Now.Ticks}.png");

            // Sobel
            Image<Gray, byte> sobel = cp.Sobel(1, 0, 3).AbsDiff(new Gray(0.0)).Convert<Gray, byte>();
            Mat SE = CvInvoke.GetStructuringElement(ElementShape.Rectangle,new Size(15,1),new Point(-1,-1));
            // Dilate
            sobel = sobel.MorphologyEx( MorphOp.Dilate,SE,new Point(-1,-1),1,BorderType.Reflect,new MCvScalar(255));
            // Find contours
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat m = new Mat();
            CvInvoke.FindContours(sobel,contours,m,RetrType.External, ChainApproxMethod.ChainApproxSimple);
            List<Rectangle> boundTexts = new List<Rectangle>();
            for (int i =0; i<contours.Size;i++)
            {
                Rectangle r = CvInvoke.BoundingRectangle(contours[i]);
                boundTexts.Add(r);
            }

            // Detect all
            InitTesseract(OcrEngineMode.TesseractOnly);
            ocr.PageSegMode = PageSegMode.SparseText;
            List<Tuple<Rectangle, string>> lMatches = new List<Tuple<Rectangle, string>>();
            foreach (Rectangle rect in boundTexts)
            {
                var r = rect;
                r.X += wPosition-5;
                r.Y += hPosition-5;
                r.Width += 10;
                r.Height += 10;
                cp.ROI = r;
                cp.Save($@"C:\Users\Ruben\Desktop\tests\PoeTests2\{DateTime.Now.Ticks}.png");
                ocr.SetImage(cp.Copy());
                string txt = ocr.GetUTF8Text().Replace("\r\n","");
                if (!String.IsNullOrWhiteSpace(txt))
                {
                    lMatches.Add(new Tuple<Rectangle, string>(r,txt));
                }
            }

            // Detect only numbers
            InitTesseract(OcrEngineMode.TesseractOnly,whiteList:"1234567890");
            ocr.PageSegMode = PageSegMode.SingleChar;
            foreach (Rectangle rect in boundTexts)
            {
                var r = rect;
                r.X += wPosition - 5;
                r.Y += hPosition - 5;
                r.Width += 10;
                r.Height += 10;
                cp.ROI = r;
                cp.Save($@"C:\Users\Ruben\Desktop\tests\PoeTests2\{DateTime.Now.Ticks}.png");
                ocr.SetImage(cp.Copy());
                string txt = ocr.GetUTF8Text().Replace("\r\n", "");
                if (!String.IsNullOrWhiteSpace(txt))
                {
                    lMatches.Add(new Tuple<Rectangle, string>(r, txt));
                }
            }

            return lMatches.Where(e => e.Item2 == textToFind);
        }


        private static void InitTesseract(OcrEngineMode mode = OcrEngineMode.Default,string whiteList = null)
        {
            if (ocr == null || ocr.Oem != mode || whiteList != null)
            {
                ocr = new Tesseract(ocrDataSetPath, "eng", mode,whiteList);
                ocr.PageSegMode = PageSegMode.SingleLine;
            }
        }
    }
}
