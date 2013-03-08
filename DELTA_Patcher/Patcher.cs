using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DELTA_Patcher.Utils;
using DELTA_Patcher.Model;
using System.Diagnostics;
using DELTA_Patcher.Data_Model;
using System.Xml.Serialization;

namespace DELTA_Patcher
{
    public static class Patcher
    {
        #region Constants

        private const string NAME_OLD_APK_DIR = "old";
        private const string NAME_NEW_APK_DIR = "new";
        private const string NAME_PATCH_DIR = "patch";
        private const string NAME_DELETE_SYS_FILE = "delete.sys";
        private const string NAME_PATCH_MANIFEST = "PatchManifest.xml";
        private const string NAME_TEMP_DIR = "temp";
        private const string NAME_STAT_FILE = "stats.csv";

        #endregion Constants

        /// <summary>
        /// Computes patch file for two versions of application presented as two APK files.
        /// Step 1 - Validate both APK files
        /// Step 2 - Change extensions from .apk to .zip and save new files' paths
        /// Step 3 - Create patch directory
        /// Step 4 - Decompress both APKs into the patch directory
        /// Step 5 - Load manifests for both APKs
        /// Step 6 - Construct sets of marked files (files are marked as DELETE, NEW, SAME, UPDATED)
        /// Step 7 - Copy files that are marked NEW into the constructed patch main directory
        /// Step 8 - Delta encode files that are marked UPDATED nad copy .diff files into the patch
        /// Step 9 - Create PatchManifest.xml
        /// Step 10 - Compress patch
        /// Clean up
        /// </summary>
        /// <param name="pathOldApk">
        /// Application's old version APK.
        /// </param>
        /// <param name="pathNewApk">
        /// Application's new version APK.</param>
        /// <param name="pathPatchDir">
        /// Destination directory for the computed patch.
        /// </param>
        public static long PatchTwoAPKFiles(string pathOldApk, string pathNewApk, string pathPatchDir,
                                            ApkInfo oldApkInfo, ApkInfo newApkInfo)
        {
            LogManager.Log(LogMessageType.Message, "Starting patch construction . . .", "Patcher");

            string pathWorkDir = Path.Combine(pathPatchDir, NAME_TEMP_DIR);

            // Step 1 - Validate both APK files
            if (!ValidateApk(pathOldApk) || !ValidateApk(pathNewApk))
            {
                LogManager.Log(LogMessageType.Error, "Bad APK input files", "Patcher");
                return 0;
            }

            // Step 2 - Change extensions from .apk to .zip and save new files' paths
            pathOldApk = FileManager.ChangeExtension(pathOldApk, ".zip");
            pathNewApk = FileManager.ChangeExtension(pathNewApk, ".zip");

            // Step 3 - Create patch directory
            Console.Write(String.Format("Constructing directory {0} . . .", pathWorkDir));
            ConstructWorkDirectory(pathWorkDir);
            Console.WriteLine(" --- DONE");

            // Step 4 - Decompress both APKs into the patch directory
            string pathOldApkMainDir = Path.Combine(pathWorkDir, NAME_OLD_APK_DIR);
            string pathNewApkMainDir = Path.Combine(pathWorkDir, NAME_NEW_APK_DIR);
            Console.Write(String.Format("Decompressing {0} . . .", pathOldApk));
            CompressionManager.DecompressZIP(pathOldApk, pathOldApkMainDir);
            Console.WriteLine(" --- DONE");
            Console.Write(String.Format("Decompressing {0} . . .", pathNewApk));
            CompressionManager.DecompressZIP(pathNewApk, pathNewApkMainDir);
            Console.WriteLine(" --- DONE");

            // Step 5 - Load manifests for both APKs
            ApkManifest oldApkManifest = ApkManager.GetApkManifest(pathOldApkMainDir);
            ApkManifest newApkManifest = ApkManager.GetApkManifest(pathNewApkMainDir);

            // Step 6 - Construct sets of marked files (files are marked as DELETE, NEW, SAME, UPDATED)
            Console.Write("Analysing manifests . . .");
            // Mark DELETE files
            HashSet<ApkManifestRecord> filesMarkedDELETE = MarkFilesDELETE(oldApkManifest, newApkManifest);
            // Mark NEW files
            HashSet<ApkManifestRecord> filesMarkedNEW = MarkFilesNEW(oldApkManifest, newApkManifest);
            // Mark UPDATED files
            HashSet<ApkManifestRecord> filesMarkedUPDATED = MarkFilesUPDATED(oldApkManifest, newApkManifest);
            Console.WriteLine(" --- DONE");
            
            // Step 7 - Copy files that are marked NEW into the constructed patch main directory
            Console.Write(String.Format("Copying new files to {0} . . .", Path.Combine(pathWorkDir, NAME_PATCH_DIR)));
            foreach (ApkManifestRecord record in filesMarkedNEW)
            {
                CopyFileIntoPatch(record.FileName, Path.Combine(pathWorkDir, NAME_PATCH_DIR), pathNewApkMainDir);
            }
            Console.WriteLine(" --- DONE");

            // Step 8 - Delta encode files that are marked UPDATED nad copy .diff files into the patch
            Console.Write("Computing delta differences . . .");
            foreach (ApkManifestRecord record in filesMarkedUPDATED)
            {
                string pathDiffFile = Path.Combine(pathWorkDir, NAME_PATCH_DIR, record.FileName.Replace('\\', '+')) + ".diff";
                ComputeDeltaDifference(Path.Combine(pathOldApkMainDir, record.FileName),
                                       Path.Combine(pathNewApkMainDir, record.FileName),
                                       pathDiffFile);

                // We don't want to include .diff file into the patch if its size is greater than the new file size.
                // Instead, we include the new version of file itself.
                if (FileManager.GetFileSize(pathDiffFile) > FileManager.GetFileSize(Path.Combine(pathNewApkMainDir, record.FileName)))
                {
                    File.Delete(pathDiffFile);
                    CopyFileIntoPatch(record.FileName, Path.Combine(pathWorkDir, NAME_PATCH_DIR), pathNewApkMainDir);

                    // Add files to filesMarkedNEW and filesMarkedDELETE
                    // During deployment we will delete old file and copy the new one instead of patching the old file
                    filesMarkedDELETE.Add(record);
                    filesMarkedNEW.Add(record);
                }
            }
            // Remove all files that are in filesMarkedNEW from filesMarkedUPDATED
            filesMarkedUPDATED.ExceptWith(filesMarkedNEW);
            Console.WriteLine(" --- DONE");

            // Step 9 - Create PatchManifest.xml
            Console.Write("Creating PatchManifest.xml . . .");
            string patchName = String.Format("{0}-{1}.deltapatch", Path.GetFileNameWithoutExtension(pathOldApk), 
                                                                   Path.GetFileNameWithoutExtension(pathNewApk));
            PatchManifest manifest = new PatchManifest(patchName, oldApkInfo, newApkInfo);
            // We need only files names in the manifest
            foreach (ApkManifestRecord record in filesMarkedNEW)
            {
                manifest.FilesNEW.Add(record.FileName);
            }
            foreach (ApkManifestRecord record in filesMarkedDELETE)
            {
                manifest.FilesDELETE.Add(record.FileName);
            }
            foreach (ApkManifestRecord record in filesMarkedUPDATED)
            {
                manifest.FilesUPDATED.Add(record.FileName);
            }
            WritePatchManifest(manifest, Path.Combine(pathWorkDir, NAME_PATCH_DIR));
            Console.WriteLine(" --- DONE");

            // Step 10 - Compress patch
            Console.Write("Compressing patch . . .");
            CompressionManager.CompressDirToZIP(Path.Combine(pathWorkDir, NAME_PATCH_DIR),
                                                Path.Combine(pathPatchDir, patchName));
            Console.WriteLine(" --- DONE");

            // Clean up
            // Change file extensions back
            Console.Write("Cleaning up . . .");
            pathOldApk = FileManager.ChangeExtension(pathOldApk, ".apk");
            pathNewApk = FileManager.ChangeExtension(pathNewApk, ".apk");
            // Delete temp directory
            FileManager.DeleteDirectory(pathWorkDir);
            Console.WriteLine(" --- DONE");
                
            Console.WriteLine(String.Format("Finished patch creation: {0} -> {1}", pathOldApk, pathNewApk));
            return FileManager.GetFileSize(Path.Combine(pathPatchDir, patchName));
        }

