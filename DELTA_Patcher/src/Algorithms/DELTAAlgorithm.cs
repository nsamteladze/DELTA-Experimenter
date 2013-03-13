using DELTA_Patcher.DataModel;
using DELTA_Common.DataModel;

namespace DELTA_Patcher.Algorithms
{
    /// <summary>
    /// Platform-independent
    /// </summary>
    public class DELTAAlgorithm : IPatchingAlgorithm 
    {
        public void ComputePatch(ApplicationPackage reference, ApplicationPackage target, string pathOutputPatch)
        {
            IDeltaEncodingAlgorithm deltaEncodingAlgorithm = new BSDIFFDeltaEncodingAlgortihm();
            deltaEncodingAlgorithm.ComputeDelta(reference.PackagePath, target.PackagePath, pathOutputPatch);
        }
    }
}
