using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Dice_and_Data
{
    class RollPattern
    {
        private string patternString = "";
        private List<RollPlan> rolls = new List<RollPlan>();
        private int constant = 0;

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
