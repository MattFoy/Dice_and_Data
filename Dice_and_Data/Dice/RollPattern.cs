using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Dice_and_Data.Dice;

namespace Dice_and_Data
{
    class RollPattern
    {
        private string patternString = "";
        private List<RollPlan> rolls = new List<RollPlan>();
        private int constant = 0;
        private int max = 0;
        public int Max { get { return max; } set { } }
        private int min = 0;
        public int Min { get { return min; } set { } }
        private double mean = 0;
        public double Mean { get { return mean; } set { } }
        private double variance = 0;
        public double Variance { get { return variance; } set { } }
        public double StandardDeviation { get { return Math.Sqrt(variance); } set { } }

        private Dictionary<int, double> pTable;

        public RollPattern(String rollString)
        {
            this.patternString = rollString;

            rollString = ValidatePattern(rollString);

            if (rollString.Length > 0)
            {
                System.Diagnostics.Trace.WriteLine(rollString + " is good!");
                //matches the "1d4" type strings
                Regex regex = new Regex("^([0-9]+d[0-9]+)$");
                String[] explode = rollString.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in explode) {                    
                    if (regex.IsMatch(s))
                    {
                        // ie. s == "1d4"
                        String[] rawRollPlan = s.Split(new char[] { 'd' }, StringSplitOptions.RemoveEmptyEntries);
                        int dCount = Int32.Parse(rawRollPlan[0]);
                        int sideCount = Int32.Parse(rawRollPlan[1]);
                        if (dCount > 0 && sideCount > 0)
                        {
                            min += dCount;
                            max += dCount * sideCount;
                            rolls.Add(new RollPlan(dCount, sideCount));
                        }
                    }
                    else 
                    {
                        // ie. s == "3"
                        constant += Int32.Parse(s);
                    }
                }
                this.patternString = this.ToString();
                GenerateProbabilityDistribution();
            }
            else
            {
                System.Diagnostics.Trace.WriteLine(rollString + " is BAD!");
                throw new DicePatternFormatException("Invalid dice pattern format.");
            }
        }

        public int run()
        {
            int result = constant;
            foreach (RollPlan rp in rolls)
            {
                result += rp.execute();
            }
            SQLiteDBWrapper.getReference().RecordRoll(this.ToString(), result);
            return result;
        }

        public double p(int x)
        {
            if (pTable.ContainsKey(x))
            {
                return pTable[x];
            }
            else
            {
                return 0.0;
            }            
        }

        private void GenerateProbabilityDistribution()
        {
            // (re)Initialize the table
            pTable = new Dictionary<int, double>(); 
            
            if (rolls.Count == 0)
            {
                //Well shit, it's just a constant.
            }
            else if (rolls.Count == 1)
            {
                for (int i = min; i <= max; i++)
                {
                    pTable.Add(i, rolls[0].p(i));
                }
                mean = rolls[0].E();
            }
            else
            {
                // Step 1: Generate a list of all the list of possible outcomes from each RollPlan. 
                // (Also calculate the mean while we're traversing this collection)
                List<List<int>> sets = new List<List<int>>();
                double avgSum = 0;
                int totalDice = 0;
                foreach (RollPlan rp in rolls)
                {
                    avgSum += rp.E();
                    totalDice += rp.diceCount;
                    for (int j = 0; j < rp.diceCount; j++)
                    {
                        List<int> possibilities = new List<int>();
                        for (int k = 1; k <= rp.sides; k++)
                        {
                            possibilities.Add(k);
                        }
                        sets.Add(possibilities);
                    }
                }
                mean = avgSum / rolls.Count;

                // Step 2: Generate the cartesian product of these sets with LINQ magic
                IEnumerable<IEnumerable<int>> result = sets
                    .Select(list => list.AsEnumerable())
                    .CartesianProduct();


                // Step 3: Create an array to store combination totals
                int[] possibleCombinations = Enumerable.Repeat(0, max - min + 1).ToArray();

                int totalSum = 0;
                foreach (IEnumerable<int> topLvl in result)
                {
                    int currentSum = 0;
                    foreach (int i in topLvl)
                    {
                        System.Diagnostics.Trace.Write(i + ", ");
                        currentSum += i;
                    }
                    System.Diagnostics.Trace.WriteLine("");
                    totalSum += 1;
                    possibleCombinations[(currentSum - min)]++;
                }

                // Step 4: Calculate how many possibilities each value weighs relative to the total number.
                for (int i = 0; i < possibleCombinations.Length; i++)
                {
                    pTable.Add(i + min, ((double)possibleCombinations[i] / (double)totalSum));
                }
            }

            // Step 5: Calculate the variance
            double sqrdDiffTotal = 0;
            foreach (KeyValuePair<int, double> entry in pTable)
            {
                sqrdDiffTotal += Math.Pow(entry.Key - mean, 2);
            }
            variance = sqrdDiffTotal / pTable.Count;
            //System.Diagnostics.Trace.WriteLine("whoa!");
            
        }

        

        public static String ValidatePattern(String pattern)
        {
            // Matches all whitespace characters
            Regex regex = new Regex("\\s*");

            // Replaces all whitespace characters with nothing; thereby removing them.
            pattern = regex.Replace(pattern, "");

            // Forces all uppercase letters to lowercase
            pattern = pattern.ToLower();

            //matches "1d4", "1d4+3", "2d4+3d5", "2d4+3+5d6+8+1d3", etc
            regex = new Regex(@"^(([0-9]+d[0-9]+)|([0-9]+))(\+(([0-9]+d[0-9]+)|([0-9]+)))*$");

            if (regex.IsMatch(pattern))
            { 
                return pattern; 
            }
            else
            {
                return "";
            }
        }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            String res = "";
            foreach(RollPlan rp in rolls) {
                sb.Append(rp.diceCount).Append("d").Append(rp.sides).Append("+");
            }
            if (rolls.Count > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            if (constant != 0)
            {
                if (constant > 0)
                {
                    sb.Append("+");
                }
                else if (constant < 0)
                {
                    sb.Append("-");
                }
                sb.Append(Math.Abs(constant));
            }
            res = sb.ToString();
            return res;
        }
    }
}
