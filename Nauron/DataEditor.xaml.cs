using Nauron.Models;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
namespace Nauron
{
    /// <summary>
    /// Logika interakcji dla klasy DataEditor.xaml
    /// </summary>
    public partial class DataEditor : Window
    {
        MainWindow main;
        Neuron neuron;
        List<List<double>> trainingX;
        List<double> trainingD;
        double xMax = 0, yMax = 0;
        double xMin = double.MaxValue, yMin = double.MaxValue;
        int pointSelected;
        public DataEditor(MainWindow mw)
        {
            InitializeComponent();
            main = mw;
            trainingD = new();
            Loaded += DataEditor_Loaded;
        }
        private void DataEditor_Loaded(object sender, RoutedEventArgs e)
        {
            neuron = main.GetNeuron();
            (trainingX, var list, var a, var b) = neuron.GetData();
            foreach (var item in list)
            {
                trainingD.Add(item);
            }
            pointSelected = -1;
            FindMinAndMaxPoint(); 
            DrawPoints();
        }

        public void ZapiszDane(object sender, RoutedEventArgs e)
        {
            DataManager.SaveToFile(FileNameBox.Text, DataToString());
            FindMinAndMaxPoint();
        }
        public void ZapiszDaneNeuron(object sender, RoutedEventArgs e)
        {
            neuron.newData(trainingX, trainingD.ToArray(), null, null);
            main.SetNeuron(neuron);
        }
        string[] DataToString()
        {
            List<string> wynik = new(trainingD.Count);
            for (int i = 0; i < trainingD.Count; i++)
            {
                wynik.Add($"{trainingX[i][0]} {trainingX[i][1]} {trainingD[i]}");
            }
            return wynik.ToArray();
        }
        public void CoordChangedX(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(XCoord.Text, out double x))
            {
                return;
            }
            trainingX[pointSelected][0] = x;
            if (pointSelected != -1)
            {
                if (x > xMax) xMax = x;
                if (x < xMin) xMin = x;
                if (!(x == xMin || x == xMax))
                    FindMinAndMaxPoint();
            }
            DrawPoints();
        }
        public void CoordChangedY(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(YCoord.Text, out double y)) 
            {
                return;
            }
            trainingX[pointSelected][1] = y;
            if (pointSelected != -1)
            {
                if (y > yMax) yMax = y;
                if (y < yMin) yMin = y;
                if (!(y == yMin || y == yMax))
                    FindMinAndMaxPoint();
            }
            DrawPoints();
        }
        public void ZmienZbior(object sender, RoutedEventArgs e)
        {
            if (DButton.Background == Brushes.Red)
                DButton.Background = Brushes.Green;
            else
                DButton.Background = Brushes.Red;
        }
        public void DodajPunkt(object sender, RoutedEventArgs e)
        {
            trainingX.Add([(xMax-xMin)/2+xMin,(yMax-yMin)/2+yMin]);
            trainingD.Add(DButton.Background == Brushes.Red ? 0:1);
            DrawPoints();
        }

        public void MouseDownCanvas(object sender, RoutedEventArgs e)
        {
            FindChosenPoint(Mouse.GetPosition(DataPointsCanvas));
            if (pointSelected != -1)
            {
                DrawPoints();
            }
        }
        void FindChosenPoint(Point mouse)
        {
            var width = DataPointsCanvas.ActualWidth;
            var height = DataPointsCanvas.ActualHeight;
            var prevoiusPoint = pointSelected;
            for (int i = 0; i < trainingX.Count; i++)
            {
                double x = main.Normalize(trainingX[i][0], xMin, xMax, 10, width - 10);
                double y = main.Normalize(trainingX[i][1], yMin, yMax, height - 10, 10);
                if(x+6>=mouse.X && x-6<=mouse.X&& y + 6 >= mouse.Y && y - 6 <= mouse.Y)
                {
                    pointSelected = i;
                    XCoord.Text = trainingX[i][0].ToString();
                    YCoord.Text = trainingX[i][1].ToString();
                    return;
                }
            }
            pointSelected = -1;
            if(prevoiusPoint!=-1)
                DrawPoints();
        }
        void DrawPoints()
        {
            var width = DataPointsCanvas.ActualWidth;
            var height = DataPointsCanvas.ActualHeight;

            DataPointsCanvas.Children.Clear();
            for (int i = 0; i < trainingX.Count; i++)
            {
                double x = main.Normalize(trainingX[i][0], xMin, xMax, 10, width - 10);
                double y = main.Normalize(trainingX[i][1], yMin, yMax, height - 10, 10);
                var point = new Ellipse
                {
                    Width = pointSelected==i? 12: 6,
                    Height = pointSelected == i ? 12 : 6,
                    Fill = trainingD[i] > 0 ? Brushes.Green : Brushes.Red,
                };
                Canvas.SetLeft(point, x- (pointSelected == i ? 12 : 6)/2);
                Canvas.SetTop(point, y- (pointSelected == i ? 12 : 6)/2);
                DataPointsCanvas.Children.Add(point);
            }
        }
        void FindMinAndMaxPoint()
        {
            xMax = 0;
            yMax = 0;
            yMin = double.MaxValue;
            xMin = double.MaxValue;
            for (int i = 0; i < trainingX.Count; i++)
            {
                if (trainingX[i][0] > xMax)
                    xMax = trainingX[i][0];
                if (trainingX[i][0] < xMin)
                    xMin = trainingX[i][0];
                if (trainingX[i][1] > yMax)
                    yMax = trainingX[i][1];
                if (trainingX[i][1] < yMin)
                    yMin = trainingX[i][1];
            }
            xMax = Math.Ceiling(xMax);
            xMin = Math.Floor(xMin);
            yMin = Math.Floor(yMin);
            yMax = Math.Ceiling(yMax);
        }
    }
}
