using System;
using System.Collections.Generic;
using System.Linq;
using DELTA_Patcher.DataModel;
using DELTA_Patcher.Utilities;
using System.IO;
using DELTA_Common.DataModel;
using DELTA_Common.Utilities;

namespace DELTA_Patcher.Algorithms
{
    public class AndroidDELTAPlusPlusAlgorithm : IPatchingAlgorithm
    {
        #region String Constants

        private const string NAME_REFERENCE_DIR = "reference";
        private const string NAME_TARGET_DIR = "target";
        private const string NAME_PATCH_DIR = "patch";
        private const string NAME_PATCH_MANIFEST = "PatchManifest.xml";

        #endregion

        #region I/O Related Methods

        private static void ConstructPathingWorkDir(string pathPatchingWorkDir)
        {
            if (Directory.Exists(pathPatchingWorkDir))
            {
                FileManager.CleanDirectory(pathPatchingWorkDir);
            }
            else
            {
                Directory.CreateDirectory(pathPatchingWorkDir);
            }

            // Create proper directory structure
            Directory.CreateDirectory(Path.Combine(pathPatchingWorkDir, NAME_PATCH_DIR));
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
        /// <param name="pathPackageDir">
        /// Path to the APK's main directory (if you work with SOME_NAME application,
        /// then pathApkMainDir can be C:\Program Files\SOME_NAME).
        /// </param>
        private static void CopyFileIntoPatch(string relativeFilePathInPackage, string pathDestDir, string pathPackageDir)
        {
            File.Copy(Path.Combine(pathPackageDir, relativeFilePathInPackage),
                      Path.Combine(pathDestDir, relativeFilePathInPackage.Replace('\\', '+')),
                      true);
        }

        #endregion

        #region Methods That Mark Files in the Reference Package

        /// <summary>
        /// Marks appropriate files in the reference package as DELETE.
        /// Files marked DELETE - presented in the reference package but not in the target package.
        /// </summary>
        /// <param name="referenceManifest">
        /// Manifest file of the reference package.
        /// </param>
        /// <param name="targetManifest">
        /// Manifest file of the target package.
        /// </param>
        /// <returns>
        /// Set of files that are marked DELETE
        /// </returns>
        private static HashSet<AndroidApplicationManifestRecord> MarkFilesDELETE(AndroidApplicationManifest referenceManifest, AndroidApplicationManifest targetManifest)
        {
            HashSet<AndroidApplicationManifestRecord> filesMarkedDelete = new HashSet<AndroidApplicationManifestRecord>(referenceManifest.FilesInPackage);
            filesMarkedDelete.ExceptWith(targetManifest.FilesInPackage);
            return filesMarkedDelete;
        }

        /// <summary>
        /// Marks appropriate files in the target package as NEW.
        /// Files marked NEW - presented in the target package but not in the reference package
        /// </summary>
        /// <param name="referenceManifest">
        /// Manifest file of the reference package.
        /// </param>
        /// <param name="targetManifest">
        /// Manifest file of the target package.
        /// </param>
        /// <returns>
        /// Set of files that are marked NEW.
        /// </returns>
        private static HashSet<AndroidApplicationManifestRecord> MarkFilesNEW(AndroidApplicationManifest referenceManifest, AndroidApplicationManifest targetManifest)
        {
            HashSet<AndroidApplicationManifestRecord> filesMarkedNew = new HashSet<AndroidApplicationManifestRecord>(targetManifest.FilesInPackage);
            filesMarkedNew.ExceptWith(referenceManifest.FilesInPackage);
            return filesMarkedNew;
        }

        /// <summary>
        /// Marks appropriate files in the reference package as UPDATED.
        /// Files marked UPDATED - presented in both the old and the target packages but with different SHAs.
        /// NOTE: Files marked UPDATED will have SHA from the reference package
        /// </summary>
        /// <param name="oldAndroidApplicationManifest">
        /// Manifest file of the reference package.
        /// </param>
        /// <param name="targetManifest">
        /// Manifest file of the target package.
        /// </param>
        /// <returns>
        /// Set of files that are marked UPDATED.
        /// </returns>
        private static HashSet<AndroidApplicationManifestRecord> MarkFilesUPDATED(AndroidApplicationManifest referenceManifest, AndroidApplicationManifest targetManifest)
        {
            // Get the files presented both in the old and the target packages
            HashSet<AndroidApplicationManifestRecord> commonFilesInManifest = new HashSet<AndroidApplicationManifestRecord>(referenceManifest.FilesInPackage);
            commonFilesInManifest.IntersectWith(targetManifest.FilesInPackage);

            HashSet<AndroidApplicationManifestRecord> filesMarkedUpdated = new HashSet<AndroidApplicationManifestRecord>();


            // Go through the files and remove those with that are the same in both manifests
            foreach (AndroidApplicationManifestRecord record in commonFilesInManifest)
            {
                // Get the file's SHA in the target package
                string newFileSHA = targetManifest.FilesInPackage.First(x => x.FileName == record.FileName).FileSHA;
                // Files are marked as SAME if SHAs are the same.
                // Files are marked as UPDATED if SHAs are different.
                if (!record.FileSHA.Equals(newFileSHA))
                {
                    filesMarkedUpdated.Add(record);
                }
            }

            return filesMarkedUpdated;
        }

        #endregion

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
        /// <param name="reference"></param>
        /// <param name="target"></param>
        /// <param name="pathOutputPatch"></param>
        public void ComputePatch(ApplicationPackage reference, ApplicationPackage target, string pathOutputPatch)
        {
            // Step 1 - Validate both APK files
            if (!AndroidPackageManager.ValidatePackage(reference) || !AndroidPackageManager.ValidatePackage(target))
            {
                LogManager.Log(LogMessageType.Error, "Bad APK input files", "Patcher");
                return;
            }

            // Step 2 - Change extensions from .apk to .zip and save new files' paths
            //pathOldApk = FileManager.ChangeExtension(pathOldApk, ".zip");
            //pathNewApk = FileManager.ChangeExtension(pathNewApk, ".zip");

            // Step 3 - Create patch directory
            string pathPatchingWorkDir = Patcher.GetPatchingWorkDir();
            ConstructPathingWorkDir(pathPatchingWorkDir);

            // Step 4 - Decompress both APKs into the temporary directory
            string pathReferenceDir = Path.Combine(pathPatchingWorkDir, NAME_REFERENCE_DIR);
            string pathTargetDir = Path.Combine(pathPatchingWorkDir, NAME_TARGET_DIR);
            CompressionManager.DecompressZIP(reference.PackagePath, pathReferenceDir);
            CompressionManager.DecompressZIP(target.PackagePath, pathTargetDir);

            // Step 5 - Load manifests for both APKs
            AndroidApplicationManifest referenceManifest = AndroidPackageManager.GetApplicationManifest(pathReferenceDir);
            AndroidApplicationManifest targetManifest = AndroidPackageManager.GetApplicationManifest(pathTargetDir);

            // Step 6 - Construct sets of marked files (files are marked as DELETE, NEW, SAME, UPDATED)
            // Mark DELETE files
            HashSet<AndroidApplicationManifestRecord> filesMarkedDELETE = MarkFilesDELETE(referenceManifest, targetManifest);
            // Mark NEW files
            HashSet<AndroidApplicationManifestRecord> filesMarkedNEW = MarkFilesNEW(referenceManifest, targetManifest);
            // Mark UPDATED files
            HashSet<AndroidApplicationManifestRecord> filesMarkedUPDATED = MarkFilesUPDATED(referenceManifest, targetManifest);
            
            // Step 7 - Copy files that are marked NEW into the constructed patch main directory
            foreach (AndroidApplicationManifestRecord record in filesMarkedNEW)
            {
                CopyFileIntoPatch(record.FileName, Path.Combine(pathPatchingWorkDir, NAME_PATCH_DIR), pathTargetDir);
            }

            // Step 8 - Delta encode files that are marked UPDATED nad copy .diff files into the patch
            var deltaEncodingAlgorithm = new BSDIFFDeltaEncodingAlgortihm();
            foreach (AndroidApplicationManifestRecord record in filesMarkedUPDATED)
            {
                string pathDiffFile = Path.Combine(pathPatchingWorkDir, NAME_PATCH_DIR, record.FileName.Replace('\\', '+')) + ".diff";
                deltaEncodingAlgorithm.ComputeDelta(Path.Combine(pathReferenceDir, record.FileName),
                                                    Path.Combine(pathTargetDir, record.FileName),
                                                    pathDiffFile);

                // We don't want to include .diff file into the patch if its size is greater than the new file size.
                // Instead, we include the new version of file itself.
                if (FileManager.GetFileSize(pathDiffFile) > FileManager.GetFileSize(Path.Combine(pathTargetDir, record.FileName)))
                {
                    File.Delete(pathDiffFile);
                    CopyFileIntoPatch(record.FileName, Path.Combine(pathPatchingWorkDir, NAME_PATCH_DIR), pathTargetDir);

                    // Add files to filesMarkedNEW and filesMarkedDELETE
                    // During deployment we will delete old file and copy the new one instead of patching the old file
                    filesMarkedDELETE.Add(record);
                    filesMarkedNEW.Add(record);
                }
            }
            // Remove all files that are in filesMarkedNEW from filesMarkedUPDATED
            filesMarkedUPDATED.ExceptWith(filesMarkedNEW);

            // Step 9 - Create PatchManifest.xml
            string patchName = NamingHelper.GetPatchName(reference, target);
            PatchManifest manifest = new PatchManifest(patchName, reference, target);
            // We need only files names in the manifest
            foreach (AndroidApplicationManifestRecord record in filesMarkedNEW)
            {
                manifest.AddFileNEW(record.FileName);
            }
            foreach (AndroidApplicationManifestRecord record in filesMarkedDELETE)
            {
                manifest.AddFileDELETE(record.FileName);
            }
            foreach (AndroidApplicationManifestRecord record in filesMarkedUPDATED)
            {
                manifest.AddFileUPDATED(record.FileName);
            }
            FileManager.SerializeObjectToXML(manifest, Path.Combine(pathPatchingWorkDir, NAME_PATCH_DIR, NAME_PATCH_MANIFEST));

            // Step 10 - Compress patch
            CompressionManager.CompressDirToZIP(Path.Combine(pathPatchingWorkDir, NAME_PATCH_DIR),
                                                Path.Combine(pathPatchingWorkDir, patchName));

            // Clean up
            // Delete temp directory
            FileManager.DeleteDirectory(pathPatchingWorkDir);
                
            Console.WriteLine(String.Format("Finished patch creation: {0} -> {1}", reference.PackagePath, target.PackagePath));
        }
    }
}
