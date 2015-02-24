using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Dice_and_Data.Dice;
using Dice_and_Data.Data;

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
        public double Mean { get { return mean + constant; } set { } }
        private double variance = 0;
        public double Variance { get { return variance; } set { } }
        public double StandardDeviation { get { return Math.Sqrt(variance); } set { } }
        private long calcTime = 0;
        public long CalcTime { get { return calcTime; } set { } }

        private Dictionary<int, double> pTable;

        public RollPattern(String rollString)
        {
            this.patternString = rollString;

            rollString = ValidatePattern(rollString);

            if (rollString.Length > 0)
            {
                Trace.WriteLine(rollString + " is good!");
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
                            RollPlan replace;
                            if ((replace = rolls.SingleOrDefault(rp => rp.sides == sideCount)) != null)
                            {
                                RollPlan mergedPlan = new RollPlan(replace.diceCount + dCount, replace.sides);
                                rolls.Remove(replace);
                                rolls.Add(mergedPlan);
                            }
                            else
                            {
                                rolls.Add(new RollPlan(dCount, sideCount));
                            }
                        }
                    }
                    else 
                    {
                        // ie. s == "3"
                        constant += Int32.Parse(s);
                    }
                }
                rolls = rolls.OrderBy(r => r.sides).ToList();
                this.patternString = this.ToString(true);
                GenerateProbabilityDistribution();
            }
            else
            {
                Trace.WriteLine(rollString + " is BAD!");
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
            RollPartial partial = SQLiteDBWrapper.getReference().CheckCache(this.ToString(false));
            if (partial.IsValid())
            {
                Trace.WriteLine("[" + this.ToString(false) + "] Cache HIT! Loading combined probability distribution now...");
                this.pTable = partial.pTable;
                this.variance = Math.Pow(partial.stdDev, 2);
                this.min = partial.min;
                this.max = partial.max;
                this.mean = partial.mean;
                this.calcTime = partial.calcTime;
                
            }
            else
            {
                Trace.WriteLine("[" + this.ToString(false) + "] Cache miss! Generating combined probability distribution now...");
                Stopwatch timer = new Stopwatch();
                timer.Start();

                // (re)Initialize the table
                pTable = new Dictionary<int, double>(); 

                if (rolls.Count == 0)
                {
                    // It's just a constant?
                }
                else if (rolls.Count == 1)
                {
                    //Not a strictly necessary if block, but there's no need to do the whole algorithm on such a simple case
                    for (int i = min; i <= max; i++)
                    {
                        pTable.Add(i, rolls[0].p(i));
                    }
                    mean = rolls[0].E();
                }
                else
                {
                    //TODO: work some magic to get around the ridiculous performance issues caused by n-ary cartesian products
                    // Step 1: Generate a list of all the list of possible outcomes from each RollPlan. 
                    // (Also calculate the mean while we're traversing this collection)
                    List<List<KeyValuePair<int, double>>> sets = new List<List<KeyValuePair<int, double>>>();
                    double avgSum = 0;
                    foreach (RollPlan rp in rolls)
                    {
                        avgSum += rp.E();
                        List<KeyValuePair<int, double>> possibilities = new List<KeyValuePair<int, double>>();
                        for (int k = rp.min; k <= rp.max; k++)
                        {
                            possibilities.Add(new KeyValuePair<int, double>(k, rp.p(k)));
                        }
                        sets.Add(possibilities);
                    }
                    mean = avgSum;

                    // Step 2: Generate the cartesian product of these sets with LINQ magic
                    IEnumerable<IEnumerable<KeyValuePair<int, double>>> result = sets
                        .Select(list => list.AsEnumerable())
                        .CartesianProduct();


                    // Step 3: Create an array to store combination totals
                    double[] partialProbabilities = Enumerable.Repeat(0.0, max - min + 1).ToArray();

                    foreach (IEnumerable<KeyValuePair<int, double>> topLvl in result)
                    {
                        int currentSum = 0;
                        double currentP = 1;
                        foreach (KeyValuePair<int, double> kvp in topLvl)
                        {
                            //Trace.Write(kvp + ", ");
                            currentSum += kvp.Key;
                            currentP *= kvp.Value;
                        }
                        //Trace.WriteLine("");
                        partialProbabilities[(currentSum - min)] += currentP;
                    }

                    // Step 4: Calculate how many possibilities each value weighs relative to the total number.
                    for (int i = 0; i < partialProbabilities.Length; i++)
                    {
                        pTable.Add(i + min, partialProbabilities[i]);
                    }
                }


                // Step 5: Calculate the variance using the forumla  SUM(i=min;i<=max): (xi - mean)^2 * p(xi)
                // (Credit to http://www.stat.yale.edu/Courses/1997-98/101/rvmnvar.htm)
                variance = 0;
                foreach (KeyValuePair<int, double> entry in pTable)
                {
                    variance += Math.Pow(entry.Key - mean, 2) * entry.Value;
                }
                //Trace.WriteLine("whoa!");
                timer.Stop();
                calcTime = timer.ElapsedMilliseconds;

                double pTotal = 0.0;
                for (int i = min; i <= max; i++)
                {
                    pTotal += pTable[i];
                }
                if (Math.Abs(1 - pTotal) < 0.01)
                {
                    CacheResults();
                }
                else
                {
                    // invalid ptable. :(
                }     
            }            
        }        

        public static String ValidatePattern(String pattern)
        {
            // Matches all whitespace characters
            Regex regex = new Regex("\\s*");

            // Replaces all whitespace characters with nothing; thereby removing them.
            pattern = regex.Replace(pattern, "");

            pattern = pattern.Replace("-", "+-");
            //pattern = pattern.Replace("- ", "-");

            // Forces all uppercase letters to lowercase
            pattern = pattern.ToLower();

            //matches "1d4", "1d4+3", "2d4+3d5", "2d4+3+5d6+8+1d3", etc
            regex = new Regex(@"^(([1-9][0-9]*d[0-9]+)|([1-9][0-9]*))(\+(([1-9][0-9]*d[0-9]+)|((-[1-9]|[1-9])[0-9]*)))*$");

            if (regex.IsMatch(pattern))
            { 
                return pattern; 
            }
            else
            {
                return "";
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public String ToString(Boolean withConstant)
        {
            StringBuilder sb = new StringBuilder();
            String res = "";
            foreach (RollPlan rp in rolls)
            {
                sb.Append(rp.diceCount).Append("d").Append(rp.sides).Append("+");
            }
            if (rolls.Count > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            if (withConstant && constant != 0)
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

        private void CacheResults()
        {
            SQLiteDBWrapper db = SQLiteDBWrapper.getReference();

            db.CacheRollPattern(this.ToString(false), min, max, mean, StandardDeviation, JSONconverter_pTables.Dict2JSON(pTable), calcTime);
            Trace.WriteLine(this.ToString(false) + " cached in SQLite database.");
            /*
            String json1 = JSONconverter_pTables.Dict2JSON(pTable);
            Trace.WriteLine(json1);
            Dictionary<int, double> pTable2 = JSONconverter_pTables.JSON2Dict(json1);
            String json2 = JSONconverter_pTables.Dict2JSON(pTable);
            Trace.WriteLine(json2);
            Trace.WriteLine((json1 == json2) ? "Same!" : "Diff!");
             * */
        }
    }
}
