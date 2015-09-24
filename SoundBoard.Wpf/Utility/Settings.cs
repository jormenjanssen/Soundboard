﻿using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using PropertyChanged;

namespace SoundBoard.Wpf.Utility
{
    public static class SettingsHelper
    {
        #region Public methods

        private static Settings _instance;

        public static Settings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = GetSettings();
                return _instance;
            }
            set { _instance = value; }
        }

        public static Settings GetSettings()
        {
            var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Riwo Soundboard", "settings.xml");
            if (!File.Exists(fileName))
                return new Settings();

            var serializer = new XmlSerializer(typeof(Settings));
            using (var textReader = new XmlTextReader(fileName))
            {
                using (var xmlReader = XmlReader.Create(textReader, new XmlReaderSettings()))
                {
                    return (Settings) serializer.Deserialize(xmlReader);
                }
            }
        }


        public static void StoreSettings(Settings settings)
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Riwo Soundboard");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Riwo Soundboard", "settings.xml");
            using (var stringWriter = new XmlTextWriter(fileName, Encoding.UTF8))
            {
                var serializer = new XmlSerializer(typeof(Settings));
                serializer.Serialize(XmlWriter.Create(stringWriter), settings);
            }
        }

        #endregion
    }


    [Serializable]
    [ImplementPropertyChanged]
    public class Settings
    {
        #region Public properties

        public string Username { get; set; }




        #endregion
    }
}