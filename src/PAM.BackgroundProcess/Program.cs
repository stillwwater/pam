using System;
using System.Threading;
using System.Windows.Forms;

namespace PAM.BackgroundProcess
{
    internal static class Program
    {
        static Mutex mutex = new Mutex(true, "{B3A50E57-D1E4-41C1-96A2-7467AB237359}");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainWindow());

                mutex.ReleaseMutex();
            }
            else { }
        }
    }
}
