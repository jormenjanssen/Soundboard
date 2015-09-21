using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SoundBoard.Wpf.Utility
{
    public static class SettingsHelper
    {
        #region Public methods

        public static Settings GetSettings()
        {
            if (!File.Exists("settings.xml"))
                return new Settings();

            var serializer = new XmlSerializer(typeof(Settings));
            using (var textReader = new XmlTextReader("settings.xml"))
            {
                using (var xmlReader = XmlReader.Create(textReader, new XmlReaderSettings()))
                {
                    return (Settings) serializer.Deserialize(xmlReader);
                }
            }
        }


        public static void StoreSettings(Settings settings)
        {
            using (var stringWriter = new XmlTextWriter("settings.xml", Encoding.UTF8))
            {
                var serializer = new XmlSerializer(typeof(Settings));
                serializer.Serialize(XmlWriter.Create(stringWriter), settings);
            }
        }

        #endregion
    }


    [Serializable]
    public class Settings
    {
        #region Public properties

        public string Username { get; set; }

        #endregion
    }
}