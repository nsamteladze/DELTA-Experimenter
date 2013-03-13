using DELTA_Common.DataModel;

namespace DELTA_Patcher.DataModel
{
    public interface IPatchingAlgorithm
    {
        void ComputePatch(ApplicationPackage reference, ApplicationPackage target, string pathOutputPatch);
    }
}
