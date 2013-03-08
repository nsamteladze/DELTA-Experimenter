using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DELTA_Patcher.Data_Model;

namespace DELTA_Patcher
{
    public static class PatchingAlgorithmFactory
    {
        public static IPatchingAlgorithm ConstructAlgorithm(EnumPatchingAlgorithms algorithm)
        {
            switch (algorithm)
            {
                case EnumPatchingAlgorithms.DELTA: 
                    return new AndroidDELTAAlgorithm();
                default:
                    return null;
            }
        }
    }
}
