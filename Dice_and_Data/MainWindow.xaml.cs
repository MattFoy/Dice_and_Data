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
            SQLiteDBWrapper wrapper = SQLiteDBWrapper.getReference();
            
            //some tests...
            /*
            //roll 1d10+3
            int result = Roll.d(10) + 3;
            wrapper.RecordRoll("1d10+3", result);

            //roll 4d8... 5 times
            for (int i = 0; i < 5; i++)
            {
                RollPlan rp = new RollPlan(4, 8);
                result = rp.execute();
                wrapper.RecordRoll("4d8", result);
            }
            */
            RollPattern rp1 = new RollPattern("1d4");
            System.Diagnostics.Trace.WriteLine("rp1: " + rp1.ToString());
            RollPattern rp2 = new RollPattern("20d500+3");
            System.Diagnostics.Trace.WriteLine("rp2: " + rp2.ToString());
            RollPattern rp3 = new RollPattern("5d6+lol+1d3+4");
            System.Diagnostics.Trace.WriteLine("rp3: " + rp3.ToString());

            for (int i = 0; i < 2; i++)
            {
                rp1.run();
            }

            for (int i = 0; i < 3; i++)
            {
                rp2.run();
            }

            for (int i = 0; i < 4; i++)
            {
                rp3.run();
            }
        }
    }
}
