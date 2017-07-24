using PAM.Core.Models;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace PAM.Setup.Models
{
    [Serializable()]
    public class Strings : ISerializable
    {
        readonly static string _dataFile = "setup_strings.xml";
        static Strings _instance;

        private Strings()
        {
        }

        public string HomeDescription { get; set; }

        public string ActivitiesDescription { get; set; }

        public string QueryingDescription { get; set; }

        public string InterpretersDescription { get; set; }

        /// <summary>
        /// Gets a Singleton instance of the Strings class
        /// </summary>
        /// <returns></returns>
        public static Strings GetInstance()
        {
            if (_instance == null && File.Exists(_dataFile))
            {
                XmlSerializer ser = new XmlSerializer(typeof(Strings));
                FileStream fs = File.OpenRead(_dataFile);

                try
                {
                    // Deserialize data from file.
                    _instance = (Strings)ser.Deserialize(fs);
                }
                catch (XmlException e)
                {
                    Logger.Log.Write($"XML error while loading {_dataFile}: {e.Message}");
                }
                finally
                {
                    fs.Close();
                    fs.Dispose();
                }
            }

            return _instance;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }
    }
}
