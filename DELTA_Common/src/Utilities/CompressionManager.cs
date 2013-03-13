using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;
using System.Diagnostics;
using System.IO;

namespace DELTA_Common.Utilities
{
    public static class CompressionManager
    {
        /// <summary>
        /// Decompresses a zipped archive into a specified directory.
        /// </summary>
        /// <param name="zipFileName">
        /// ZIP archive that needs to be decompressed.
        /// </param>
        /// <param name="unpackDirName">
        /// Directory to decompress ZIP archive into.
        /// </param>
        public static void DecompressZIP(string zipFileName, string unpackDirName)
        {
            try
            {
                using (ZipFile zip = ZipFile.Read(zipFileName))
                {
                    // This call to ExtractAll() assumes:
                    //   - none of the entries are password-protected.
                    //   - want to extract all entries to current working directory
                    //   - none of the files in the zip already exist in the directory;
                    //     if they do, the method will throw.
                    zip.ExtractAll(unpackDirName);
                }
            }
            catch (System.Exception e)
            {
                System.Console.Error.WriteLine("Exception: " + e);
            }
        }

        /// <summary>
        /// Compresses the specified directory and creates a zipped archive from it.
        /// </summary>
        /// <param name="directoryToZip">
        /// Directory with files that will be compressed. 
        /// </param>
        /// <param name="zipArchiveName">
        /// Name of the resulting .zip archive that will contain all the files in the specified directory.
        /// </param>
        public static void CompressDirToZIP(string directoryToZip, string zipArchiveName)
        {
            try
            {
                using (ZipFile zipArchive = new ZipFile())
                {
                    // NOTE: Only top level files. Subdirectories are not traversed.
                    string[] fileNames = System.IO.Directory.GetFiles(directoryToZip);

                    foreach (String fileName in fileNames)
                    {
                        ZipEntry e = zipArchive.AddFile(fileName, "");
                    }

                    zipArchive.Save(zipArchiveName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e);
            }
        }
    }
}
