using Microsoft.Azure.CognitiveServices.Search.ImageSearch;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace DeepLearningDemo.CognitiveServices
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                var computerVisionApiKey = Environment.GetEnvironmentVariable("ComputerVisionApiKey");
                using (var fs = new FileStream(ofd.FileName, FileMode.Open))
                using (var api = new ComputerVisionAPI(new Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ApiKeyServiceClientCredentials(computerVisionApiKey)))
                {
                    api.AzureRegion = AzureRegions.Westeurope;
                    result.DataContext = await api.AnalyzeImageInStreamAsync(fs, new List<VisualFeatureTypes> { VisualFeatureTypes.Adult, VisualFeatureTypes.Categories, VisualFeatureTypes.Color, VisualFeatureTypes.Description, VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType, VisualFeatureTypes.Tags });
                }
                imgVision.Source = new BitmapImage(new Uri(ofd.FileName));
            }
        }

        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
            var imageSearchApiKey = Environment.GetEnvironmentVariable("ImageSearchApiKey");
            using (var api = new ImageSearchAPI(new Microsoft.Azure.CognitiveServices.Search.ImageSearch.ApiKeyServiceClientCredentials(imageSearchApiKey)))
            {
                var res = await api.Images.SearchAsync(search.Text);
                imagesSearch.ItemsSource = res.Value;
            }
        }
    }
}
