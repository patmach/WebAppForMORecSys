using System.Diagnostics;
using System.IO;

namespace WebAppForMORecSys.Loggers
{
    /// <summary>
    /// File logger that appends every message to selected text file
    /// </summary>
    public class MyFileLogger
    {
        /// <summary>
        /// Path to log file
        /// </summary>
        private string _path;

        /// <summary>
        /// Format of the string representation of date that is used
        /// </summary>
        public string DateFormat = "dd-MM-yyyy HH:mm:ss.f";
        public MyFileLogger(string path)
        {
            this._path = path;
        }

        /// <summary>
        /// Logs selected message to the file
        /// </summary>
        /// <param name="message">Message to be logged</param>
        public void Log(string message)
        {
            Task.Run(() =>
            {
                lock (this)
                {
                    using (var sw = new StreamWriter(_path, true))
                    {
                        sw.WriteLine(message);
                    }
                }
            });
        }
    }
}
