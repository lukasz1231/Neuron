using Nauron.Models;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Nauron
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double[] trainingX;
        double[] trainingY;
        double[] testingX;
        double[] testingY;
        Perceptron perceptron;
        DataManager dataManager;
        public MainWindow()
        {
            InitializeComponent();

            dataManager = new DataManager();
            (trainingX, trainingY, testingX, testingY) = dataManager.LoadData("data1.txt", 0.7);
            perceptron = new Perceptron(trainingX, trainingY, testingX, testingY, 0.1, 1);
            double error = perceptron.Train(trainingX, trainingY);
            MessageBox.Show("Trained with error: " + error.ToString("F4"));

        }
       

    }
}