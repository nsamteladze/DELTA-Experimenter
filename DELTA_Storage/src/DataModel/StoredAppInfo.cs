using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DELTA_Common.DataModel;

namespace DELTA_Storage.DataModel
{
    // TODO: Document
    [Serializable, XmlRoot("application")]
    public class StoredAppInfo
    {
        #region Properties

        [XmlElement("name")]
        public string AppPackageName { get; set; }
        [XmlElement("label")]
        public string AppName { get; set; }
        [XmlElement("versions")]
        public SerializableDictionary<long, string> AppVersions { get; set; }

        #endregion Properties

        #region Constructors

        // Private parametless contructor is required for serialization
        private StoredAppInfo()
        {
            AppPackageName = String.Empty;
            AppName = String.Empty;
            AppVersions = new SerializableDictionary<long, string>();
        }

        public StoredAppInfo(string appPackageName, string appName, IDictionary<long, string> appVersions)
        {
            AppPackageName = appPackageName;
            AppName = appName;
            AppVersions = new SerializableDictionary<long, string>(appVersions);
        }

        public StoredAppInfo(string appPackageName, string appName)
        {
            AppName = appPackageName;
            AppName = appName;
            AppVersions = new SerializableDictionary<long, string>();
        }

        #endregion Constructors

        public void AddVersion(long appVersion, string appPackageFileName)
        {
            AppVersions.Add(appVersion, appPackageFileName);
        }

        // TODO: Document
        public bool ContainsVersion(long version)
        {
            return AppVersions.ContainsKey(version);
        }
    }
}
