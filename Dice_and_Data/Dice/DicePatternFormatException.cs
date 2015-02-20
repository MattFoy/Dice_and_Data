using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice_and_Data
{
    class DicePatternFormatException : FormatException
    {
        public DicePatternFormatException(string message) : base(message) 
        {

        }

        public DicePatternFormatException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
