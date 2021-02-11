using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFTDataWorker.Models.DataModels;

namespace TFTDataWorker.Models
{
    public class ApplicationConfiguration
    {
        public string CurrentTftSet { get; set; }
        public List<Set> AllTftSet { get; set; }
        public string DataFolder { get; set; }
    }
}
