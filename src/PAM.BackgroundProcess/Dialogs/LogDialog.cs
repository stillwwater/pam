using System.Windows.Forms;

namespace PAM.BackgroundProcess.Dialogs
{
    public partial class LogDialog : Form
    {
        /// <summary>
        /// Initializes a new instance of the LogDialog class.
        /// </summary>
        /// <param name="data"></param>
        public LogDialog(string data)
        {
            InitializeComponent();

            Load += (s, e) => LogTextBox.Text = data;
        }
    }
}
