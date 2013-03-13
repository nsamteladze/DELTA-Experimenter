using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DELTA_Patcher.DataModel
{
    // Determine how patches between versions are computed
    // Patch can be computed between every available version or just between consrcutive versions
    public enum EnumPatchComputedBetweenOptions
    {
            EveryVersion,
            ConsecutiveVersionsOnly
    }
}
