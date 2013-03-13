using System.Collections.Generic;
using System.IO;
using DELTA_Common.DataModel;
using DELTA_Common.Utilities;
using DELTA_Patcher.DataModel;
using DELTA_Storage;

namespace DELTA_Patcher
{
    public static class Patcher
    {
        #region String Constants

        private const string NAME_STAT_FILE = "stats.csv";
        private const string NAME_PATCHING_WORK_DIR = "DELTA_Pather";

        #endregion    

        public static string GetPatchingWorkDir()
        {
            return (Path.Combine(FileManager.SYSTEM_TEMP_DIR, NAME_PATCHING_WORK_DIR));
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
    }
}

