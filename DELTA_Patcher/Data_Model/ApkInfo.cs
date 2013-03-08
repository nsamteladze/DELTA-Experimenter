using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DELTA_Patcher.Data_Model
{
    /// <summary>
    /// Contains information about APK file (package name, code version, application label)
    /// </summary>
    [Serializable, XmlRoot("apk")]
    public class ApkInfo
    {
        #region Properties

        [XmlElement("package")]
        public string PackageName { get; set; }
        [XmlElement("label")]
        public string AppLabel { get; set; }
        [XmlElement("codeVersion")]
        public long CodeVersion { get; set; }

        #endregion Properties

        #region Constructors

        public ApkInfo()
        {
            PackageName = String.Empty;
            AppLabel = String.Empty;
            CodeVersion = 0;
        }

        public ApkInfo(string packageName, string appLabel, long codeVersion)
        {
            PackageName = packageName;
            AppLabel = appLabel;
            CodeVersion = codeVersion;
        }

        #endregion Constructors

    }
}
