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

            //roll 1d10+3
            int result = DiceRoller.d(10) + 3;
            wrapper.RecordRoll("1d10+3", result);

            //roll 4d8... 5 times
            for (int i = 0; i < 5; i++)
            {
                result = DiceRoller.roll(4, 8);
                wrapper.RecordRoll("4d8", result);
            }


        }
    }
}
