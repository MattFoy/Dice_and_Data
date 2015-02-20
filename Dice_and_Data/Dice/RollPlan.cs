using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice_and_Data
{
    class RollPlan
    {
        public int diceCount;
        public int sides;

        public RollPlan(int diceCount, int sides)
        {
            this.diceCount = diceCount;
            this.sides = sides;
            if ((diceCount <= 0) || (sides <= 0)) {
                throw new Exception("Invalid RollPlan: (" + diceCount + ", " + sides + ") Parameters must be positive integers.");
            }
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
