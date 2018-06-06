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
        public static Rectangle rec = new Rectangle() { X = 200, Y = 300, Width = 600, Height = 400 };

        public static IObservable<(Bitmap, Bitmap, IEnumerable<VirtualKeyCode>)> CaptureGameData()
        {
            return InterceptKeys
                .CaptureKeys()
                .Buffer(TimeSpan.FromMilliseconds(100))
                .Scan(Enumerable.Empty<(VirtualKeyCode, bool)>(), (acc, keys) => acc.Concat(keys).GroupBy(x => x.Item1).Where(x => x.Select(xx => xx.Item2 ? 1 : -1).Sum() > 0).Select(x => ((Key: x.Key, true))))
                .Select(keys => MergeKeys(keys))
                .Where(keys => keys.keys.Any())
                .Do(x => CreateTrainingData(x.grayBitmap, x.keys));
        }

        private static (Bitmap bitmap, Bitmap grayBitmap, IEnumerable<VirtualKeyCode> keys) MergeKeys(IEnumerable<(VirtualKeyCode, bool)> keys)
        {
            var bitmap = Capture(rec);
            var mergedKeys = keys.Where(x => x.Item2).Select(x => x.Item1);
            return (bitmap, ResizeAndGray(bitmap), mergedKeys);
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
            var retValue = ImageUtil.ParallelExtractCHW(bitmap, true);

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
            if (key.Contains(VirtualKeyCode.VK_X) && key.Contains(VirtualKeyCode.LEFT))
                return "0 0 1 0 0 ";
            else if (key.Contains(VirtualKeyCode.VK_X) && key.Contains(VirtualKeyCode.RIGHT))
                return "0 0 0 1 0 ";
            else if (key.Contains(VirtualKeyCode.VK_X))
                return "1 0 0 0 0 ";
            else if (key.Contains(VirtualKeyCode.VK_C))
                return "0 1 0 0 0 ";
            else
                return "0 0 0 0 1 ";

        }
    }
}