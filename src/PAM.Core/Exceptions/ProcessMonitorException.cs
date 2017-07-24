using System;

namespace PAM.Core.Exceptions
{
    public class ProcessMonitorException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the PAMCoreException class.
        /// </summary>
        /// <param name="message"></param>
        public ProcessMonitorException(string message)
            : base($"Error: Failed to initialize the Process Monitor, {message}") { }
    }
}
