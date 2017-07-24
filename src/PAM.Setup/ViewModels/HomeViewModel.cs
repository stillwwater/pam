using PAM.Setup.Models;

namespace PAM.Setup.ViewModels
{
    internal class HomeViewModel : IChildViewModel
    {
        /// <summary>
        /// Initializes a new instance of the HomeViewModel class.
        /// </summary>
        public HomeViewModel()
        {
        }

        public string Name
        {
            get => string.Empty;
        }

        public Strings Strings
        {
            get => Strings.GetInstance();
        }

        public void SaveSettings()
        {
        }
    }
}
