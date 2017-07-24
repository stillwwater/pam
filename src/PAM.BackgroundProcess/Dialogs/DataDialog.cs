using PAM.Core.Models;
using System.Windows.Forms;

namespace PAM.BackgroundProcess.Dialogs
{
    public partial class DataDialog : Form
    {
        /// <summary>
        /// Initializes a new instance of the DataLog class and adds Activity data to a DataGridView.
        /// </summary>
        /// <param name="data"></param>
        public DataDialog(Activity[] data)
        {
            InitializeComponent();

            Load += (s, e) =>
            {
                foreach (var a in data)
                {
                    DataGrid.Rows.Add(a.Name, a.State, a.ProcessesArray.Length, a.ElapsedTime);
                }
            };
        }
    }
}
