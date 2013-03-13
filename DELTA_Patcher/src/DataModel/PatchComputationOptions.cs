using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DELTA_Patcher.DataModel
{
    /// <summary>
    /// Options used in Patcher during patches computation for an application in storage
    /// </summary>
    public class PatchComputationOptions
    {
        #region Properties

        public int NumberOfLatestApplicationVersionsUsed { get; set; }

        public EnumPatchComputedBetweenOptions PatchComputedBetween { get; set; }

        public string OutputPatchDir { get; set; }

        #endregion

        #region Constructors

        public PatchComputationOptions(int numberOfLaterstApplicationVersionsUsed, EnumPatchComputedBetweenOptions patchComputedBetween)
        {
            NumberOfLatestApplicationVersionsUsed = numberOfLaterstApplicationVersionsUsed;
            PatchComputedBetween = patchComputedBetween;
        }

        #endregion
    }
}
