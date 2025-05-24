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
using System.Windows.Shapes;

namespace Nauron
{
    /// <summary>
    /// Logika interakcji dla klasy FunctionSelector.xaml
    /// </summary>
    public partial class FunctionSelector : Window
    {
        MainWindow main;
        public FunctionSelector(MainWindow mw)
        {
            main = mw;
            InitializeComponent();
        }

        public void FunctionSelector_Loaded(object sender, RoutedEventArgs e)
        {
        }

        void StepBinary(object sender, RoutedEventArgs e)
        {
            main.ChangeFunction(0);
            Loaded -= FunctionSelector_Loaded;
            this.Close();
        }
        void StepBipolar(object sender, RoutedEventArgs e)
        {
            main.ChangeFunction(1);
            Loaded -= FunctionSelector_Loaded;
            this.Close();
        }
        void Sigmoid(object sender, RoutedEventArgs e)
        {
            main.ChangeFunction(2);
            Loaded -= FunctionSelector_Loaded;
            this.Close();
        }
        void Tanh(object sender, RoutedEventArgs e)
        {
            main.ChangeFunction(3);
            Loaded -= FunctionSelector_Loaded;
            this.Close();
        }
        void ReLU(object sender, RoutedEventArgs e)
        {
            main.ChangeFunction(4);
            Loaded -= FunctionSelector_Loaded;
            this.Close();
        }
        void LeakyReLU(object sender, RoutedEventArgs e)
        {
            main.ChangeFunction(5);
            Loaded -= FunctionSelector_Loaded;
            this.Close();
        }

    }
}
