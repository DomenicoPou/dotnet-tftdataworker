using CommonLibrary.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFTDataWorker.Models;
using TFTDataWorker.Models.DataModels;

namespace TFTDataWorker.Handlers
{
    public class ApplicationConfigHandler
    {
        public ApplicationConfiguration configuration;
        public ApplicationConfigHandler()
        {
            configuration = ConfigurationHandler<ApplicationConfiguration>.ReadConfig();

            if (configuration == null) configuration = new ApplicationConfiguration();
            if (configuration.CurrentTftSet == null) configuration.CurrentTftSet = "null";
            if (configuration.AllTftSet == null) configuration.AllTftSet = new List<Set>();
            if (configuration.DataFolder == null) configuration.DataFolder = "";

            ConfigurationHandler<ApplicationConfiguration>.WriteConfig(configuration);
        }

        public void save()
        {
            ConfigurationHandler<ApplicationConfiguration>.WriteConfig(configuration);
        }
    }
}
