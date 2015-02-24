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
    }
}
