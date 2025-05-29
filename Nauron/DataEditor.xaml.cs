using Nauron.Models;
using System.ComponentModel;
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
        int pointSelected, highlightedPoint;
        bool mouseDown, shiftDown;
        public DataEditor(MainWindow mw)
        {
            InitializeComponent();
            main = mw;
            trainingD = new();
            Loaded += DataEditor_Loaded;
        }
        private void DataEditor_Loaded(object sender, RoutedEventArgs e)
        {
            KeyDown += new KeyEventHandler(KeyDownHandler);
            KeyUp += new KeyEventHandler(KeyUpHandler);
            neuron = main.GetNeuron();
            (trainingX, var list) = neuron.GetData();
            foreach (var item in list)
            {
                trainingD.Add(item);
            }
            Closing += new CancelEventHandler(ZapiszDaneNeuron);
            pointSelected = -1;
            highlightedPoint = -1;
            shiftDown = false;
            FindMinAndMaxPoint(); 
            DrawPoints();
        }

        public void ZapiszDane(object sender, RoutedEventArgs e)
        {
            DataManager.SaveToFile(FileNameBox.Text, DataToString());
            FindMinAndMaxPoint();
        }
        public void ZapiszDaneNeuron(object sender, CancelEventArgs e)
        {
            neuron.newData(trainingX, trainingD);
            main.SetNeuron(neuron);
        }
        void UsunPunkt(object sender, RoutedEventArgs e)
        {
            if(pointSelected == -1)
            {
                MessageBox.Show("Nie wybrano punktu");
                return;
            }
            trainingD.RemoveAt(pointSelected);
            trainingX.RemoveAt(pointSelected);
            pointSelected = -1;
            FindMinAndMaxPoint();
            DrawPoints();
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
            if (!double.TryParse(XCoord.Text, out double x) || pointSelected==-1)
            {
                return;
            }
            trainingX[pointSelected][0] = x;
            if (x > xMax) xMax = x;
            if (x < xMin) xMin = x;
            if (!(x == xMin || x == xMax))
                FindMinAndMaxPoint();
            DrawPoints();
        }
        public void CoordChangedY(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(YCoord.Text, out double y) || pointSelected == -1) 
            {
                return;
            }
            trainingX[pointSelected][1] = y;
            if (y > yMax) yMax = y;
            if (y < yMin) yMin = y;
            if (!(y == yMin || y == yMax))
                FindMinAndMaxPoint();
            DrawPoints();
        }
        public void ZmienZbior(object sender, RoutedEventArgs e)
        {
            if (DButton.Background == Brushes.Red){
                DButton.Background = Brushes.Green;
                if (pointSelected != -1){
                    trainingD[pointSelected] = 1;
                    DrawPoints();
                }
            }
            else{
                DButton.Background = Brushes.Red;
                if(pointSelected != -1) {
                    trainingD[pointSelected] = 0;
                    DrawPoints();
                }
            }

        }
        public void DodajPunkt(object sender, RoutedEventArgs e)
        {
            trainingX.Add([(xMax-xMin)/2+xMin,(yMax-yMin)/2+yMin]);
            trainingD.Add(DButton.Background == Brushes.Red ? 0:1);
            DrawPoints();
        }

        public void MouseDownCanvas(object sender, RoutedEventArgs e)
        {
            mouseDown = true;
            var prevoiusPoint = pointSelected;
            if (highlightedPoint != -1)
            {
                pointSelected = highlightedPoint;
                if (prevoiusPoint != pointSelected){
                    XCoord.Text = trainingX[pointSelected][0].ToString();
                    YCoord.Text = trainingX[pointSelected][1].ToString();
                    DButton.Background = trainingD[pointSelected] == 1 ? Brushes.Green : Brushes.Red;
                    DrawPoints();
                }
            }
            else{
                pointSelected = -1;
                if(prevoiusPoint!= pointSelected)
                    DrawPoints();
            }
        }
        public void MouseUpCanvas(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }
        void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if(e.Key== Key.LeftShift)
            {
                shiftDown = true;
            }
        }
        void KeyUpHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                shiftDown = false;
            }
        }
        public void MouseMoveCanvas(object sender, RoutedEventArgs e)
        {
            Point mouse;
            FindHighlightedPoint(mouse = Mouse.GetPosition(DataPointsCanvas));
            if (highlightedPoint != -1)
            {
                DrawPoints();
            }
            if(pointSelected!=-1 && mouseDown)
            {
                var width = DataPointsCanvas.ActualWidth;
                var height = DataPointsCanvas.ActualHeight;

                double x = main.Normalize(mouse.X, 10, width-10, xMin, xMax);
                double y = main.Normalize(mouse.Y, height-10, 10, yMin, yMax);

                XCoord.Text = (Math.Round(x, shiftDown ? 5 : 2)).ToString();
                YCoord.Text = (Math.Round(y, shiftDown ? 5 : 2)).ToString();
            }
        }
        void FindHighlightedPoint(Point mouse)
        {
            var width = DataPointsCanvas.ActualWidth;
            var height = DataPointsCanvas.ActualHeight;
            var prevoiusPoint = highlightedPoint;
            for (int i = 0; i < trainingX.Count; i++)
            {
                double x = main.Normalize(trainingX[i][0], xMin, xMax, 10, width - 10);
                double y = main.Normalize(trainingX[i][1], yMin, yMax, height - 10, 10);
                if(x+6>=mouse.X && x-6<=mouse.X&& y + 6 >= mouse.Y && y - 6 <= mouse.Y)
                {
                    highlightedPoint = i;
                    return;
                }
            }
            highlightedPoint = -1;
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
                    Width = pointSelected==i || highlightedPoint==i? 12: 6,
                    Height = pointSelected == i || highlightedPoint == i ? 12 : 6,
                    Fill = trainingD[i] > 0 ? pointSelected == i ? Brushes.YellowGreen : Brushes.Green : pointSelected == i ? Brushes.MediumVioletRed :  Brushes.Red,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(point, x- point.Width/ 2);
                Canvas.SetTop(point, y- point.Width / 2);
                DataPointsCanvas.Children.Add(point);
            }
        }
        void RecalculateMinMax(object sender, RoutedEventArgs e)
        {
            FindMinAndMaxPoint();
            DrawPoints();
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
            xMax = xMax + (xMax - xMin) * 0.01;
            yMax = yMax + (yMax - yMin) * 0.01;
            xMin = xMin - ( xMax - xMin) * 0.01;
            yMin = yMin - ( yMax - yMin) * 0.01;
        }
    }
}
