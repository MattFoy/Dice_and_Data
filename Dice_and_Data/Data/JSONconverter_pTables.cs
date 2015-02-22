using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice_and_Data.Data
{
    public static class JSONconverter_pTables
    {
        //naieve implementation for internal use only

        public static String Dict2JSON(Dictionary<int, double> pTable)
        {
            StringBuilder sb = new StringBuilder("{");

            foreach (KeyValuePair<int,double> kvp in pTable)
            {
                sb.Append("{").Append(kvp.Key).Append(":").Append(kvp.Value).Append("}").Append(",");
            }

            sb.Remove(sb.Length-1, 1);
            sb.Append("}");
            return sb.ToString();
        }

        public static Dictionary<int, double> JSON2Dict(String json)
        {
            Dictionary<int, double> result = new Dictionary<int, double>();
            System.Diagnostics.Trace.WriteLine("Attempting to parse: " + json);

            //Trim any whitespace
            json = json.Trim();

            if (json.Length < 3)
            {
                return new Dictionary<int,double>();
            }
            //Trim the leading and trailing brackets
            json = json.Substring(1, json.Length - 2);

            foreach (String entry in json.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                //So we aren't messing up the iteration variable
                String s = entry;

                //Trim any whitespace
                s = s.Trim();

                //Trim the leading and trailing brackets
                s = s.Substring(1, s.Length - 2);

                String[] kvp = s.Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                int key = Int16.Parse(kvp[0]);
                double value = Double.Parse(kvp[1]);
                result.Add(key, value);
            }

            return result;
        }

    }
}
