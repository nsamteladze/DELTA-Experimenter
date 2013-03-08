using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DELTA_Patcher.Model
{
    /// <summary>
    /// Represents manifest's record (name + SHA) about a file in Apk.
    /// </summary>
    public class ApkManifestRecord
    {
        #region Properties

        public string FileName { get; set; }
        public string FileSHA { get; set; }

        #endregion Properties

        #region Constructors

        public ApkManifestRecord(string fileName, string SHA)
        {
            FileName = fileName;
            FileSHA = SHA;
        }

        #endregion Constructors

        #region Override of Equals, GetHashCode

        /* NOTE:
         * ApkManifestRecord objects are equal if their FileName fields are equal.
         * Difference in SHA is not considered when we compare ApkFileRecord objects.
         */ 

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to ApkManifestRecord return false.
            ApkManifestRecord record = obj as ApkManifestRecord;
            if ((object)record == null)
            {
                return false;
            }

            // Return true if the fields match:
            return FileName.Equals(record.FileName);
        }

        public bool Equals(ApkManifestRecord record)
        {
            // If parameter is null return false:
            if ((object)record == null)
            {
                return false;
            }

            // Return true if the fields match:
            return FileName.Equals(record.FileName);
        }

        public override int GetHashCode()
        {
            return FileName.GetHashCode();
        }

        #endregion Override of Equals, GetHashCode
    }
}
