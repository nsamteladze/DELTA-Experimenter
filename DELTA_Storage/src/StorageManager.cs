using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using DELTA_Common.DataModel;
using DELTA_Storage.DataModel;
using DELTA_Common.Utilities;
using System.Linq;

namespace DELTA_Storage
{
    public static class StorageManager
    {
        // Is set from the Experimenter when application in started
        public static string PATH_APK_STORAGE;
        private const string FILE_NAME_APP_INFO = "app_info.xml";
        private const string FILE_NAME_STORAGE_INFO = "apk_storage_info.xml";

        // TODO: Document
        public static void LoadPackageIntoStorage(ApplicationPackage appPackage)
        {
            if (!AppIsInStorage(appPackage.PackageName)) 
            {
                CreateAppDirInStorage(appPackage.PackageName);
                CreateEmptyStoredAppInfo(appPackage.PackageName, appPackage.ApplicationName);
            }

            StoredAppInfo appInfo = GetStoredAppInfo(appPackage.PackageName);

            if (!appInfo.ContainsVersion(appPackage.CodeVersion))
            {
                string apkNameInStorage = NamingHelper.GetPackageNameInStorage(appPackage);

                CopyApkIntoStorage(appPackage.PackagePath, appPackage.PackageName, apkNameInStorage);

                appInfo.AddVersion(appPackage.CodeVersion, apkNameInStorage);

                UpdateAppInfo(appInfo);
                UpdateStorageInfo(appPackage.PackageName);
            }
        }

        // TODO: Document
        private static bool AppIsInStorage(string appName)
        {
            StorageInfo storageInfo = GetStorageInfo();
            return storageInfo.ContainsApp(appName);
        }

        // TODO: Document
        private static void CreateAppDirInStorage(string appName)
        {
            string pathDir = Path.Combine(PATH_APK_STORAGE, appName);

            // ERROR if there is a file with the same name
            if (File.Exists(pathDir)) return;

            if (!Directory.Exists(pathDir))
            {
                Directory.CreateDirectory(pathDir);
            }
        }

        // TODO: Document
        private static void CreateEmptyStoredAppInfo(string appName, string appLabel)
        {
            StoredAppInfo appInfo = new StoredAppInfo(appName, appLabel);
            WriteStoredAppInfo(appInfo);
        }

        // TODO: Document
        private static StoredAppInfo GetStoredAppInfo(string appName)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(StoredAppInfo));
            StoredAppInfo storedAppInfo = null;
            using (StreamReader xmlReader = new StreamReader(Path.Combine(GetAppDirPath(appName), FILE_NAME_APP_INFO)))
            {
                storedAppInfo = (StoredAppInfo)xmlSerializer.Deserialize(xmlReader);
            }
            return storedAppInfo;
        }

        // TODO: Document
        private static StorageInfo GetStorageInfo()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(StorageInfo));
            StorageInfo storageInfo = null;
            using (StreamReader xmlReader = new StreamReader(Path.Combine(PATH_APK_STORAGE, FILE_NAME_STORAGE_INFO)))
            {
                storageInfo = (StorageInfo)xmlSerializer.Deserialize(xmlReader);
            }
            return storageInfo;
        }

        /// <summary>
        /// Returns N latest application packages where N = numLatestPackages
        /// </summary>
        /// <param name="appPackageName"></param>
        /// <param name="numPackages">
        /// Number of the latest packages to return. If (numPackages = 0) or it is greater than
        /// the number of packages available, then all the available packages are returned.
        /// </param>
        /// <returns>
        /// Returns NULL if numPackages is negative. 
        /// Returns NULL if application could not been found in the storage.
        /// Returns a list of packages otherwise (the latest package last).
        /// </returns>
        public static List<ApplicationPackage> GetLatestAppPackages(string appPackageName, int numPackages)
        {
            if (!AppIsInStorage(appPackageName) || (numPackages < 0)) return null;

            List<ApplicationPackage> listAppPackages = new List<ApplicationPackage>();

            // Get a sorted list of all stored application packages (versions)
            StoredAppInfo appInfo = GetStoredAppInfo(appPackageName);
            List<long> listAppCodeVersions = appInfo.AppVersions.Keys.ToList<long>();
            listAppCodeVersions.Sort();

            // If numPackages is 0 or greater than then the number of available packages, 
            // then all available packages will be returned.
            if ((numPackages == 0) || (numPackages > listAppCodeVersions.Count))
            {
                numPackages = listAppCodeVersions.Count; 
            }

            // Populate the list based on the required number of packages
            string pathAppDirInStorage = GetAppDirPath(appPackageName);
            for (int i = listAppCodeVersions.Count - numPackages; i < listAppCodeVersions.Count; ++i)
            {
                long nextAppCodeVersion = listAppCodeVersions[i];
                string pathAppPackage = Path.Combine(pathAppDirInStorage, appInfo.AppVersions[nextAppCodeVersion]);

                // Application package type must be determined based on the storage type (or application type) - iOS/Android
                listAppPackages.Add(new AndroidApplicationPackage(pathAppPackage, appInfo.AppName, appInfo.AppLabel, nextAppCodeVersion));
            }

            return listAppPackages; 
        }

        public static List<string> GetListOfAppsInStorage()
        {
            StorageInfo storageInfo = GetStorageInfo();
            List<string> listOfApps = new List<string>(storageInfo.AppsInStorage.Keys);

            return listOfApps;
        }

        // TODO: Document
        private static void CopyApkIntoStorage(string pathCopiedApk, string desinationApp, string apkNameInStorage)
        {
            File.Copy(pathCopiedApk, 
                      Path.Combine(GetAppDirPath(desinationApp), 
                                   apkNameInStorage), 
                      true);
        }

        // TODO: Document
        private static void UpdateAppInfo(StoredAppInfo appInfo)
        {
            WriteStoredAppInfo(appInfo);
        }

        // TODO: Document
        private static void UpdateStorageInfo(string appName)
        {
            StorageInfo storageInfo = GetStorageInfo();
            storageInfo.AddApplication(appName);
            WriteStorageInfo(storageInfo);
        }

        // TODO: Document
        private static void WriteStorageInfo(StorageInfo storageInfo)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(StorageInfo));
            using (StreamWriter writer = new StreamWriter(Path.Combine(PATH_APK_STORAGE, FILE_NAME_STORAGE_INFO), false))
            {
                xmlSerializer.Serialize(writer, storageInfo);
            }
        }

        // TODO: Document
        private static void WriteStoredAppInfo(StoredAppInfo storedAppInfo)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(StoredAppInfo));
            using (StreamWriter writer = new StreamWriter(Path.Combine(GetAppDirPath(storedAppInfo.AppName), 
                                                                       FILE_NAME_APP_INFO), false))
            {
                xmlSerializer.Serialize(writer, storedAppInfo);
            }
        }

        // TODO: Document
        private static string GetAppDirPath(string appName)
        {
            return Path.Combine(PATH_APK_STORAGE, appName);
        }
    }
}
