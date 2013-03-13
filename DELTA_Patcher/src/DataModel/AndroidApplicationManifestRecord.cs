
namespace DELTA_Patcher.DataModel
{
    /// <summary>
    /// Represents manifest's record (name + SHA) about a file in Android application package.
    /// </summary>
    public class AndroidApplicationManifestRecord
    {
        #region Properties

        public string FileName { get; set; }
        public string FileSHA { get; set; }

        #endregion

        #region Constructors

        public AndroidApplicationManifestRecord(string fileName, string SHA)
        {
            FileName = fileName;
            FileSHA = SHA;
        }

        #endregion

        #region Override of Equals, GetHashCode

        /* NOTE:
         * AndroidApplicationManifestRecord objects are equal if their FileName fields are equal.
         * Difference in SHA is not considered when we compare ApkFileRecord objects.
         */ 

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to AndroidApplicationManifestRecord return false.
            var record = obj as AndroidApplicationManifestRecord;
            if ((object)record == null)
            {
                return false;
            }

            // Return true if the fields match:
            return FileName.Equals(record.FileName);
        }

        public bool Equals(AndroidApplicationManifestRecord record)
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

        #endregion
    }
}
