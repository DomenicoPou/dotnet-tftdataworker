using Calculator.Source.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Source.Models
{
    class ChampComparer : IComparer<Champion>
    {
        public int Compare(Champion x, Champion y)
        {

            if (x == null || y == null)
            {
                return 0;
            }

            // "CompareTo()" method 
            return x.name.CompareTo(y.name);

        }
    }
}
