using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace PAM.UI.Helpers
{
    internal class CSV
    {
        /// <summary>
        /// Creates a new instance of the CSV class.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="lineEndings"></param>
        public CSV(string file, params string[] header)
        {
            Content = new List<string>();
            CSVFile = file;

            if (!File.Exists(file) || new FileInfo(file).Length == 0)
            {
                AppendRow(header);
            }
        }

        public List<string> Content { get; private set; }

        public string CSVFile { get; set; }

        /// <summary>
        /// Creates a new instance of the CSV class, and loads contents from a CSV file.
        /// </summary>
        /// <param name="csvFile"></param>
        public void Load()
        {
            Content = File.ReadAllLines(CSVFile).ToList();
        }

        /// <summary>
        /// Append a row of data to the CSV's Content.
        /// </summary>
        /// <param name="columns"></param>
        public void AppendRow(params string[] columns)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < columns.Length; i++)
            {
                sb.Append(columns[i]);

                if (i != columns.Length - 1) { sb.Append(','); }
            }
            Content.Add(sb.ToString());
        }

        /// <summary>
        /// Enumerates rows from the CSV's Content.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ReadRows()
        {
            foreach (string row in Content)
            {
                yield return row;
            }
        }

        /// <summary>
        /// Saves content to CSV file
        /// </summary>
        /// <param name="csvFile"></param>
        public void Save()
        {
            File.AppendAllLines(CSVFile, Content);
        }

        /// <summary>
        /// Loads data from a CSV file to a DataTable
        /// </summary>
        /// <returns></returns>
        public DataTable ToDataTable(bool readFile = false)
        {
            var rows = readFile && File.Exists(CSVFile)
                     ? File.ReadLines(CSVFile)
                     : ReadRows();

            string header = rows.First();

            var dt = new DataTable();

            foreach (string h in header.Split(','))
            {
                dt.Columns.Add(new DataColumn(h, typeof(string)));
            }

            foreach (string r in rows)
            {
                if (r == header) { continue; }

                dt.Rows.Add(r.Split(','));
            }

            return dt;
        }
    }
}
