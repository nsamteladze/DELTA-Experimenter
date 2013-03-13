using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DELTA_Patcher.Data_Model
{
    public interface IApplicationPackage : ICopyable
    {
        string GetPackageName();
        string GetApplicationName();
        long GetCodeVersion();
        string GetPackagePath();
    }
}
