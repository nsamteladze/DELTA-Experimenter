using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DELTA_Patcher.DataModel
{
    public interface ICopyable
    {
        ICopyable CreateCopy();
    }
}
