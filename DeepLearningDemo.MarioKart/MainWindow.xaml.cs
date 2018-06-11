using CNTK;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using static CNTK.FSharp.Core.Minibatch;

namespace DeepLearningDemo.MarioKart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IDisposable subsctiption;
        private ChartValues<double> values = new ChartValues<double>();
        private List<double> tmpValues = new List<double>();

        public MainWindow()
        {
            InitializeComponent();
            triningStats.Values = values;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (subsctiption == null)
            {
                subsctiption = GenerateData.CaptureGameData()
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(b =>
                    {
                        img.Source = BitmapToImageSource(b.Item1);
                        grayImg.Source = BitmapToImageSource(b.Item2);
                        keys.ItemsSource = b.Item3;
                    });
            }
            else
            {
                subsctiption.Dispose();
                subsctiption = null;
            }
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

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => NeuralNetwork.train(ReportProgress));
        }

        private void ReportProgress(TrainingSummary trainingSummary)
        {
            tmpValues.Add(trainingSummary.Loss);
            if (tmpValues.Count == 100)
            {
                values.Add(tmpValues.Average());
                tmpValues.Clear();
            }
        }

        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {
            PlayGame.LoadModel("LSTM.model", DeviceDescriptor.GPUDevice(0));
            await PlayGame.DeepLearningPlay(DeviceDescriptor.GPUDevice(0));
        }
    }
}