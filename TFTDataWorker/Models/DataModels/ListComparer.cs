using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Source.Handlers
{
    class ListComparer : IEqualityComparer<List<object>>
    {
        public bool Equals(List<object> x, List<object> y)
        {
            // TODO - need to do this case insensitively
            //return x.SequenceEqual(y);

            // run a for loop manually to check the equality in order
            // break as soon as not
            if (x.Count != y.Count)
                return false;

            for (int i = 0; i < x.Count; i++)
            {
                if (x[i] == null ^ y[i] == null)    // if only one is null
                {
                    return false;
                }
                else if (x[i] == null && y[i] == null)  // if both are null
                {
                    continue;
                }
                else if (x[i] is string && y[i] is string)      // if both are strings
                {
                    if (!string.Equals((string)x[i], (string)y[i], StringComparison.OrdinalIgnoreCase)) // check equality ignoring the case
                        return false;
                }
                else        // non of the cases above
                {
                    if (!(x[i].Equals(y[i])))
                        return false;
                }
            }

            return true;
        }

        public int GetHashCode(List<object> obj)
        {
            int hashcode = 0;
            foreach (object t in obj)
            {
                if (t != null)      // shouldn't be null, coding to avoid bad data
                {
                    if (t is string)    // if string
                    {
                        hashcode ^= ((string)t).ToLowerInvariant().GetHashCode();   // get hashcode independent of the case
                    }
                    else
                    {
                        hashcode ^= t.GetHashCode();
                    }
                }
            }
            return hashcode;
        }
    }
}
