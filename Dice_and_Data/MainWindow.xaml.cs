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
        RollPattern rp;
        Display.CanvasDiceGraph graph;
        
        public MainWindow()
        {
            InitializeComponent();
            Data.SQLiteDBWrapper.getReference();

            //rp = new RollPattern("2d8+1d4+1d6");
            graph = new Display.CanvasDiceGraph((Canvas)this.FindName("DiceChart"));
            //graph.SetRollPattern(rp);            
        }

        private void DiceChart_Loaded(object sender, RoutedEventArgs e)
        {
            graph.Draw();
        }        
    }
}
