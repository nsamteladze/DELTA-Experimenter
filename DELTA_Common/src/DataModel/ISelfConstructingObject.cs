using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DELTA_Common.DataModel
{
    public interface ISelfConstructingObject
    {
        bool ConstructObject(List<List<string>> data);
    }
}
