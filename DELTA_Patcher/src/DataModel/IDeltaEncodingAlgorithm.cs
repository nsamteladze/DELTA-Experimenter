using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DELTA_Patcher.DataModel
{
    public interface IDeltaEncodingAlgorithm
    {
        void ComputeDelta(string pathReference, string pathTarget, string pathOutputPatch);
    }
}
