using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DELTA_Patcher.Data_Model
{
    public interface IDeltaEncodingAlgorithm
    {
        void ComputeDelta(ApplicationPackage reference, ApplicationPackage target, string pathOutputPatch);
    }
}
