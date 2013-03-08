using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DELTA_Patcher.Model
{
    /// <summary>
    /// Represents APK manifest (MANIFEST.MF).
    /// </summary>
    public class ApkManifest
    {
        #region Properties

        public HashSet<ApkManifestRecord> FilesInApk
        {
            get;
            private set;
        }

        #endregion Properties

        #region Constructors

        public ApkManifest()
        {
            FilesInApk = new HashSet<ApkManifestRecord>();
        }

        public ApkManifest(ApkManifest apkManifest)
        {
            FilesInApk = new HashSet<ApkManifestRecord>(apkManifest.FilesInApk);
        }

        public ApkManifest(IEnumerable<ApkManifestRecord> filesInApk)
        {
            FilesInApk = new HashSet<ApkManifestRecord>(filesInApk);
        }

        #endregion Constructors

        #region Add Data Methods

        public void AddFileRecordToManifest(ApkManifestRecord fileRecord)
        {
            FilesInApk.Add(fileRecord);
        }
        
        #endregion Add Data Methods
    }
}
