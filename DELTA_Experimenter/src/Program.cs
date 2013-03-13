using System;
using System.Collections.Generic;
using System.IO;
using DELTA_Common.Utilities;
using DELTA_Experimenter.CommandLine;
using DELTA_Storage;

namespace DELTA_Experimenter
{
    public class Program
    {
        private static string PATH_COMMAND_LINE_SETTINGS;

        private static string KEY_COMMAND_LINE = "command_line";
        private static string KEY_APK_STORAGE = "apk_storage";
        private static string KEY_SYSTEM_TEMP_DIR = "system_temp_dir";

        private const string PATH_SETTINGS = "settings.config";

        public static void Main(string[] args)
        {
            if (!LoadSettings()) FailAndExit("Can't load application's settings.");

            ApplicationCommandLine commandLine = new ApplicationCommandLine(PATH_COMMAND_LINE_SETTINGS);
            commandLine.Start();

            Console.WriteLine("Press any key to exit the application . . .");
            Console.ReadKey();
        }

        /// <summary>
        /// Load application's settings.
        /// </summary>
        /// <returns>
        /// TRUE - if application's settings were loaded successfully.
        /// FALSE - otherwise.
        /// </returns>
        private static bool LoadSettings()
        {
            if (!File.Exists(PATH_SETTINGS)) return false;

            Dictionary<string, string> dictSettings = FileManager.ReadSettingsFile(PATH_SETTINGS);
            if (dictSettings == null) return false;

            if (!dictSettings.TryGetValue(KEY_COMMAND_LINE, out PATH_COMMAND_LINE_SETTINGS)) return false;
            if (!dictSettings.TryGetValue(KEY_APK_STORAGE, out StorageManager.PATH_APK_STORAGE)) return false;
            if (!dictSettings.TryGetValue(KEY_SYSTEM_TEMP_DIR, out FileManager.SYSTEM_TEMP_DIR)) return false;

            return true;
        }

        /// <summary>
        /// Call this method when application failed and needs to be stopped.
        /// </summary>
        /// <param name="message">
        /// Message that will be output for the user.
        /// </param>
        private static void FailAndExit(string message)
        {
            Console.WriteLine(String.Format("FATAL ERROR! APK Patch Creator failed.\nMessage: {0}\nPress any key to close the application . . .", message));
            Console.ReadKey();
            Environment.Exit(-1);
        }
    }
}
