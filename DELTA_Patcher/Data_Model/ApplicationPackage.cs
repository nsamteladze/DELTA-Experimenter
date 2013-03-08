using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DELTA_Patcher.Data_Model
{
    public class ApplicationPackage
    {
        #region Private Data

        private IApplicationPackageInfo _packageInfo;

        #endregion

        #region Properties

        public string PackagePath { get; set; }
        public string ApplicationName
        {
            get { return _packageInfo.GetApplicationName(); }
        }
        public string PackageName
        {
            get { return _packageInfo.GetPackageName(); }
        }
        public long CodeVersion
        {
            get { return _packageInfo.GetCodeVersion(); }
        }

        #endregion

        #region Constructors

        public ApplicationPackage(string packagePath, IApplicationPackageInfo packageInfo)
        {
            PackagePath = packagePath;
            _packageInfo = (IApplicationPackageInfo)packageInfo.CreateCopy();
        }

        #endregion
    }
}
