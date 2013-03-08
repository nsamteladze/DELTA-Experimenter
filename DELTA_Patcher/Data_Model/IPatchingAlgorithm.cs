using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DELTA_Patcher.Data_Model
{
    public interface IPatchingAlgorithm
    {
        void ComputePatch(ApplicationPackage reference, ApplicationPackage target, string pathOutputPatch);
    }
}
