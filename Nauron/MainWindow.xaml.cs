using Nauron.Models;
using System;
using System.Diagnostics.Eventing.Reader;
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
        DataEditor de;
        Neuron neuron;
        DataManager dataManager;
        double yMin, yMax, xMin, xMax;
        private void DrawAxesWithLabels(Canvas canvas, bool withX = true)
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
            xMax = 0;
            xMin = double.MaxValue;
            if(withX){
                for (int i = 0; i < trainingX.Count; i++)
                {
                    if (trainingX[i][0] > xMax)
                        xMax = trainingX[i][0];
                    if (trainingX[i][0] < xMin)
                        xMin = trainingX[i][0];
                }
                xMax=Math.Ceiling(xMax);
                xMin = Math.Floor(xMin);
            }
            yMin=double.MaxValue;
            yMax=0;
            if (withX)
            {
                for (int i = 0; i < trainingX.Count; i++)
                {
                    if (trainingX[i][1] > yMax)
                        yMax = trainingX[i][1];
                    if (trainingX[i][1] < yMin)
                        yMin = trainingX[i][1];
                }
                yMax = Math.Ceiling(yMax);
                yMin = Math.Floor(yMin);
            }
            else
            {
                yMin = Math.Floor(neuron.GetTrainingErrors().Min());
                yMax = Math.Ceiling(neuron.GetTrainingErrors().Max());
            }


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

        private void DrawData()
        {
            PlotCanvas.Children.Clear();
            DrawAxesWithLabels(PlotCanvas); // Dodanie osi z etykietami

            (trainingX, trainingD, testingX, testingD) = neuron.GetData();
            double width = PlotCanvas.ActualWidth;
            double height = PlotCanvas.ActualHeight;
            for (int i = 0; i < trainingX.Count; i++)
            {
                    double x = Normalize(trainingX[i][0], xMin, xMax, 10, width - 10);
                    double y = Normalize(trainingX[i][1], yMin, yMax, height - 10, 10);

                    Ellipse point = new Ellipse
                    {
                        Width = 6,
                        Height = 6,
                        Fill = trainingD[i] > 0 ? Brushes.Green : Brushes.Red
                    };

                    Canvas.SetLeft(point, x-3);
                    Canvas.SetTop(point, y-3);
                        PlotCanvas.Children.Add(point);
            }
        }

        private void DrawDecisionBoundary()
        {
            double width = PlotCanvas.ActualWidth;
            double height = PlotCanvas.ActualHeight;
            (trainingX, trainingD, testingX, testingD) = neuron.GetData();
            var W = neuron.GetWeights();
            double x, y, yTemp;
            Polyline polyline = new Polyline
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 2 }
            };
            if (W[2] == 0) W[2]=0.0000001;
            for (int i = 0; i < trainingX.Count; i++)
            {
                x = Normalize(i, 0, trainingX.Count - 1, xMin, xMax);
                yTemp = -W[1] / W[2] * x - W[0] / W[2];
                x = Normalize(i, 0, trainingX.Count-1, 10, width - 10);
                y = Normalize(yTemp, yMin, yMax, height - 10, 10);
                polyline.Points.Add(new Point(x, y));
            }
            polyline.ClipToBounds = true;
            PlotCanvas.Children.Add(polyline);
        }
        private void DrawErrorPlot()
        {
            ErrorPlotCanvas.Children.Clear();
            DrawAxesWithLabels(ErrorPlotCanvas, false); // Dodanie osi z etykietami
            var errors = neuron.GetTrainingErrors();


            double width = ErrorPlotCanvas.ActualWidth;
            double height = ErrorPlotCanvas.ActualHeight;

            Polyline polyline = new Polyline
            {
                Stroke = Brushes.DarkRed,
                StrokeThickness = 2
            };

            for (int i = 0; i < errors.Count; i++)
            {
                double x = (i / ( errors.Count-1.0)) * (width - 20) + 10;
                double y = Normalize(errors[i],yMin, yMax,height-10,10);
                polyline.Points.Add(new Point(x, y));
                if (errors.Count == 1)
                {
                    polyline.Points.Clear();
                    polyline.Points.Add(new Point(10, y));
                    x = width - 10;
                    polyline.Points.Add(new Point(x, y));
                }
            }

            ErrorPlotCanvas.Children.Add(polyline);
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
        private void OpenDataEditor(object sender, RoutedEventArgs e)
        {
            if(!neuron.IsInitialized()){
                MessageBox.Show("Błąd!\nZainicjalizuj dane");
                return;
            }
            if (de == null) de = new DataEditor(this);
                if (!de.IsActive)
                {
                    if (!de.IsLoaded)
                    {
                        de = new DataEditor(this);
                    }
                    de.Show();
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
                (trainingX, trainingD, testingX, testingD) = dataManager.LoadData(fileName);
                neuron.newData(trainingX, trainingD, testingX, testingD);
                neuron.ChangeLearningRate(double.Parse(LearningRate.Text));
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
            double error;
            try
            {
                if (CheckboxErr.IsChecked ?? false){
                    error = neuron.TrainToBias(double.Parse(MaxErrorBox.Text), long.Parse(MaxIterBox.Text));
                }
                else if (CheckboxIter.IsChecked ?? false)
                    error = neuron.TrainToIterations(long.Parse(MaxIterBox.Text));
                else if (CheckboxSingle.IsChecked ?? false)
                    error = neuron.SingleIterationTrain();
                else
                {
                    MessageBox.Show("Wybrano niepoprawny tryb trenowania");
                    return;
                }
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
        
        public double Normalize(double value, double min, double max, double targetMin, double targetMax)
        {
            if (min == max)
                max++;
            return targetMin + (value - min) / (max - min) * (targetMax - targetMin);
        }
        public void SetNeuron(Neuron nr)
        {
            neuron = nr;
        }
        public Neuron GetNeuron()
        {
            return neuron;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CheckboxSingle.IsChecked = true;
            neuron = new Perceptron(0);
            dataManager = new DataManager();
        }
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
    }
}