        public static void PatchApplicationInStorage(string applicationName)
        {
            List<ApplicationVersionsPair> appVersions = StorageManager.GetPairsOfAppVersions(applicationName);

            if (appVersions == null) return;

            using (StreamWriter writer = new StreamWriter(Path.Combine(StorageManager.GetPatchDirInStorage(), NAME_STAT_FILE), true))
            {
                string appLabel = appVersions[0].NewApkInfo.AppLabel;
                writer.Write(String.Format("{0},{1}", applicationName, appLabel));

                foreach (ApplicationVersionsPair versionsPair in appVersions)
                {
                    long apkSize = PatchTwoAPKFiles(versionsPair.PathOldApk, versionsPair.PathNewApk,
                                                    StorageManager.GetPatchDirInStorage(),
                                                    versionsPair.OldApkInfo, versionsPair.NewApkInfo);
                    writer.Write(String.Format(",{0}", (double)apkSize / FileManager.GetFileSize(versionsPair.PathNewApk)));
                    
                }

                writer.WriteLine();
            }
        }

        // This is a general method that can compute patches for a specified application using the specified algorithm.
        // Number of generated patches depends on the chosen options
        public static void ComputePatchesForApplicationInStorage(string appPackageName, IPatchingAlgorithm patchingAlgorithm, PatchComputationOptions options)
        {
            // Get the packages required to compute all the patches
            List<ApplicationPackage> listAllAppPackages = StorageManager.GetLatestAppPackages(appPackageName, options.NumberOfLatestApplicationVersionsUsed);

            if (listAllAppPackages == null) return;

            // Compute patches based on the chosen options
            if (options.PatchComputedBetween == EnumPatchComputedBetweenOptions.ConsecutiveVersionsOnly)
            {
                for (int i = 0; i < listAllAppPackages.Count; ++i)
                {
                    patchingAlgorithm.ComputePatch(listAllAppPackages[i], listAllAppPackages[i + 1], options.OutputPatchDir);
                }
            }
            else if (options.PatchComputedBetween == EnumPatchComputedBetweenOptions.EveryVersion)
            {
                for (int i = 0; i < listAllAppPackages.Count; ++i)
                {
                    for (int j = i + 1; j < listAllAppPackages.Count; ++j)
                    {
                        patchingAlgorithm.ComputePatch(listAllAppPackages[i], listAllAppPackages[j], options.OutputPatchDir);
                    }
                }
            }

        }

