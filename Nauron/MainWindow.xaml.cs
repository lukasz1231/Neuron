using Nauron.Models;
using System;
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
        List<List<double>> trainingX;
        double[] trainingD;
        List<List<double>> testingX;
        double[] testingD;
        FunctionSelector fs;
        Neuron neuron;
        DataManager dataManager;


        private void DrawData()
        {
            PlotCanvas.Children.Clear();
            DrawAxesWithLabels(PlotCanvas); // Dodanie osi z etykietami

            (trainingX, trainingD, testingX, testingD) = neuron.GetData();
            double width = PlotCanvas.ActualWidth;
            double height = PlotCanvas.ActualHeight;
            double maxX=0, minX=double.MaxValue, maxY = 0, minY = double.MaxValue;
            for (int i = 0; i < trainingX.Count; i++)
            {
                if (trainingX[i][0]>maxX)
                    maxX = trainingX[i][0];
                if (trainingX[i][0]<minX)
                    minX= trainingX[i][0];
                if (trainingX[i][1] > maxY)
                    maxY = trainingX[i][1];
                if (trainingX[i][1] < minY)
                    minY = trainingX[i][1];
            }
            for (int i = 0; i < trainingX.Count; i++)
            {
                    double x = Normalize(trainingX[i][0], minX, maxX, 10, width - 10);
                    double y = Normalize(trainingX[i][1], minY, maxY, 10, height - 10);

                    Ellipse point = new Ellipse
                    {
                        Width = 6,
                        Height = 6,
                        Fill = trainingD[i] > 0 ? Brushes.Green : Brushes.Red
                    };

                    Canvas.SetLeft(point, x);
                    Canvas.SetTop(point, y);
                        PlotCanvas.Children.Add(point);
            }
            for(int i=0;i< trainingX.Count; i++)
            {
                var wynik = neuron.Calculate(i);
                if (wynik > maxY)
                    maxY = wynik;
                if (wynik < minY)
                    minY = wynik;
            }
            for (int i = 0; i < trainingX.Count; i++)
            {
                double x = Normalize(trainingX[i][0], minX, maxX, 10, width - 10);
                double y = Normalize(neuron.Calculate(i), minY, maxY, 10, height - 10);

                Ellipse point = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = Brushes.Violet
                };

                Canvas.SetLeft(point, x);
                Canvas.SetTop(point, y);
                PlotCanvas.Children.Add(point);
            }
        }

        private void DrawDecisionBoundary()
        {
            double width = PlotCanvas.ActualWidth;
            double height = PlotCanvas.ActualHeight;
            (trainingX, trainingD, testingX, testingD) = neuron.GetData();

            //List<List<double>> weights = neuron.GetWeights();
            //if (weights.Count<2 || weights[0][1]==0)
            //    return; // brak dzielenia przez zero lub brakujących danych

            //double thresholdX = -weights[0][0] / weights[0][1];

            double maxX = 0, minX = double.MaxValue, maxY = 0, minY = double.MaxValue;
            double wynik = 0;
            for (int i = 0; i < trainingX.Count; i++)
            {
                if (trainingX[i][0] > maxX)
                    maxX = trainingX[i][0];
                if (trainingX[i][0] < minX)
                    minX = trainingX[i][0];
                wynik = neuron.Calculate(i);
                if(wynik > maxY)
                    maxY = wynik;
                if(wynik < minY)
                    minY = wynik;
            }
            double x, y, x2, y2;
            for (int i = 0; i < trainingX.Count-1; i++)
            {
                    x = Normalize(trainingX[i][0], minX, maxX, 10, width - 10);
                    y = Normalize(neuron.Calculate(i), minY, maxY, 10, height - 10);
                    x2 = Normalize(trainingX[i+1][0], minX, maxX, 10, width - 10);
                    y2 = Normalize(neuron.Calculate(i + 1), minY, maxY, 10, height - 10);

                    Line line = new Line
                    {
                        X1 = x,
                        Y1 = y,
                        X2 = x2,
                        Y2 = y2,
                        Stroke = Brushes.Blue,
                        StrokeThickness = 2,
                        StrokeDashArray = new DoubleCollection { 2 }
                    };
            PlotCanvas.Children.Add(line);
            }
        }
        private void FunctionButton_Click(object sender, RoutedEventArgs e)
        {
            if (fs == null) fs = new FunctionSelector(this);
            if(!fs.IsActive){
                if(!fs.IsLoaded){
                    fs = new FunctionSelector(this);
                }
                fs.Show();
            }
        }
        public void ChangeFunction(int i)
        {
            neuron.ChangeFunction(i);
        }
        private void InitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fileName = FileNameBox.Text.Trim(); 
                (trainingX, trainingD, testingX, testingD) = dataManager.LoadData(fileName, double.Parse(UlamekTestowych.Text));
                neuron.newData(trainingX, trainingD, testingX, testingD);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            neuron.InitTrain();
        }
        private void TrainButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = FileNameBox.Text.Trim();
            double error;
            //try
            //{
                if (CheckboxErr.IsChecked ?? false){
                    error = neuron.TrainToBias(double.Parse(MaxErrorBox.Text), Convert.ToInt64(MaxIterBox.Text));
                }
                else if (CheckboxIter.IsChecked ?? false)
                    error = neuron.TrainToIterations(Convert.ToInt64(MaxIterBox.Text));
                else if (CheckboxSingle.IsChecked ?? false)
                    error = neuron.SingleIterationTrain();
                else
                {
                    MessageBox.Show("Wybrano niepoprawny tryb trenowania");
                    return;
            }
        //}
        //    catch (ArgumentException ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //        return;
        //    }
        //    catch (FileNotFoundException ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //        return;
        //    }
            ErrorText.Content = $"Trained with error: {error:F4}";
            DrawData();
            DrawDecisionBoundary();
            DrawErrorPlot();
        }
        public void ChangeTrainingModeSingle(object sender, RoutedEventArgs e)
        {
            CheckboxIter.IsChecked = false;
            CheckboxErr.IsChecked = false;
        }
        public void ChangeTrainingModeMaxIt(object sender, RoutedEventArgs e)
        {
            CheckboxErr.IsChecked = false;
            CheckboxSingle.IsChecked = false;
        }
        public void ChangeTrainingModeMaxErr(object sender, RoutedEventArgs e)
        {
            CheckboxSingle.IsChecked= false;
            CheckboxIter.IsChecked= false;
        }
        private void DrawErrorPlot()
        {
            ErrorPlotCanvas.Children.Clear();
            DrawAxesWithLabels(ErrorPlotCanvas, false); // Dodanie osi z etykietami
            var errors=neuron.GetTrainingErrors();

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
                double y = (errors[i] / maxError) * (height - 20) - 10;
                polyline.Points.Add(new Point(x, y));
            }

            ErrorPlotCanvas.Children.Add(polyline);
        }

        private void DrawAxesWithLabels(Canvas canvas, bool withX=true)
        {
            (trainingX, trainingD, testingX, testingD) = neuron.GetData();
            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;

            if (withX)
            {
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
            }
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
            double xMax = 0, xMin = double.MaxValue;
            for (int i = 0; i < trainingX.Count; i++)
            {
                if (trainingX[i].Max() > xMax)
                    xMax = trainingX[i].Max();
                if (trainingX[i].Min() < xMin)
                    xMin = trainingX[i].Min();
            }
            double yMin = trainingD.Min();
            double yMax = trainingD.Max();

            if (withX)
            {
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
            if (min == max)
                max++;
            return targetMin + (value - min) / (max - min) * (targetMax - targetMin);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CheckboxSingle.IsChecked = true;
            neuron = new Perceptron(1);
            dataManager = new DataManager();
        }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
    }
}
