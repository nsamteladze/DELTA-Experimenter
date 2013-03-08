using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DELTA_Patcher.Data_Model;
using DELTA_Patcher.Utils;

namespace DELTA_Patcher
{
    public class AndroidDELTAAlgorithm : IPatchingAlgorithm 
    {
        public void ComputePatch(ApplicationPackage reference, ApplicationPackage target, string pathOutputPatch)
        {
            IDeltaEncodingAlgorithm deltaEncodingAlgorithm = new BSDIFFDeltaEncodingAlgortihm();
            deltaEncodingAlgorithm.ComputeDelta(reference, target, pathOutputPatch);
        }
    }
}
