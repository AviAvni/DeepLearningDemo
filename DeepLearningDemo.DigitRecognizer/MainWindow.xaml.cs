using LiveCharts;
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static CNTK.FSharp.Core.Minibatch;

namespace DeepLearningDemo.DigitRecognizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ChartValues<double> values = new ChartValues<double>();

        public MainWindow()
        {
            InitializeComponent();
            triningStats.Values = values;
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => NeuralNetwork.train(ReportProgress));
        }

        private void ReportProgress(TrainingSummary trainingSummary)
        {
            values.Add(trainingSummary.Evaluation);
            if (values.Count > 40)
            {
                values.RemoveAt(0);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            testsResult.Text = NeuralNetwork.test();
        }

        private static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            try
            {
                using (var memory = new MemoryStream())
                {
                    bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    var bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();

                    return bitmapimage;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            tests.ItemsSource =
                NeuralNetwork.visualize()
                .Select(BitmapToImageSource);
        }

        private void img_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            img1.Source =
                BitmapToImageSource(
                    NeuralNetwork.predict(
                        ImageUtil.ParallelExtractCHW(
                            ImageUtil.Resize(
                                BitmapFromSource(
                                    InkCanvasToBitmapSource()), 28, 28, true), true)));
            img.Strokes.Clear();
        }

        public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        private BitmapSource InkCanvasToBitmapSource()
        {
            int width = (int)img.ActualWidth ;
            int height = (int)img.ActualHeight;
            var size = new System.Windows.Size(img.ActualWidth, img.ActualHeight);
            img.Measure(size);
            img.Arrange(new Rect(size));
            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(img);
            return rtb;
        }
    }
}
