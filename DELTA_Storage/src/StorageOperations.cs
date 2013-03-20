using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using DELTA_Common.DataModel;
using DELTA_Common.Utilities;
using DELTA_Storage.DataModel;

namespace DELTA_Storage
{
    internal static class StorageOperations
    {
        private const string FILE_NAME_APP_INFO = "app_info.xml";
        private const string FILE_NAME_STORAGE_INFO = "apk_storage_info.xml";

        // TODO: Document
        public static bool IsAppInStorage(string appPackageName, string pathStorage)
        {
            StorageInfo storageInfo = GetStorageInfo(pathStorage);
            return storageInfo.ContainsApp(appPackageName);
        }

        // TODO: Document
        public static void CreateAppDirInStorage(string appPackageName, string pathStorage)
        {
            string pathDir = Path.Combine(pathStorage, appPackageName);

            // ERROR if there is a file with the same name
            if (File.Exists(pathDir)) return;

            if (!Directory.Exists(pathDir))
            {
                Directory.CreateDirectory(pathDir);
            }
        }

        // TODO: Document
        public static void CreateEmptyStoredAppInfo(string appPackageName, string appName, string pathStorage)
        {
            StoredAppInfo appInfo = new StoredAppInfo(appPackageName, appName);
            WriteStoredAppInfoToStorage(appInfo, pathStorage);
        }

        // TODO: Document
        public static StoredAppInfo GetStoredAppInfo(string appPackageName, string pathStorage)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(StoredAppInfo));
            StoredAppInfo storedAppInfo = null;
            using (StreamReader xmlReader = new StreamReader(Path.Combine(GetAppDirPathInStorage(appPackageName, pathStorage), FILE_NAME_APP_INFO)))
            {
                storedAppInfo = (StoredAppInfo)xmlSerializer.Deserialize(xmlReader);
            }
            return storedAppInfo;
        }

        // TODO: Document
        private static StorageInfo GetStorageInfo(string pathStorage)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(StorageInfo));
            StorageInfo storageInfo = null;
            using (StreamReader xmlReader = new StreamReader(Path.Combine(pathStorage, FILE_NAME_STORAGE_INFO)))
            {
                storageInfo = (StorageInfo)xmlSerializer.Deserialize(xmlReader);
            }
            return storageInfo;
        }

        private static List<string> GetListOfAppsInStorage(string pathStorage)
        {
            StorageInfo storageInfo = GetStorageInfo(pathStorage);
            List<string> listOfApps = new List<string>(storageInfo.AppsInStorage.Keys);

            return listOfApps;
        }

        // TODO: Document
        public static void LoadPackageIntoStorage(ApplicationPackage package, string pathStorage)
        {
            File.Copy(package.PackagePath,
                      Path.Combine(GetAppDirPathInStorage(package.PackageName, pathStorage),
                                   NamingHelper.GetPackageNameInStorage(package)),
                      true);
        }

        // TODO: Document
        public static void UpdateStoredAppInfo(StoredAppInfo appInfo, string pathStorage)
        {
            WriteStoredAppInfoToStorage(appInfo, pathStorage);
        }

        // TODO: Document
        public static void AddAppPackageToStorageInfo(string appPackageName, string pathStorage)
        {
            StorageInfo storageInfo = GetStorageInfo(pathStorage);
            storageInfo.AddAppPackage(appPackageName);
            WriteStorageInfo(storageInfo, pathStorage);
        }

        // TODO: Document
        private static void WriteStorageInfo(StorageInfo storageInfo, string pathStorage)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(StorageInfo));
            using (StreamWriter writer = new StreamWriter(Path.Combine(pathStorage, FILE_NAME_STORAGE_INFO), false))
            {
                xmlSerializer.Serialize(writer, storageInfo);
            }
        }

        // TODO: Document
        private static void WriteStoredAppInfoToStorage(StoredAppInfo storedAppInfo, string pathStorage)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(StoredAppInfo));
            using (StreamWriter writer = new StreamWriter(Path.Combine(GetAppDirPathInStorage(storedAppInfo.AppName, pathStorage),
                                                                       FILE_NAME_APP_INFO), false))
            {
                xmlSerializer.Serialize(writer, storedAppInfo);
            }
        }

        // TODO: Document
        public static string GetAppDirPathInStorage(string appPackageName, string pathStorage)
        {
            return Path.Combine(pathStorage, appPackageName);
        }

    }
}
