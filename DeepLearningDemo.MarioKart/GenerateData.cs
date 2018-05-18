using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using WindowsInput.Native;

namespace DeepLearningDemo.MarioKart
{
    public class GenerateData
    {
        public static int FileCounter = 1;
        public static string trainingFilePath = "training/training_data_{0}.txt";

        public static int ResizeWidth = 100;
        public static int ResizeHeight = 74;
        public static Rectangle rec = new Rectangle() { X = 100, Y = 100, Width = 1300, Height = 1300 };

        public static IObservable<(Bitmap, IEnumerable<VirtualKeyCode>)> CaptureGameData()
        {
            return InterceptKeys
                .CaptureKeys()
                .Buffer(TimeSpan.FromMilliseconds(200))
                .Where(keys => keys.Count > 0)
                .Scan(Enumerable.Empty<(VirtualKeyCode, bool)>(), (acc, keys) => acc.Concat(keys).GroupBy(x => x.Item1).Where(x => x.Select(xx => xx.Item2 ? 1 : -1).Sum() > 0).Select(x => (x.Key, true)))
                .Select(keys => (bitmap: Capture(rec), keys: keys.Where(x => x.Item2).Select(x => x.Item1)))
                .Do(x => CreateTrainingData(x.bitmap, x.keys));
        }

        private static void CreateTrainingData(Bitmap img, IEnumerable<VirtualKeyCode> key)
        {
            var outputLabel = InputKeyToHotVector(key);

            var strBuilder = ProcessImageForTraining(img, outputLabel);

            var strPath = string.Format(trainingFilePath, FileCounter);

            File.AppendAllLines(strPath, new[] { strBuilder.ToString() });
        }

        public static StringBuilder ProcessImageForTraining(Bitmap bitmap, string outputLabel)
        {
            var retValue = ResizeAndCHWExctraction(bitmap);

            var sb = new StringBuilder();

            var outLabel = "|label " + outputLabel;
            sb.Append(outLabel);

            sb.Append("\t|features ");

            for (int i = 0; i < retValue.Count; i++)
            {
                var strVal = retValue[i].ToString(CultureInfo.InvariantCulture);
                if (retValue.Count > i + 1)
                    strVal += " ";

                sb.Append(strVal);
            }
            return sb;
        }

        public static List<float> ResizeAndCHWExctraction(Bitmap bitmap)
        {
            var imageToProcess = ResizeAndGray(bitmap);

            return ImageUtil.ParallelExtractCHW(imageToProcess, true);
        }

        public static Bitmap ResizeAndGray(Bitmap bitmap)
        {
            var img = ImageUtil.Resize(bitmap, ResizeWidth, ResizeHeight, true);

            return ImageUtil.MakeGrayscale(img);
        }

        public static Bitmap Capture(Rectangle Region)
        {
            try
            {
                var bmp = new Bitmap(Region.Width, Region.Height);

                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(Region.Location, Point.Empty, Region.Size, CopyPixelOperation.SourceCopy);
                    g.Flush(System.Drawing.Drawing2D.FlushIntention.Sync);
                }

                return bmp;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public static string InputKeyToHotVector(IEnumerable<VirtualKeyCode> key)
        {
            var output = new string[9];
            if (key.Contains(VirtualKeyCode.VK_W) && key.Contains(VirtualKeyCode.VK_A))
                return "0 0 1 0 0 ";
            else if (key.Contains(VirtualKeyCode.VK_W) && key.Contains(VirtualKeyCode.VK_F))
                return "0 0 0 1 0 ";
            else if (key.Contains(VirtualKeyCode.VK_W))
                return "1 0 0 0 0 ";
            else if (key.Contains(VirtualKeyCode.VK_S))
                return "0 1 0 0 0 ";
            else
                return "0 0 0 0 1 ";

        }
    }
}