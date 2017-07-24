namespace PAM.Setup.ViewModels
{
    internal interface IChildViewModel
    {
        string Name { get; }

        void SaveSettings();
    }
}
