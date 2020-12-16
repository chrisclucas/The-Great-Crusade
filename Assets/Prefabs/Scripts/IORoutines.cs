using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CommonRoutines
{
    public class IORoutines
    {
        /// <summary>
        /// Writes message to the log file
        /// </summary>
        /// <param name="logEntry"></param>
        public static void WriteToLogFile(string logEntry)
        {
            using (StreamWriter writeFile = File.AppendText(GlobalGameFields.path + GlobalGameFields.logfile))
                writeFile.WriteLine(logEntry);
        }

        /// <summary>
        /// This routine returns a byte of 1 or 0 for a boolean value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte WriteBooleanToSaveFormat(bool value)
        {
            if (value == true)
                return (1);
            else
                return (0);
        }

        public static bool ReturnBoolFromSaveFormat(string value)
        {
            if (value == "1")
                return (true);
            else
                return (false);
        }
    }
}
