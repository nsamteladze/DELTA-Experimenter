using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DELTA_Common.DataModel
{
    public class AndroidApplicationPackage : ApplicationPackage
    {
        #region Constructors

        public AndroidApplicationPackage(string packagePath, string packageName, string appName, long codeVersion)
            : base(packagePath, packageName, appName, codeVersion)
        {
            
        }

        #endregion
    }
}
