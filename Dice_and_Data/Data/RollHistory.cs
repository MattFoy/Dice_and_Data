using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice_and_Data.Data
{
    class RollHistory : Dictionary<int,int>
    {        
        public double p(int i)
        {
            double totalRolls = 0;
            double iRolls = 0;
            foreach (KeyValuePair<int,int> kvp in this)
            {
                totalRolls += kvp.Value;
                if (kvp.Key == i)
                {
                    iRolls = kvp.Value;
                }
            }
            return (iRolls/totalRolls);
        }

        public int Rolls()
        {
            int rolls = 0;
            foreach (KeyValuePair<int, int> kvp in this)
            {
                rolls += kvp.Value;
            }
            return rolls;
        }

        public double highestP()
        {
            int maxValue = 0;
            int maxValuesKey = 0;
            foreach (KeyValuePair<int, int> kvp in this)
            {
                if (kvp.Value > maxValue)
                {
                    maxValue = kvp.Value;
                    maxValuesKey = kvp.Key;
                }
            }
            return p(maxValuesKey);
        }

        public double Mean()
        {
            double sum = 0.0;
            int total = 0;
            foreach (KeyValuePair<int, int> kvp in this)
            {
                sum += kvp.Key * kvp.Value;
                total += kvp.Value;
            }
            return sum / total;
        }

        public double StandardDeviation()
        {
            double mean = Mean();

            double sum = 0.0;
            foreach (KeyValuePair<int, int> kvp in this)
            {
                sum += p(kvp.Key) * Math.Pow(mean - kvp.Value, 2);
            }
            return Math.Sqrt(sum);
        }
    }
}
