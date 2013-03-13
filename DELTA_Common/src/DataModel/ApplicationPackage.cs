using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DELTA_Common.DataModel
{
    [Serializable, XmlRoot("applicationPackage")]
    public abstract class ApplicationPackage
    {
        #region Properties

        [XmlIgnore]
        public string PackagePath 
        { 
            get; 
            private set; 
        }

        [XmlElement("packageName")]
        public string PackageName
        {
            get;
            private set;
        }

        [XmlElement("applicationName")]
        public string ApplicationName
        {
            get;
            private set;
        }

        [XmlElement("codeVersion")]
        public long CodeVersion
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        protected ApplicationPackage(string packagePath, string packageName, string appName, long codeVersion)
        {
            this.PackagePath = packagePath;
            this.PackageName = packageName;
            this.ApplicationName = appName;
            this.CodeVersion = codeVersion;
        }

        #endregion
    }
}
