using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFTDataWorker.Models.DataModels
{
    public class Set
    {
        public string set { get; set; }
        public string url { get; set; }
        public string name { get; set; }
        public bool isExtracted { get; set; }
        public bool isCurrent { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Set)
            {
                return ((Set)obj).set == this.set;
            }
            return base.Equals(obj);
        }
    }
}
