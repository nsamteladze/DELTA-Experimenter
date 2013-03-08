using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DELTA_Patcher.Data_Model
{
    public class ApplicationInExperiment
    {
        public string PackageName { get; set; }
        public int NumOfLatestVersionsToUse { get; set; }

        public ApplicationInExperiment(string packageName, int numOfLatestVersionsToUse)
        {
            this.PackageName = packageName;
            this.NumOfLatestVersionsToUse = numOfLatestVersionsToUse;
        }
    }
}
