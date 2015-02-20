﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice_and_Data
{
    static class DiceRoller
    {
        private static Random myRand = new Random();

        private static int rollCount = 0;

        public static int RollCount { 
            get { return rollCount; } 
            set { System.Diagnostics.Trace.WriteLine(value); } 
        }

        public static int d(int sides)
        {
            rollCount++;
            return myRand.Next(1, sides);
        }

        public static int roll(int diceCount, int sides)
        {
            int sum = 0;
            for (int i = 0; i < diceCount; i++)
            {
                sum += d(sides);
            }
            return sum;
        }
    }
}
