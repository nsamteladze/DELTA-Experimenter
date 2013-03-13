using DELTA_Patcher.Algorithms;
using DELTA_Patcher.DataModel;

namespace DELTA_Patcher
{
    public static class PatchingAlgorithmFactory
    {
        public static IPatchingAlgorithm ConstructAlgorithm(EnumPatchingAlgorithms algorithm)
        {
            switch (algorithm)
            {
                case EnumPatchingAlgorithms.DELTA: 
                    return new DELTAAlgorithm();
                default:
                    return null;
            }
        }
    }
}
