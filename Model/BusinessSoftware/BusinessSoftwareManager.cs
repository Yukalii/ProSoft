using EasySave.Model.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model.BusinessSoftware
{
    public class BusinessSoftwareManager : IBusinessSoftwareManager
    {
        private readonly ConfigManager _configManager;

        public BusinessSoftwareManager(ConfigManager configManager)
        {
            _configManager = configManager;
        }

        public bool SoftwareIsRunning()
        {
            string processName = _configManager.Config.BusinessApp;

            if (string.IsNullOrWhiteSpace(processName))
            {
                return false;
            }

            return Process.GetProcessesByName(processName).Length > 0;
        }
    }
}
