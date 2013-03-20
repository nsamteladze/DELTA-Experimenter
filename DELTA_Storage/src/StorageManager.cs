using System.Collections.Generic;
using System.IO;
using System.Linq;
using DELTA_Common.DataModel;
using DELTA_Common.Utilities;
using DELTA_Storage.DataModel;

namespace DELTA_Storage
{
    public class StorageManager
    {
        #region Private Fields

        private string _pathStorage;

        #endregion

        #region Properties

        public string PathStorage 
        { 
            get
            {
                return _pathStorage;
            }
            // Storage must already exist. If not, then the storage path will be null.
            set
            {
                if (File.Exists(value))
                {
                    _pathStorage = value;
                }
                else
                {
                    _pathStorage = null;
                }
            }
        }

        #endregion

        #region Constructors

        // It is mandatory to specify a valid storage path
        public StorageManager(string pathStorage)
        {
            this.PathStorage = pathStorage;
        }

        #endregion

        /// <summary>
        /// Add the specified package to the storage and update the meta information
        /// </summary>
        /// <param name="appPackage"></param>
        public void LoadPackageIntoStorage(ApplicationPackage appPackage)
        {
            // If application is new and is not presented in storage yet
            if (!StorageOperations.IsAppInStorage(appPackage.PackageName, PathStorage)) 
            {
                StorageOperations.CreateAppDirInStorage(appPackage.PackageName, PathStorage);
                StorageOperations.CreateEmptyStoredAppInfo(appPackage.PackageName, appPackage.ApplicationName, PathStorage);
            }

            StoredAppInfo appInfo = StorageOperations.GetStoredAppInfo(appPackage.PackageName, PathStorage);

            // Add new version if it is not already in storage
            if (!appInfo.ContainsVersion(appPackage.CodeVersion))
            {
                string apkNameInStorage = NamingHelper.GetPackageNameInStorage(appPackage);

                // Physically copy package into storage
                StorageOperations.LoadPackageIntoStorage(appPackage, PathStorage);

                appInfo.AddVersion(appPackage.CodeVersion, apkNameInStorage);

                // Update meta info for the application and for the storage itself
                StorageOperations.UpdateStoredAppInfo(appInfo, PathStorage);
                StorageOperations.AddAppPackageToStorageInfo(appPackage.PackageName, PathStorage);
            }
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
        public List<ApplicationPackage> GetLatestAppPackages(string appPackageName, int numPackages)
        {
            if (!StorageOperations.IsAppInStorage(appPackageName, PathStorage) || (numPackages < 0)) return null;

            List<ApplicationPackage> listAppPackages = new List<ApplicationPackage>();

            // Get a sorted list of all stored application packages (versions)
            StoredAppInfo appInfo = StorageOperations.GetStoredAppInfo(appPackageName, PathStorage);
            List<long> listAppCodeVersions = appInfo.AppVersions.Keys.ToList<long>();
            listAppCodeVersions.Sort();

            // If numPackages is 0 or greater than then the number of available packages, 
            // then all available packages will be returned.
            if ((numPackages == 0) || (numPackages > listAppCodeVersions.Count))
            {
                numPackages = listAppCodeVersions.Count; 
            }

            // Populate the list based on the required number of packages
            string pathAppDirInStorage = StorageOperations.GetAppDirPathInStorage(appPackageName, PathStorage);
            for (int i = listAppCodeVersions.Count - numPackages; i < listAppCodeVersions.Count; ++i)
            {
                long nextAppCodeVersion = listAppCodeVersions[i];
                string pathAppPackage = Path.Combine(pathAppDirInStorage, appInfo.AppVersions[nextAppCodeVersion]);

                // Application package type must be determined based on the storage type (or application type) - iOS/Android
                listAppPackages.Add(new AndroidApplicationPackage(pathAppPackage, appInfo.AppName, appInfo.AppName, nextAppCodeVersion));
            }

            return listAppPackages; 
        }

        
    }
}
