using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DELTA_Patcher.Model;

namespace DELTA_Patcher.Utils
{
    public static class LogManager
    {
        public static void Log(LogMessageType messageType, string message, string reporter)
        {
            Console.WriteLine(String.Format("{0} | {1}: {2}.", reporter, messageType, message));
        }

        private static string ConvertLogMessageTypeToString(LogMessageType messageType)
        {
            switch (messageType)
            {
                case LogMessageType.Message:
                    return "Message";
                case LogMessageType.Error:
                    return "Error";
                case LogMessageType.Warning:
                    return "Warning";
                case LogMessageType.Exception:
                    return "Exception";
                default:
                    return "Unknown";
            }

        }

    }
}
