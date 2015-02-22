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
            Data.SQLiteDBWrapper.getReference();
            // Individual overflow break downs:
            // d2: 25->26
            // d3: 17->18
            // d4: 14->15
            // d5: 12->13
            // d6: 11->12
            // d7: 11->12
            // d8: 10->11
            // d9:  9->10
            // d10: 9->10
            // d20: 7-> 8
            // Inverse logarithmic function... booo
            // doesn't break down as long as you keep each die under it's limit... so 14d4 + 11d6 + 10d8 + 9d10 works, albeit a bit slowly
            /*
            Double sum = 0;
            RollPattern rp1 = new RollPattern("3d8 - 5"); //"14d4 + 11d6 + 10d8 + 9d10");
            for (int i = rp1.Min; i <= rp1.Max; i++)
            {
                sum += rp1.p(i);
                System.Diagnostics.Trace.WriteLine("p("+i+") = " + rp1.p(i).ToString());
            }
            System.Diagnostics.Trace.WriteLine("Sum = " + sum.ToString());
            System.Diagnostics.Trace.WriteLine("Average = " + rp1.Mean);
            System.Diagnostics.Trace.WriteLine("StdDev = " + rp1.StandardDeviation);
            System.Diagnostics.Trace.WriteLine("Pattern = " + rp1.ToString(true));
            System.Diagnostics.Trace.WriteLine("CalcTime = "+ rp1.CalcTime + "ms");

            Application.Current.Shutdown();
             * */
            Data.CacheWorkout.Work();
        }

        
    }
}