        public static void PatchAllApplicationsInStorage()
        {
            int i = 0;
            List<string> appsInStorage = StorageManager.GetListOfAppsInStorage();
            int numAppsInStorage = appsInStorage.Count;
            foreach (string appName in appsInStorage)
            {
                ++i;
                PatchApplicationInStorage(appName);
                Console.WriteLine(String.Format("---------------\n     {0} of {1}\n---------------",
                                  i, numAppsInStorage));
            }
        }

        public static void FinishPatchingApplicationsInStorage()
        {
            List<List<string>> statFile = FileManager.ReadFromCSV(Path.Combine(StorageManager.GetPatchDirInStorage(), NAME_STAT_FILE));
            if (statFile == null) return;

            HashSet<string> patchedApps = new HashSet<string>();
            foreach (List<string> appRecord in statFile)
            {
                patchedApps.Add(appRecord[0]);
            }

            int i = 0;
            List<string> appsInStorage = StorageManager.GetListOfAppsInStorage();
            int numAppsInStorage = appsInStorage.Count;
            foreach (string appName in appsInStorage)
            {
                ++i;
                Console.WriteLine(String.Format("---------------\n     {0} of {1}\n---------------",
                                  i, numAppsInStorage));

                if (patchedApps.Contains(appName)) continue;
                PatchApplicationInStorage(appName);
            }
        }

