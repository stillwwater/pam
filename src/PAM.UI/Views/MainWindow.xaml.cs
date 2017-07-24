using MahApps.Metro.Controls;
using PAM.UI.ViewModels;
using System;
using System.Windows;

namespace PAM.UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            Closed += (s, e) => CloseVM.Command.Execute(e);
        }
    }
}
