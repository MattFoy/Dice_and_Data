using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dice_and_Data.Dice;

namespace Dice_and_Data
{
    class RollPlan
    {
        public int diceCount;
        public int sides;

        public int min;
        public int max;
        public int pDenominator;

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
            pTable = new Dictionary<int, double>();
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
                    return p(newX);
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
    }
}
