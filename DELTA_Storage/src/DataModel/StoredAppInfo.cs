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
        public string AppName { get; set; }
        [XmlElement("label")]
        public string AppLabel { get; set; }
        [XmlElement("versions")]
        public SerializableDictionary<long, string> AppVersions { get; set; }

        #endregion Properties

        #region Constructors

        // Private parametless contructor is required for serialization
        private StoredAppInfo()
        {
            AppName = String.Empty;
            AppLabel = String.Empty;
            AppVersions = new SerializableDictionary<long, string>();
        }

        public StoredAppInfo(string appName, string appLabel, IDictionary<long, string> appVersions)
        {
            AppName = appName;
            AppLabel = appLabel;
            AppVersions = new SerializableDictionary<long, string>(appVersions);
        }

        public StoredAppInfo(string appName, string appLabel)
        {
            AppName = appName;
            AppLabel = appLabel;
            AppVersions = new SerializableDictionary<long, string>();
        }

        #endregion Constructors

        public void AddVersion(long appVersion, string apkFileName)
        {
            AppVersions.Add(appVersion, apkFileName);
        }

        // TODO: Document
        public bool ContainsVersion(long version)
        {
            return AppVersions.ContainsKey(version);
        }
    }
}
