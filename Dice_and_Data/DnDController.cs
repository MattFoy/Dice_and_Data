using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice_and_Data
{
    class DnDController
    {
        RollPattern rp;
        Display.CanvasDiceGraph graph;

        public DnDController(System.Windows.Controls.Canvas canvas)
        {
            graph = new Display.CanvasDiceGraph(canvas);
        }

        public bool SetRollPattern(String pattern)
        {
            if (rp != null && rp.ToString(true) == RollPattern.ValidatePattern(pattern))
            {
                return true;
            }
            else if (RollPattern.ValidatePattern(pattern) != "")
            {
                rp = new RollPattern(pattern);
                graph.SetRollPattern(rp);
                graph.Draw();
                return true;
            }
            else
            {
                return false;
            }
        }

        public int Roll()
        {
            if (rp == null)
            {
                return 0;
            }
            else
            {
                int result = rp.run();
                graph.UpdateGraphWithRoll(result);
                return result;
            }
        }

        public void DrawGraph()
        {
            graph.Draw();
        }

        public void ResetHistory()
        {
            Data.SQLiteDBWrapper.getReference().NewSession();
            graph.LoadHistory();
        }

        public double RollMean()
        {
            return rp.Mean;
        }

        public double RollStdDev()
        {
            return rp.StandardDeviation;
        }
    }
}
