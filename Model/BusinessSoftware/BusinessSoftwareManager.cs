using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Model.BusinessSoftware
{
    public class BusinessSoftwareManager : IBusinessSoftwareManager
    {
        public bool SoftwareIsRunning()
        {
            return Process.GetProcessesByName("CalculatorApp").Length > 0;
        }
    }
}
