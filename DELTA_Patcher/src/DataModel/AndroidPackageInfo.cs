using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DELTA_Patcher.Data_Model
{
    public class AndroidPackageInfo : IApplicationPackage
    {
        public string PackageName
        {
            get;
            private set;
        }

        public string AppName
        {
            get;
            private set;
        }

        public long CodeVersion
        {
            get;
            private set;
        }

        public AndroidPackageInfo(string packageName, string appName, long codeVersion)
        {
            this.PackageName = packageName;
            this.AppName = appName;
            this.CodeVersion = codeVersion;
        }

        public string GetPackageName()
        {
            return PackageName;
        }

        public string GetApplicationName()
        {
            return AppName;
        }

        public long GetCodeVersion()
        {
            return CodeVersion;
        }

        public ICopyable CreateCopy()
        {
            return (new AndroidPackageInfo(PackageName, AppName, CodeVersion));
        }
    }
}
