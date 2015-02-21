using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dice_and_Data.Dice
{
    static class AdvMath
    {
        // http://blogs.msdn.com/b/ericlippert/archive/2010/06/28/computing-a-cartesian-product-with-linq.aspx
        // also, http://stackoverflow.com/questions/13647662/generating-a-n-ary-cartesian-product-example
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
              emptyProduct,
              (accumulator, sequence) =>
                from accseq in accumulator
                from item in sequence
                select accseq.Concat(new[] { item }));
        }
        
        public static long factorial(int x)
        {
            if (x <= 1)
            {
                return 1;
            }
            else
            {
                return x * factorial(x - 1);
            }
        }

        public static int choose(double a, double b)
        {
            if (a < 0 || b < 0 || (b > a))
            {
                return 1;
            }
            if ((a == b) || (b == 0))
            {
                return 1;
            }

            double result = 1;
            for (int i = 1; i <= b; i++)
            {
                result *= ((a - i + 1) / i);
            }
            return (int)result;
        }
    }
}
