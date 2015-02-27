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
        DnDController controller;

        //private delegate void NoArgDelegate();
        
        public MainWindow()
        {
            InitializeComponent();
            DicePatternTxt.Text = Data.SQLiteDBWrapper.getReference().GetLastPattern();

            controller = new DnDController(DiceChart);

            controller.SetRollPattern(DicePatternTxt.Text);

            controller.DrawGraph();         
        }

        private void DiceChart_Loaded(object sender, RoutedEventArgs e)
        {
            controller.DrawGraph();
        }

        private void RollBtn_Click(object sender, RoutedEventArgs e)
        {
            RollBtn.IsEnabled = false;
            String pattern = DicePatternTxt.Text;
            
            //Create the worker thread to handle the execution
            new System.Threading.Thread(() =>
            {
                Data.SQLiteDBWrapper.getReference().SetLastPattern(pattern);                
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (controller.SetRollPattern(pattern))
                    {
                        int result = controller.Roll();
                        Resultbox.Content = result.ToString();
                        if (result > controller.RollMean() + controller.RollStdDev())
                        {
                            RollAppraisal.Content = "Great!";
                        }
                        else if (result > controller.RollMean())
                        {
                            RollAppraisal.Content = "Good!";
                        }
                        else if (result < controller.RollMean() - controller.RollStdDev())
                        {
                            RollAppraisal.Content = "Terrible!";
                        }
                        else if (result < controller.RollMean())
                        {
                            RollAppraisal.Content = "Subpar";
                        }
                        else
                        {
                            RollAppraisal.Content = "Average";
                        }
                    }
                }));
            }).Start();

            //Create a delayed thread to re-enable the roll button.
            new System.Threading.Thread(() =>
            {
                System.Threading.Thread.Sleep(1000); //1 second
                this.Dispatcher.BeginInvoke(new Action(() => {
                    RollBtn.IsEnabled = true;
                }));
            }).Start();
            //new NoArgDelegate(new Action(() => { thing(pattern); })).BeginInvoke(null, null);                 
        }

        private void ResetSession_Click(object sender, RoutedEventArgs e)
        {
            controller.ResetHistory();
        }        
    }
}
