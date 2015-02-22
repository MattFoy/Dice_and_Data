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

        public static void Work2()
        {
            String[] dice = new String[] { "d4", "d6", "d8", "d10", "d12", "d20" };
            int [] diceBounds = Enumerable.Repeat(0, dice.Length).ToArray();
            
            // Step 1: Calculate diceBounds.
            for (int i = 0; i < dice.Length; i++)
            {
                for(int j = 2; j <= 20; j++)
                {
                    RollPattern rp = new RollPattern(j + dice[i]);
                    double pSum = 0;
                    for (int k = rp.Min; k <= rp.Max; k++)
                    {
                        pSum += rp.p(k);
                    }
                    if (Math.Abs(1 - pSum) < 0.01)
                    {
                        continue;
                    }
                    else
                    {
                        diceBounds[i] = j;
                        break;
                    }
                }                
            }

            //Step 2:
            int[] itr = Enumerable.Repeat(0, dice.Length).ToArray();
            long tested = 0;
            int added = 0;
            while (true)
            {
                //Step 2.1: Increment the itr array
                bool carry = true;
                for (int i = 0; i < itr.Length; i++)
                {
                    if (carry)
                    {
                        if (++itr[i] % diceBounds[i] == 0)
                        {
                            if (i == dice.Length - 1) 
                            { 
                                return; 
                            }
                            else
                            {
                                itr[i] = 0;
                                carry = true;
                            }
                        }
                        else
                        {
                            carry = false;
                        }                        
                    }
                }

                String pattern = "";
                for (int i = 0; i < itr.Length; i++)
                {
                    if (itr[i] > 0)
                    {
                        pattern += itr[i] + dice[i] + "+";
                    }
                }
                pattern = pattern.TrimEnd(new char[] { '+' });
                String status;
                if (!SQLiteDBWrapper.getReference().CheckCache(pattern).IsValid())
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
