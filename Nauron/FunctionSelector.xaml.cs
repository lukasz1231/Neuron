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

        void StepBinary(object sender, RoutedEventArgs e)
        {
            main.ChangeFunction(0);
            this.Close();
        }
        void StepBipolar(object sender, RoutedEventArgs e)
        {
            main.ChangeFunction(1);
            this.Close();
        }
        void Sigmoid(object sender, RoutedEventArgs e)
        {
            main.ChangeFunction(2);
            this.Close();
        }
        void Tanh(object sender, RoutedEventArgs e)
        {
            main.ChangeFunction(3);
            this.Close();
        }
        void ReLU(object sender, RoutedEventArgs e)
        {
            main.ChangeFunction(4);
            this.Close();
        }
        void LeakyReLU(object sender, RoutedEventArgs e)
        {
            main.ChangeFunction(5);
            this.Close();
        }

    }
}
