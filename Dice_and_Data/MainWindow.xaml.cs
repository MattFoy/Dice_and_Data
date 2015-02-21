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

namespace Dice_and_Data
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            Double sum = 0;
            RollPattern rp1 = new RollPattern("2d6");
            for (int i = rp1.Min; i <= rp1.Max; i++)
            {
                sum += rp1.p(i);
                System.Diagnostics.Trace.WriteLine("p("+i+") = " + rp1.p(i).ToString());
            }
            System.Diagnostics.Trace.WriteLine("Sum = " + sum.ToString());
            System.Diagnostics.Trace.WriteLine("Average = " + rp1.Mean);
            System.Diagnostics.Trace.WriteLine("StdDev = " + rp1.StandardDeviation);

            
            
        }
    }
}
