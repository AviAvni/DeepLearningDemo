using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
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
using static CNTK.FSharp.Core.Minibatch;

namespace DeepLearningDemo.DigitRecognizer
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            NeuralNetwork.train(ReportProgress);
        }

        private void ReportProgress(TrainingSummary trainingSummary)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NeuralNetwork.visualize();
        }
    }
}
