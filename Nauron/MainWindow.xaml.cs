using Nauron.Models;
using System.IO;
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
        Neuron neuron;
        DataManager dataManager;


        private void DrawData()
        {
            PlotCanvas.Children.Clear();
            DrawAxesWithLabels(PlotCanvas); // Dodanie osi z etykietami

            double width = PlotCanvas.ActualWidth;
            double height = PlotCanvas.ActualHeight;

            for (int i = 0; i < trainingX.Length; i++)
            {
                double x = Normalize(trainingX[i], trainingX.Min(), trainingX.Max(), 10, width - 10);
                double y = height / 2;

                Ellipse point = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = trainingY[i] > 0 ? Brushes.Green : Brushes.Red
                };

                Canvas.SetLeft(point, x - 3);
                Canvas.SetTop(point, y - 3);
                PlotCanvas.Children.Add(point);
            }
        }

        private void DrawDecisionBoundary()
        {
            double width = PlotCanvas.ActualWidth;
            double height = PlotCanvas.ActualHeight;

            double[] weights = neuron.GetWeights();
            if (weights.Length < 2 || weights[1] == 0)
                return; // brak dzielenia przez zero lub brakujących danych

            double thresholdX = -weights[0] / weights[1];

            double x = Normalize(thresholdX, trainingX.Min(), trainingX.Max(), 10, width - 10);

            Line line = new Line
            {
                X1 = x,
                Y1 = 0,
                X2 = x,
                Y2 = height,
                Stroke = Brushes.Blue,
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 2 }
            };

            PlotCanvas.Children.Add(line);
        }

        private void TrainButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = FileNameBox.Text.Trim();
            try
            {
                (trainingX, trainingY, testingX, testingY) = dataManager.LoadData(fileName, 0.7);
            }
            catch(FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            catch(ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            neuron.newData(trainingX, trainingY, testingX, testingY);
            int maxEpochs = int.Parse(MaxIterBox.Text.Trim());
            double error = neuron.Train(trainingX, trainingY, maxEpochs);

            ErrorText.Text = $"Trained with error: {error:F4}";

            DrawData();
            DrawDecisionBoundary();
        }

        private void DrawErrorPlot(List<double> errors)
        {
            ErrorPlotCanvas.Children.Clear();
            DrawAxesWithLabels(ErrorPlotCanvas); // Dodanie osi z etykietami

            if (errors.Count < 2) return;

            double width = ErrorPlotCanvas.ActualWidth;
            double height = ErrorPlotCanvas.ActualHeight;

            double maxError = errors.Max();

            Polyline polyline = new Polyline
            {
                Stroke = Brushes.DarkRed,
                StrokeThickness = 2
            };

            for (int i = 0; i < errors.Count; i++)
            {
                double x = (i / (double)(errors.Count - 1)) * (width - 20) + 10;
                double y = height - (errors[i] / maxError) * (height - 20) - 10;
                polyline.Points.Add(new Point(x, y));
            }

            ErrorPlotCanvas.Children.Add(polyline);
        }

        private void DrawAxesWithLabels(Canvas canvas)
        {
            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;

            // Rysowanie osi X
            Line xAxis = new Line
            {
                X1 = 0,
                Y1 = height / 2,
                X2 = width,
                Y2 = height / 2,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            canvas.Children.Add(xAxis);

            // Rysowanie osi Y
            Line yAxis = new Line
            {
                X1 = width / 2,
                Y1 = 0,
                X2 = width / 2,
                Y2 = height,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            canvas.Children.Add(yAxis);

            // Przeskalowanie wartości dla osi X i Y
            double xMin = trainingX.Min();
            double xMax = trainingX.Max();
            double yMin = trainingY.Min();
            double yMax = trainingY.Max();

            // Rysowanie etykiet na osi X (co 1 jednostka)
            for (int i = (int)xMin; i <= xMax; i++)
            {
                double x = Normalize(i, xMin, xMax, 10, width - 10);
                TextBlock label = new TextBlock
                {
                    Text = i.ToString(),
                    Foreground = Brushes.Black,
                    FontSize = 10
                };
                Canvas.SetLeft(label, x - 10);
                Canvas.SetTop(label, height / 2 + 5);
                canvas.Children.Add(label);
            }

            // Rysowanie etykiet na osi Y (co 1 jednostka)
            for (int j = (int)yMin; j <= yMax; j++)
            {
                double y = Normalize(j, yMin, yMax, height - 10, 10); // Odwrócone przeskalowanie osi Y - do sprawdzenia
                TextBlock label = new TextBlock
                {
                    Text = j.ToString(),
                    Foreground = Brushes.Black,
                    FontSize = 10
                };
                Canvas.SetLeft(label, width / 2 + 5);
                Canvas.SetTop(label, y - 5);
                canvas.Children.Add(label);
            }
        }

        private double Normalize(double value, double min, double max, double targetMin, double targetMax)
        {
            return targetMin + (value - min) / (max - min) * (targetMax - targetMin);
        }

        private string GetEquation()
        {
            var weights = neuron.GetWeights();
            if (weights.Length < 3 || weights[2] == 0) return "Brak równania (dzielenie przez 0)";

            double a = -weights[1] / weights[2];
            double b = -weights[0] / weights[2];

            return $"y = {a:F2}x + {b:F2}";
        }

        public void ChangeFunction()
        {

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            dataManager = new DataManager();
            (trainingX, trainingY, testingX, testingY) = dataManager.LoadData("data1.txt", 0.7);
            neuron = new Perceptron(trainingX, trainingY, testingX, testingY, 0.1, 1);
            int maxEpochs = int.Parse(MaxIterBox.Text.Trim());
            double error = neuron.Train(trainingX, trainingY, maxEpochs);

            ErrorText.Text = $"Trained with error: {error:F4}, Prosta: {GetEquation()}";

            DrawData();
            DrawDecisionBoundary();
            DrawErrorPlot(neuron.GetTrainingErrors());
        }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
    }
}
