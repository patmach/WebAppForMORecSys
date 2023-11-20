using System.Diagnostics;
using System.IO;

namespace WebAppForMORecSys.Loggers
{
    public class MyFileLogger
    {
        private string path;

        public string format = "dd-MM-yyyy HH:mm:ss.f";
        public MyFileLogger(string path)
        {
            this.path = path;
        }

        public void Log(string message)
        {
            Task.Run(() =>
            {
                lock (this)
                {
                    using (var sw = new StreamWriter(path, true))
                    {
                        sw.WriteLine(message);
                    }

                }
            });
        }
    }
}
