using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dice_and_Data.Dice;
using Dice_and_Data.Data;

namespace Dice_and_Data
{
    class RollPlan
    {
        public int diceCount;
        public int sides;

        public int min;
        public int max;
        public int pDenominator;

        private long calcTime;
        private Dictionary<int, double> pTable;

        public RollPlan(int diceCount, int sides)
        {            
            this.diceCount = diceCount;
            this.sides = sides;

            if ((diceCount <= 0) || (sides <= 0)) {
                throw new Exception("Invalid RollPlan: (" + diceCount + ", " + sides + ") Parameters must be positive integers.");
            }

            min = diceCount;
            max = diceCount * sides;
            pDenominator = (int)Math.Pow(sides, diceCount);

            RollPartial partial = SQLiteDBWrapper.getReference().CheckCache(sides, diceCount);
            if (partial.IsValid())
            {
                System.Diagnostics.Trace.WriteLine("Cache HIT! Loading probability distribution now...");
                pTable = partial.pTable;
                calcTime = partial.calcTime;
                System.Diagnostics.Trace.WriteLine("Loaded pTable from SQLite database: " + diceCount + "d" + sides);
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("Cache miss! Building probability distribution now...");
                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                timer.Start();
                pTable = new Dictionary<int, double>();
                double pTotal = 0.0;
                for (int i = min; i <= max; i++)
                {
                    pTotal += p(i);
                }
                timer.Stop();
                this.calcTime = timer.ElapsedMilliseconds;
                if (Math.Abs(1 - pTotal) < 0.02)
                {
                    CacheResults();
                }
                else
                {

                }
            }            
        }

        public double p(int x)
        {
            if (pTable.ContainsKey(x))
            {
                return pTable[x];
            }
            else
            {
                int middleMark = (int)Math.Floor((double)(max + min) / (double)2);
                if (x > middleMark)
                {
                    int newX = (max - x) + min;
                    //System.Diagnostics.Trace.WriteLine("x:(" + x + "->" + newX + "), min: " + min + ", max: " + max + ", middle: " + middleMark);
                    double r = p(newX);
                    pTable.Add(x, r);
                    return r;
                }
                else
                {
                    double result = 0.0;
                    int thing = (int)Math.Floor((double)(x - diceCount) / (double)sides);

                    for (int k = 0; k <= thing; k++)
                    {
                        result += Math.Pow(-1, k) * AdvMath.choose(diceCount, k) * AdvMath.choose(x - (sides * k) - 1, diceCount - 1);
                    }

                    result /= pDenominator;
                    pTable.Add(x, result);
                    return result;
                }
            }            
        }

        public double E()
        {
            return diceCount * ((double)(sides + 1) / (double)2);
        }

        public double variance()
        {
            return (double)(diceCount * (Math.Pow(sides, 2) - 1)) / (double)(max - min + 1);
        }

        public double stdDev()
        {
            return Math.Sqrt(variance());
        }

        public int execute()
        {
            int sum = 0;
            for (int i = 0; i < diceCount; i++)
            {
                sum += Roll.d(sides);
            }
            return sum;
        }

        private void CacheResults()
        {
            SQLiteDBWrapper db = SQLiteDBWrapper.getReference();
            db.CacheRollPlan(diceCount, sides, min, max, E(), stdDev(), JSONconverter_pTables.Dict2JSON(pTable), calcTime);
            System.Diagnostics.Trace.WriteLine(this.ToString() + " cached in SQLite database.");
        }
    }
}
