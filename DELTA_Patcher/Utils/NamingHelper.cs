using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DELTA_Patcher.Data_Model;

namespace DELTA_Patcher.Utils
{
    public static class NamingHelper
    {
        public static string GetPatchName(ApplicationPackage reference, ApplicationPackage target)
        {
            return (String.Format("{0}-{1}-{2}.deltapatch", reference.PackageName, reference.CodeVersion, target.CodeVersion));
        }
    }
}
