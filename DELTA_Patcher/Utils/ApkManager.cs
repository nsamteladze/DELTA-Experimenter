using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DELTA_Patcher.Model;
using System.IO;

namespace DELTA_Patcher.Utils
{
    public static class ApkManager
    {
        #region String Constants

        // Path to the manifest file (MANIFEST.MF) from the APK's main directory
        private const string PATH_MANIFEST = @"META-INF\MANIFEST.MF";

        #endregion String Constants

        /// <summary>
        /// Load manifest file (MANIFEST.MF) for the specified APK.
        /// </summary>
        /// <param name="pathApkMainDirectory">
        /// Directory with all the APK's files.
        /// </param>
        /// <returns>
        /// APK manifest file with records about file in APK.
        /// Returns NULL if manifest file could not be found.
        /// </returns>
        public static ApkManifest GetApkManifest(string pathApkMainDirectory)
        {
            ApkManifest apkManifest = new ApkManifest();

            string pathApkManifest = Path.Combine(pathApkMainDirectory, PATH_MANIFEST);

            // Error if MANIFEST.MF can't be found in the APK's main directory
            if (!File.Exists(pathApkManifest)) return null;

            // We consider the specifics of MANIFEST.MF when we read it
            // MANIFEST.MF consists of records about files in APK
            // Record looks like:
            //     Name: file_name (NOTE: may be on 2 lines)
            //     SHA1-Digest: file_sha1
            using (StreamReader reader = new StreamReader(pathApkManifest))
            {
                while (!reader.EndOfStream)
                {
                    string input = reader.ReadLine();
                    // Skip empty lines and control information as we are interested only
                    // in records about files
                    if (input.Equals(String.Empty) || !input.StartsWith("Name")) continue;

                    string[] inputParts = input.Split(' ');
                    string fileName;
                    string fileSHA;

                    // Read another line to check that the file's path is presented as one line
                    // If it is one line that it must be followed by SHA1
                    string secondInput = reader.ReadLine().Trim();
                    if (secondInput.StartsWith("SHA"))
                    {
                        fileName = inputParts[1];
                        for (int i = 2; i < inputParts.Length; ++i)
                        {
                            fileName += (" " + inputParts[i]);
                        }
                        fileSHA = secondInput.Split(' ')[1];
                        
                    }
                    // Case when 2 lines are used for the file name
                    // Add combination of lines as the file name
                    else
                    {
                        fileName = inputParts[1];
                        for (int i = 2; i < inputParts.Length; ++i)
                        {
                            fileName += (" " + inputParts[i]);
                        }
                        fileName += secondInput;
                        // SHA is on the next line
                        fileSHA = reader.ReadLine().Split(' ')[1];
                    }

                    // Make all the slashes Windows style
                    FileManager.FixSlashesInPath(ref fileName);

                    apkManifest.AddFileRecordToManifest(new ApkManifestRecord(fileName, fileSHA));
                }
            }

            return apkManifest;
        }
    }
}
