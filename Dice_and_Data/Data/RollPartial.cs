using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice_and_Data.Data
{
    class RollPartial
    {
        public String pattern;
        public int min;
        public int max;
        public double mean;
        public double stdDev;
        public Dictionary<int, double> pTable;
        public long calcTime;

        public RollPartial(String pattern, int min, int max, double mean, double stdDev, String pTableJSON, long calcTime)
        {
            this.pattern = pattern;
            this.min = min;
            this.max = max;
            this.mean = mean;
            this.stdDev = stdDev;
            this.pTable = JSONconverter_pTables.JSON2Dict(pTableJSON);
            this.calcTime = calcTime;
        }

        public Boolean IsValid()
        {
            if (min < 1 || max < 1)
            {
                //min or max aren't set
                return false;
            }
            if (mean >= max || mean <= min) {
                //invalid mean
                return false;
            }
            if (stdDev > mean)
            {
                //ridiculous stdDev given the nature of dice distributions
                return false;
            }
            if (pTable.Count != (max - min + 1))
            {
                //invalid number of distribution entries?
                return false;
            }
            double pTotal = 0;
            for (int i = min; i <= max; i++)
            {
                if (!pTable.ContainsKey(i))
                {
                    return false;
                }
                else
                {
                    pTotal += pTable[i];
                }                
            }
            if (Math.Abs(1 - pTotal) > 0.1)
            {
                // sum of total probabilities wasn't even close to 1!
                return false;
            }

            // all seems o.k. can't do much more without actually re-calculating the probabilities... which would defeat the purpose.
            return true;
        }
    }
}
