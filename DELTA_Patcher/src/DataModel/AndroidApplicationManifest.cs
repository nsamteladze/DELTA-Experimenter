using System.Collections.Generic;

namespace DELTA_Patcher.DataModel
{
    /// <summary>
    /// Represents Android application manifest (MANIFEST.MF).
    /// </summary>
    public class AndroidApplicationManifest
    {
        #region Properties

        public HashSet<AndroidApplicationManifestRecord> FilesInPackage
        {
            get;
            private set;
        }

        #endregion Properties

        #region Constructors

        public AndroidApplicationManifest()
        {
            FilesInPackage = new HashSet<AndroidApplicationManifestRecord>();
        }

        public AndroidApplicationManifest(AndroidApplicationManifest appManifest)
        {
            FilesInPackage = new HashSet<AndroidApplicationManifestRecord>(appManifest.FilesInPackage);
        }

        public AndroidApplicationManifest(IEnumerable<AndroidApplicationManifestRecord> filesInPackage)
        {
            FilesInPackage = new HashSet<AndroidApplicationManifestRecord>(filesInPackage);
        }

        #endregion Constructors

        #region Add Data Methods

        public void AddFileRecordToManifest(AndroidApplicationManifestRecord fileRecord)
        {
            this.FilesInPackage.Add(fileRecord);
        }
        
        #endregion Add Data Methods
    }
}
