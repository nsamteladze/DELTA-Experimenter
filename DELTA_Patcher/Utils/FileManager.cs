using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DELTA_Patcher.Model;

namespace DELTA_Patcher.Utils
{
    public class FileManager
    {
        /// <summary>
        /// Reads file with application's setting in a dictionary.
        /// </summary>
        /// <param name="pathSettingFile">
        /// Path to the file with application's settings.
        /// </param>
        /// <returns>
        /// A dictionary where keys represent the settings' names and values
        /// represent settings values. NULL if file could not been read.
        /// </returns>
        public static Dictionary<string, string> ReadSettingsFile(string pathSettingFile)
        {
            if (!File.Exists(pathSettingFile)) return null;

            Dictionary<string, string> dictSettings = new Dictionary<string, string>();

            using (StreamReader reader = new StreamReader(pathSettingFile))
            {
                while (!reader.EndOfStream)
                {
                    string input = reader.ReadLine();
                    if (input.Equals(String.Empty)) continue;

                    string[] inputParts = input.Split(' ');
                    if (inputParts.Length != 2) return null;

                    dictSettings.Add(inputParts[0], inputParts[1]);
                }
            }

            return dictSettings;
        }

        /// <summary>
        /// Changes any extension of the specified file to .zip.
        /// </summary>
        /// <param name="filePath">
        /// Path to the file, which extension will be changed.
        /// </param>
        /// <returns>
        /// New file path.
        /// </returns>
        public static string ChangeExtension(string filePath, string newExtension)
        {
            string newFilePath = Path.ChangeExtension(filePath, newExtension);

            File.Move(filePath, newFilePath);

            return newFilePath;
        }

        /// <summary>
        /// Deletes everything in the specified directory.
        /// </summary>
        /// <param name="pathDir">
        /// Directory to be cleaned.
        /// </param>
        public static void CleanDirectory(string pathDir)
        {
            while (!IsDirectoryEmpty(pathDir))
            {
                try
                {
                    // Delete all the files in all the sub directories
                    foreach (string pathFile in Directory.EnumerateFiles(pathDir, "*", SearchOption.AllDirectories))
                    {
                        File.Delete(pathFile);
                    }
                    // Delete hierarchy of empty sub directories
                    foreach (string pathSubDir in Directory.EnumerateDirectories(pathDir, "*", SearchOption.TopDirectoryOnly))
                    {
                        DeleteEmptyDirectory(pathSubDir);
                    }

                }
                catch (Exception e)
                {
                    LogManager.Log(LogMessageType.Exception, e.Message, "FileManager");
                }
            }
        }

        /// <summary>
        /// Deletes the specified directory.
        /// NOTE: directory and all its sub directories cannot contain any files.
        /// </summary>
        /// <param name="pathDir">
        /// Directory to delete.
        /// </param>
        private static void DeleteEmptyDirectory(string pathDir)
        {
            try
            {
                if (!IsDirectoryEmpty(pathDir))
                {
                    foreach (string pathSubdir in Directory.EnumerateDirectories(pathDir, "*", SearchOption.TopDirectoryOnly))
                    {
                        DeleteEmptyDirectory(pathSubdir);
                    }
                }

                Directory.Delete(pathDir);
            }
            catch (Exception e)
            {
                LogManager.Log(LogMessageType.Exception, e.Message, "FileManager");
            }
        }

        /// <summary>
        /// Deletes the specified directory with its contents.
        /// </summary>
        /// <param name="pathDir">
        /// Directory to delete.
        /// </param>
        public static void DeleteDirectory(string pathDir)
        {
            CleanDirectory(pathDir);
            DeleteEmptyDirectory(pathDir);
        }

        /// <summary>
        /// Determines whether directory is empty or not.
        /// </summary>
        /// <param name="pathDir">
        /// Directory to check.
        /// </param>
        /// <returns>
        /// TRUE - if the specified directory is empty.
        /// FALSE - otherwise.
        /// </returns>
        private static bool IsDirectoryEmpty(string pathDir)
        {
            return !Directory.EnumerateFileSystemEntries(pathDir).Any();
        }

        /// <summary>
        /// Replaces '/' with the Windows style '\' making path understandable for Windows OS.
        /// </summary>
        /// <param name="path">
        /// Path to fix.
        /// </param>
        public static void FixSlashesInPath(ref string path)
        {
            path = path.Replace('/', '\\');
        }

        /// <summary>
        /// Returns file size.
        /// </summary>
        /// <param name="pathFile">
        /// File of interest.
        /// </param>
        /// <returns>
        /// File size in bytes.
        /// </returns>
        public static long GetFileSize(string pathFile)
        {
            // Returns 0 if file does not exist or is a directory
            if (!File.Exists(pathFile) || Directory.Exists(pathFile)) return 0;

            FileInfo fileInfo = new FileInfo(pathFile);
            return fileInfo.Length;
        }

        /// <summary>
        /// Reads all data from CSV file into a 2-dimentional array
        /// </summary>
        /// <param name="pathFileToRead"></param>
        /// <returns></returns>
        public static List<List<string>> ReadFromCSV(string pathFileToRead)
        {
            if (!File.Exists(pathFileToRead)) return null;

            List<List<string>> readData = new List<List<string>>();

            using (StreamReader reader = new StreamReader(pathFileToRead))
            {
                while (!reader.EndOfStream)
                {
                    string input = reader.ReadLine();

                    if (!input.Equals(String.Empty))
                    {
                        readData.Add(new List<string>(input.Split(',')));
                    }
                }
            }

            return readData;
        }

        /// <summary>
        /// Checks whether the path is valid.
        /// </summary>
        /// <param name="pathToCheck"></param>
        /// <returns></returns>
        public static bool IsValidPath(string pathToCheck)
        {
            string validPath;

            // GetFullPath returns exception is path is not valid
            try
            {
                validPath = Path.GetFullPath(pathToCheck);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return false;
            }

            // Relative path is not acceptable
            if (!Path.IsPathRooted(validPath)) return false;

            return true;
        }
    }
}
