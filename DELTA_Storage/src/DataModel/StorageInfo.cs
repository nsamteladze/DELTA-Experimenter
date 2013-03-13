using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DELTA_Common.DataModel;

namespace DELTA_Storage.DataModel
{
    // TODO: Document
    [Serializable, XmlRoot("storage")]
    public class StorageInfo
    {
        #region Properties

        [XmlElement("name")]
        public string StorageName { get; set; }
        [XmlElement("applications")]
        public SerializableDictionary<string, int> AppsInStorage { get; set; }

        #endregion Properties

        #region Constructors

        // Private parametless contructor is required for serialization
        private StorageInfo()
        {
            StorageName = String.Empty;
            AppsInStorage = new SerializableDictionary<string, int>();
        }

        public StorageInfo(string storageName, IDictionary<string, int> apps)
        {
            StorageName = storageName;
            AppsInStorage = new SerializableDictionary<string, int>(apps);
        }

        public StorageInfo(string storageName)
        {
            StorageName = storageName;
            AppsInStorage = new SerializableDictionary<string, int>();
        }

        #endregion Constructors

        // TODO: Document
        public bool ContainsApp(string appName)
        {
            return AppsInStorage.ContainsKey(appName);
        }

        // TODO: Document
        public void AddApplication(string appName)
        {
            int numVersions;


            if (AppsInStorage.TryGetValue(appName, out numVersions))
            {
                numVersions += 1;
                AppsInStorage[appName] = numVersions;
            }
            else AppsInStorage.Add(appName, 1);
        }
    }
}
