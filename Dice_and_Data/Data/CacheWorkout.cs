using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dice_and_Data.Dice;

namespace Dice_and_Data.Data
{
    static class CacheWorkout
    {
        static System.Threading.Thread worker;
        static bool stopRequested = false;

        public static void Start() {
            stopRequested = false;
            worker = new System.Threading.Thread(Work);
            worker.Start();
        }
        public static void Stop()
        {
            stopRequested = true;
        }

        public static void Work()
        {
            string[] diceSuffixes = new string[] { "d4", "d6", "d8", "d10", "d12", "d20" };
            int[] looseBounds = new int[] { 15, 12, 11, 10, 10, 7 };
            int added = 0;
            int tested = 0;

            for (int i = 1; i < 30; i++)
            { //i is how many dice in TOTAL we will be rolling
                if (stopRequested) { return; }
                //int choices = Dice.AdvMath.choose(diceSuffixes.Length + i - 1, i);
                List<List<int>> sets = new List<List<int>>();
                for (int j = 0; j < i; j++)
                { // j is just to perform the following operations as many times as there are dice
                    List<int> add = new List<int>();
                    for (int k = 0; k < diceSuffixes.Length; k++)
                    {
                        add.Add(k);
                    }
                    sets.Add(add);
                }
                if (stopRequested) { return; }
                IEnumerable<IEnumerable<int>> result = sets.Select(list => list.AsEnumerable()).CartesianProduct();
                foreach (IEnumerable<int> combo in result)
                {
                    if (stopRequested) { return; }
                    int[] count = Enumerable.Repeat(0, diceSuffixes.Length).ToArray();
                    foreach (int j in combo)
                    {
                        count[j]++;
                    }
                    String pattern = "";
                    bool worthTrying = true;
                    for (int j = 0; j < diceSuffixes.Length; j++)
                    {
                        if (count[j] > 0)
                        {
                            if (count[j] >= looseBounds[j]) { worthTrying = false; }
                            pattern += count[j] + diceSuffixes[j] + "+";
                        }
                    }
                    pattern = pattern.TrimEnd(new char[] { '+' });
                    System.Diagnostics.Trace.WriteLine("Checking: " + pattern);
                    String status = "";
                    if (worthTrying && !SQLiteDBWrapper.getReference().CheckCache(pattern).IsValid())
                    {
                        status = "Adding:";
                        added++;
                        RollPattern rpTest = new RollPattern(pattern);
                    }
                    else
                    {
                        status = "ABORTING:";
                        tested++;
                    }
                    System.Diagnostics.Trace.WriteLine("[" + added + " / " + tested + "] " + status + " " + pattern);
                }

            }

        }
    }
}
