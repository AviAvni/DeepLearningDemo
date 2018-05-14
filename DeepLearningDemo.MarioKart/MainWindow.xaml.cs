using System;
using System.Drawing;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DeepLearningDemo.MarioKart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IDisposable subsctiption;

        public MainWindow()
        {
            InitializeComponent();
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
                        keys.ItemsSource = b.Item2;
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

    }
}