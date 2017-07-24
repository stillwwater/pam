using PAM.Core.Models;
using System.Windows;

namespace PAM.UI.Helpers
{
    internal static class Dialog
    {
        /// <summary>
        /// Displays an Error MessageBox to the user.
        /// </summary>
        /// <param name="error"></param>
        /// <param name="caption"></param>
        public static void Alert(string error, string caption = null, bool log = false)
        {
            if (log)
            {
                Logger.Log.Write(caption ?? error);
            }
            caption = caption == null ? "PAM" : $"PAM - {caption}";

            MessageBox.Show(error, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
