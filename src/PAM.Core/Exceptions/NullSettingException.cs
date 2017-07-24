using System;

namespace PAM.Core.Exceptions
{
    public class NullSettingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the NullSettingException class.
        /// </summary>
        /// <param name="message"></param>
        public NullSettingException(string setting)
            : base($"Error: Please define \"{setting}\" in the settings file and reload.") { }
    }
}