        public static void Stat()
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(StorageManager.GetPatchDirInStorage(), "temp.csv"), true))
            {
                foreach (string file in Directory.EnumerateFiles(StorageManager.GetPatchDirInStorage(), "*.deltapatch", SearchOption.TopDirectoryOnly))
                {
                    Console.Write(Path.GetFileNameWithoutExtension(file));
                    long patchSize = FileManager.GetFileSize(file);
                    string tempDir = Path.Combine(StorageManager.GetPatchDirInStorage(), "temp");
                    Directory.CreateDirectory(tempDir);
                    CompressionManager.DecompressZIP(file, tempDir);
                    int numFiles = Directory.EnumerateFiles(tempDir, "*", SearchOption.TopDirectoryOnly).Count();
                    writer.WriteLine(String.Format("{0},{1},{2}", Path.GetFileNameWithoutExtension(file), patchSize, numFiles));
                    FileManager.DeleteDirectory(tempDir);
                    Console.WriteLine("------ DONE");
                }
            }
        }

        /// <summary>
        /// Construct patch directory by deleting all the files from the specified directory if 
        /// it already exists, or by creating a new directory if it does not exist.
        /// </summary>
        /// <param name="pathWorkDir">
        /// Patch directory that needs to be constructed.
        /// </param>
        private static void ConstructWorkDirectory(string pathWorkDir)
        {
            if (Directory.Exists(pathWorkDir))
            {
                FileManager.CleanDirectory(pathWorkDir);
            }
            else
            {
                Directory.CreateDirectory(pathWorkDir);
            }

            Directory.CreateDirectory(Path.Combine(pathWorkDir, NAME_PATCH_DIR));
        }

        /// <summary>
        /// Checks that the specified file exists and has .apk extension.
        /// </summary>
        /// <param name="pathApk">
        /// Path to the checked file.
        /// </param>
        /// <returns>
        /// TRUE - file is OK
        /// FALSE - file is NOT OK
        /// </returns>
        private static bool ValidateApk(string pathApk)
        {
            // Apk is considered bad only if the file does not exist or if it does not have .apk extension.
            if (!File.Exists(pathApk) || !Path.GetExtension(pathApk).Equals(".apk")) return false;
            return true;                
        }

        /// <summary>
        /// Marks appropriate files in the old APK as DELETE.
        /// Files marked DELETE - presented in the old APK but not in the new APK
        /// </summary>
        /// <param name="oldApkManifest">
        /// Manifest file of the old APK.
        /// </param>
        /// <param name="newApkManifest">
        /// Manifest file of the new APK.
        /// </param>
        /// <returns>
        /// Set of files that are marked DELETE
        /// </returns>
        private static HashSet<ApkManifestRecord> MarkFilesDELETE(ApkManifest oldApkManifest, ApkManifest newApkManifest)
        {
            HashSet<ApkManifestRecord> filesMarkedDelete = new HashSet<ApkManifestRecord>(oldApkManifest.FilesInApk);
            filesMarkedDelete.ExceptWith(newApkManifest.FilesInApk);
            return filesMarkedDelete;
        }

        /// <summary>
        /// Marks appropriate files in the new APK as NEW.
        /// Files marked NEW - presented in the new APK but not in the old APK
        /// </summary>
        /// <param name="oldApkManifest">
        /// Manifest file of the old APK.
        /// </param>
        /// <param name="newApkManifest">
        /// Manifest file of the new APK.
        /// </param>
        /// <returns>
        /// Set of files that are marked NEW.
        /// </returns>
        private static HashSet<ApkManifestRecord> MarkFilesNEW(ApkManifest oldApkManifest, ApkManifest newApkManifest)
        {
            HashSet<ApkManifestRecord> filesMarkedNew = new HashSet<ApkManifestRecord>(newApkManifest.FilesInApk);
            filesMarkedNew.ExceptWith(oldApkManifest.FilesInApk);
            return filesMarkedNew;
        }
        
        /// <summary>
        /// Marks appropriate files in the old APK as UPDATED.
        /// Files marked UPDATED - presented in both the old and the new APKs but with different SHAs.
        /// NOTE: Files marked UPDATED will have SHA from the old APK
        /// </summary>
        /// <param name="oldApkManifest">
        /// Manifest file of the old APK.
        /// </param>
        /// <param name="newApkManifest">
        /// Manifest file of the new APK.
        /// </param>
        /// <returns>
        /// Set of files that are marked UPDATED.
        /// </returns>
        private static HashSet<ApkManifestRecord> MarkFilesUPDATED(ApkManifest oldApkManifest, ApkManifest newApkManifest)
        {
            // Get the files presented both in the old and the new APKs
            HashSet<ApkManifestRecord> commonFilesInManifest = new HashSet<ApkManifestRecord>(oldApkManifest.FilesInApk);
            commonFilesInManifest.IntersectWith(newApkManifest.FilesInApk);

            HashSet<ApkManifestRecord> filesMarkedUpdated = new HashSet<ApkManifestRecord>();


            // Go through the files and remove those with that are the same in both manifests
            foreach (ApkManifestRecord record in commonFilesInManifest)
            {
                // Get the file's SHA in the new APK
                string newFileSHA = newApkManifest.FilesInApk.First(x => x.FileName == record.FileName).FileSHA;
                // Files are marked as SAME if SHAs are the same.
                // Files are marked as UPDATED if SHAs are different.
                if (!record.FileSHA.Equals(newFileSHA))
                {
                    filesMarkedUpdated.Add(record);
                }
            }

            return filesMarkedUpdated;
        }

        /// <summary>
        /// Copies the specified file into the patch and renames it.
        /// There are no directories inside the patch. Files names are replaced with
        /// there full paths in APK (res\xml\1.xml -> res-xml-1.xml).
        /// '\' is replaced by '-' as Windows does not allow symbol '\' in files names.
        /// </summary>
        /// <param name="copiedFilePathInApk">
        /// File's path in APK main direcory (if file's full path is C:\APK_NAME\res\xml\1.xml,
        /// then copiedFilePathInApk will be res\xml\1.xml).
        /// </param>
        /// <param name="pathDestinationPatchDir">
        /// Destination directory where the file will copied to.
        /// </param>
        /// <param name="pathApkMainDir">
        /// Path to the APK's main directory (if you work with SOME_NAME application,
        /// then pathApkMainDir can be C:\Program Files\SOME_NAME).
        /// </param>
        private static void CopyFileIntoPatch(string copiedFilePathInApk, string pathDestinationPatchDir, string pathApkMainDir)
        {
            File.Copy(Path.Combine(pathApkMainDir, copiedFilePathInApk),
                      Path.Combine(pathDestinationPatchDir, copiedFilePathInApk.Replace('\\', '+')),
                      true);
        }

        /// <summary>
        /// Writes data to patch\delete.sys file that contains names of files
        /// that need to be deleted from the old APK.
        /// </summary>
        /// <param name="pathPatchDir">
        /// Main directory of the constructed patch.
        /// </param>
        /// <param name="data">
        /// Data that will be written in delete.sys.
        /// </param>
        /// <returns>
        /// TRUE - if files were written in delete.sys.
        /// FALSE - if error occured (e.g. delete.sys exists and is a directory).
        /// </returns>
        private static bool WriteToDeleteSys(string pathPatchDir, IEnumerable<ApkManifestRecord> data)
        {
            string pathDeleteSys = Path.Combine(pathPatchDir, NAME_PATCH_DIR, NAME_DELETE_SYS_FILE);

            // If there is a directory with the name "delete.sys"
            if (Directory.Exists(pathDeleteSys)) return false;

            using (StreamWriter writer = new StreamWriter(pathDeleteSys, false))
            {
                foreach (ApkManifestRecord record in data)
                {
                    writer.WriteLine(record.FileName);
                }
            }

            return true;
               
        }

        /// <summary>
        /// Computes delta difference between two files using Bsdiff algorithm.
        /// NOTE: Requires bsdiff.exe to be in the same directory as the app's executable file.
        /// </summary>
        /// <param name="pathOldFile">
        /// Path to the old file version.
        /// </param>
        /// <param name="pathNewFile">
        /// Path to the new file version.
        /// </param>
        /// <param name="pathPatch">
        /// Destination path for the computed patch.
        /// </param>
        public static void ComputeDeltaDifference(string pathOldFile, string pathNewFile, string pathPatch)
        {
            // Invokes bsdiff.exe to compute patch
            var psi = new ProcessStartInfo
            {
                FileName = "bsdiff.exe",
                // Parameters format: old_file new_file patch_file
                Arguments = String.Format("\"{0}\" \"{1}\" \"{2}\"", pathOldFile, pathNewFile, pathPatch),
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
            var process = Process.Start(psi);
            if (process.WaitForExit((int)TimeSpan.FromSeconds(10).TotalMilliseconds))
            {
                var result = process.StandardOutput.ReadToEnd();
            }
        }

        private static void WritePatchManifest(PatchManifest manifest, string pathPatch)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PatchManifest));
            using (StreamWriter xmlWriter = new StreamWriter(Path.Combine(pathPatch, NAME_PATCH_MANIFEST), false))
            {
                serializer.Serialize(xmlWriter, manifest);
            }
        }
    }
}

