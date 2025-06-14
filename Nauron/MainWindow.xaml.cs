﻿using Microsoft.Win32;
using Nauron.Models;
using System;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
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
        List<double> trainingD;
        DataEditor de;
        Neuron neuron;
        DataManager dataManager;
        double yMin, yMax, xMin, xMax;
        bool initialized, shiftDown, sDown, ctrlDown, oDown;
        string fileName, filePath;
        private void DrawAxesWithLabels(Canvas canvas, bool withX = true)
        {
            (trainingX, trainingD) = neuron.GetData();
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

            (trainingX, trainingD) = neuron.GetData();
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
            (trainingX, trainingD) = neuron.GetData();
            var W = neuron.GetWeights();
            double x, y, yTemp;
            Polyline polyline = new Polyline
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 2 }
            };
            if (W[2] == 0) W[2]=0.0000001;
            for (int i = 0; i < trainingX.Count; i+=trainingX.Count-1)
            {
                x = Normalize(i, 0, trainingX.Count - 1, xMin, xMax);
                yTemp = -W[1] / W[2] * x - W[0] / W[2];
                x = Normalize(i, 0, trainingX.Count-1, 10, width - 10);
                y = Normalize(yTemp, yMin, yMax, height - 10, 10);
                if (y < 10)
                {
                    if (W[1] == 0) W[1] = 0.0000001;
                    y = Normalize(10, height - 10, 10, yMin, yMax);
                    x = -W[2] / W[1] * y - W[0] / W[1];
                    y = 10;
                    x = Normalize(x, xMin, xMax, 10, width - 10);
                }
                if (y > height - 10)
                {
                    if (W[1] == 0) W[1] = 0.0000001;
                    y = Normalize(height - 10, height - 10, 10, yMin, yMax);
                    x = -W[2] / W[1] * y - W[0] / W[1];
                    y = height-10;
                    x = Normalize(x, xMin, xMax, 10, width - 10);
                }
                polyline.Points.Add(new Point(x, y));
            }
            polyline.Clip = PlotCanvas.Clip;
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

        private void OpenDataEditor(object sender, RoutedEventArgs e)
        {
            if(!initialized){
                MessageBox.Show("Błąd!\nWgraj dane");
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
            de.Closing += new CancelEventHandler(CloseDataEditor);
        }
        void CloseDataEditor(object sender, CancelEventArgs e)
        {
            if (fileName != null && fileName!="")
                Title = "Neuron " + fileName + "*";
            DrawData();
            DrawDecisionBoundary();
        }
        public void ChangeFunction(int i)
        {
            neuron.ChangeFunction(i);
        }
        void ResetNeuronButton_Click(object sender, RoutedEventArgs e)
        {
            neuron.InitTrain();
            if (fileName != null && fileName != "")
                Title = "Neuron " + fileName + "*";
            if(initialized){
                DrawData();
                DrawDecisionBoundary();
            }
        }

        private void InitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fileName = FileNameBox.Text.Trim(); 
                (trainingX, trainingD) = dataManager.LoadData(fileName);
                neuron.newData(trainingX, trainingD);
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
            if (fileName != null && fileName != "")
                Title = "Neuron " + fileName + "*";
            initialized = true;
            DrawData();
            DrawDecisionBoundary();
        }
        private void TrainButton_Click(object sender, RoutedEventArgs e)
        {
            double error;
            try
            {
                neuron.ChangeLearningRate(double.Parse(LearningRate.Text));
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
            catch (FormatException ex)
            {
                MessageBox.Show("Nie podano poprawnego formatu (np. 0,1)\n" + ex.Message);
                return;
            }
            if (fileName != null && fileName != "")
                Title = "Neuron " + fileName + "*";
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
        void UncheckOtherFunctions(object wybrany)
        {
            CheckBox[] functions = new[] { btnStepBinary, btnStepBipolar, btnSigmoid, btnTanh, btnReLU, btnLeakyReLU };
            foreach (CheckBox wybor in functions)
            {
                if(wybor!=wybrany)
                    wybor.IsChecked = false;
            }
        }
        void FunctionFromSaveFile()
        {
            CheckBox checkBox;
            switch (neuron.GetFunction())
            {
                case 0: checkBox = btnStepBinary; break;
                case 1: checkBox = btnStepBipolar; break;
                case 2: checkBox = btnSigmoid; break;
                case 3: checkBox = btnTanh; break;
                case 4: checkBox = btnReLU; break;
                case 5: checkBox = btnLeakyReLU; break;
                default: checkBox = btnStepBinary; break;
            }
            checkBox.IsChecked = true;
            UncheckOtherFunctions(checkBox);
        }
        void FunctionChangeAction(object sender, RoutedEventArgs e)
        {
            if(e.Source == btnStepBinary)
                ChangeFunction(0);
            else if(e.Source == btnStepBipolar)
                ChangeFunction(1);
            else if(e.Source == btnSigmoid)
                ChangeFunction(2);
            else if(e.Source == btnTanh)
                ChangeFunction(3);
            else if(e.Source == btnReLU)
                ChangeFunction(4);
            else if(e.Source == btnLeakyReLU)
                ChangeFunction(5);
            UncheckOtherFunctions(e.Source);
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
        void KeyUpHandler(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.S: sDown = false; break;
                case Key.LeftShift: shiftDown = false; break;
                case Key.LeftCtrl: ctrlDown = false; break;
                case Key.O: oDown = false; break;
                default: break;
            }
        }
        void KeyDownHandler(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.S: sDown = true; break;
                case Key.LeftShift: shiftDown = true; break;
                case Key.LeftCtrl: ctrlDown = true; break;
                case Key.O: oDown = true; break;
                default: break;
            }
            if (ctrlDown)
            {
                if(sDown){
                    if (shiftDown)
                    {
                        SaveNewFile();
                    }
                    else
                        SaveFile();
                }
                if (oDown)
                    OpenFile();
            }
        }
        public void ChangeNeuronP(object sender, RoutedEventArgs e)
        {
            if (neuron is Perceptron) return;

            AdalCheckbox.IsChecked = false;
            var perc = new Perceptron(neuron.GetFunction());
            perc.ChangeWeights(neuron.GetWeights());
            (var X, var D) = neuron.GetData();
            perc.newData(X,D);
            neuron = perc;
        }
        public void ChangeNeuronA(object sender, RoutedEventArgs e)
        {
            if (neuron is Adaline) return;
            PercCheckbox.IsChecked = false;
            var adal = new Adaline(neuron.GetFunction());
            adal.ChangeWeights(neuron.GetWeights());
            (var X, var D) = neuron.GetData();
            adal.newData(X, D);
            neuron = adal;
        }
        public void SaveButton(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }
        public void SaveAsButton(object sender, RoutedEventArgs e)
        {
            SaveNewFile();
        }
        void OpenButton(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }
        bool isFileExtensionCorrect(string filename)
        {
            if (filename.Length < 4)
                return false;
            string extension = "";
            for(int i = filename.Length - 4; i < filename.Length; i++)
            {
                extension += filename[i];
            }
            if(extension==".nrn")
                return true;
            return false;
        }
        void OpenFile()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = ".nrn";
                if (openFileDialog.ShowDialog() == true)
                {
                    if(!isFileExtensionCorrect(openFileDialog.SafeFileName))
                    {
                        MessageBox.Show("Nie wybrano pliku z rozszerzeniem .nrn");
                        return;
                    }
                    fileName = openFileDialog.SafeFileName;
                    filePath = openFileDialog.FileName;
                    Title = "Neuron " + fileName;
                    neuron = dataManager.OpenFile(this, filePath);
                    (var x, var a) = neuron.GetData();
                    FunctionFromSaveFile();
                    if (x != null)
                    {
                        DrawData();
                        DrawDecisionBoundary();
                    }
                    if(neuron.GetTrainingErrors().Count>0)
                        DrawErrorPlot();
                    else
                        initialized = false;
                }
            }
            catch(NullReferenceException e)
            {
                MessageBox.Show("Plik jest pusty");
            }

        }
        void SaveNewFile()
        {
            shiftDown = sDown = ctrlDown = false;
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = System.IO.Path.GetFullPath("Data/");
            saveFileDialog.AddExtension = true;
            saveFileDialog.DefaultExt = ".nrn";
            if (saveFileDialog.ShowDialog() == true){
                fileName = saveFileDialog.SafeFileName;
                filePath = saveFileDialog.FileName;
                Title = "Neuron "+ fileName;
                dataManager.SaveFile(filePath, neuron);
            }
        }
        void SaveFile()
        {
            shiftDown = sDown = ctrlDown = false;
            if(fileName == null || fileName=="")
            {
                SaveNewFile();
                return;
            }
            Title = "Neuron "+ fileName;
            dataManager.SaveFile(filePath, neuron);
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if(!Directory.Exists("Data"))
                Directory.CreateDirectory("Data");
            Title = "Neuron //New file";
            initialized = false;
            CheckboxSingle.IsChecked = true;
            btnStepBinary.IsChecked = true;
            shiftDown = sDown = ctrlDown = false;
            neuron = new Perceptron(0);
            dataManager = new DataManager();
            KeyUp += new KeyEventHandler(KeyUpHandler);
            KeyDown += new KeyEventHandler(KeyDownHandler);
            PercCheckbox.IsChecked = true;
        }
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
    }
}
