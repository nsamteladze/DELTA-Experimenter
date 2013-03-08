using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DELTA_Patcher.Data_Model;
using System.Xml.Serialization;
using System.IO;

namespace DELTA_Patcher.Utils
{
    public static class StorageManager
    {
        public static string PATH_APK_STORAGE;
        private const string FILE_NAME_APP_INFO = "app_info.xml";
        private const string FILE_NAME_STORAGE_INFO = "apk_storage_info.xml";
        private const string NAME_PATCH_DIR = "_patches";

        public static string GetPatchDirInStorage()
        {
            return Path.Combine(PATH_APK_STORAGE, NAME_PATCH_DIR);
        }

        // TODO: Document
        public static void LoadApkIntoStorage(string pathApk, ApkInfo apkInfo)
        {
            string appName = apkInfo.PackageName;

            if (!AppIsInStorage(appName)) 
            {
                CreateAppDirInStorage(appName);
                CreateEmptyStoredAppInfo(appName, apkInfo.AppLabel);
            }

            StoredAppInfo appInfo = GetStoredAppInfo(appName);

            if (!appInfo.ContainsVersion(apkInfo.CodeVersion))
            {
                string apkNameInStorage = ConstructApkName(apkInfo);

                CopyApkIntoStorage(pathApk, appName, apkNameInStorage);

                appInfo.AddVersion(apkInfo.CodeVersion, apkNameInStorage);

                UpdateAppInfo(appInfo);
                UpdateStorageInfo(appName);
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
            using (StreamReader xmlReader = new StreamReader(Path.Combine(ConstructAppDirPath(appName), FILE_NAME_APP_INFO)))
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

        // TODO: Document
        public static List<ApplicationVersionsPair> GetPairsOfAppVersions(string appName)
        {
            if (!AppIsInStorage(appName)) return null;

            List<ApplicationVersionsPair> appVersions = null;

            StoredAppInfo appInfo = GetStoredAppInfo(appName);

            if (appInfo.AppVersions.Count > 1)
            {
                appVersions = new List<ApplicationVersionsPair>();
                List<long> versions = appInfo.AppVersions.Keys.ToList<long>();
                versions.Sort();
                for (int i = 0; i < versions.Count - 1; ++i)
                {
                    string appDirInStorage = ConstructAppDirPath(appName);
                    string pathOldApk = Path.Combine(appDirInStorage, appInfo.AppVersions[versions[i]]);
                    string pathNewApk = Path.Combine(appDirInStorage, appInfo.AppVersions[versions[i + 1]]);
                    ApkInfo oldApkInfo = new ApkInfo(appInfo.AppName, appInfo.AppLabel, versions[i]);
                    ApkInfo newApkInfo = new ApkInfo(appInfo.AppName, appInfo.AppLabel, versions[i + 1]);
                    appVersions.Add(new ApplicationVersionsPair(pathOldApk, pathNewApk, oldApkInfo, newApkInfo));
                }
            }

            return appVersions;
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
            string pathAppDirInStorage = ConstructAppDirPath(appPackageName);
            for (int i = listAppCodeVersions.Count - numPackages; i < listAppCodeVersions.Count; ++i)
            {
                long nextAppCodeVersion = listAppCodeVersions[i];
                string pathAppPackage = Path.Combine(pathAppDirInStorage, appInfo.AppVersions[nextAppCodeVersion]);

                listAppPackages.Add(new ApplicationPackage(pathAppPackage,
                                                           new AndroidPackageInfo(appInfo.AppName, appInfo.AppLabel, nextAppCodeVersion)));
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
        private static string ConstructApkName(ApkInfo apkInfo)
        {
            return String.Format("{0}-{1}.apk", apkInfo.PackageName, apkInfo.CodeVersion);
        }

        // TODO: Document
        private static void CopyApkIntoStorage(string pathCopiedApk, string desinationApp, string apkNameInStorage)
        {
            File.Copy(pathCopiedApk, 
                      Path.Combine(ConstructAppDirPath(desinationApp), 
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
            using (StreamWriter writer = new StreamWriter(Path.Combine(ConstructAppDirPath(storedAppInfo.AppName), 
                                                                       FILE_NAME_APP_INFO), false))
            {
                xmlSerializer.Serialize(writer, storedAppInfo);
            }
        }

        // TODO: Document
        private static string ConstructAppDirPath(string appName)
        {
            return Path.Combine(PATH_APK_STORAGE, appName);
        }
    }
}
