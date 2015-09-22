using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using log4net;
using SoundBoard.AutoUpdate;
using SoundBoard.Updating.Internal;

namespace SoundBoard.Updating.Helpers
{
    public static class AssemblyHelper
    {
        #region Private fields

        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static IEnumerable<string> IconConventions = new List<string>
        {
            "Application.ico",
            "App.ico",
        };

        #endregion

        #region Public methods

        public static ImageSource GetAssemblyApplicationImage()
        {
            var currentIconString = string.Empty;
            ImageSource currentImageSource = null;

            foreach (var iconString in IconConventions)
            {
                try
                {
                    var myImage = new Image
                    {
                        Source = new BitmapImage(new Uri(string.Format(@"/{0};component/{1}", GetCurrentAssemblyName(), iconString), UriKind.Relative))
                    };

                    currentImageSource = myImage.Source;
                    break;
                }
                catch (Exception ex)
                {
                    _log.Warn(string.Format("Failed to get application image by convention {0}", currentIconString), ex);
                }
            }

            return currentImageSource;
        }

        public static Uri GetAssemblyUrl(string source)
        {
            Uri combinedUri = null;

            var baseUri = new Uri(source);
            var xmlFile = string.Format("/{0}.autoupdate.xml", Assembly.GetEntryAssembly().GetName().Name);

            try
            {
                combinedUri = baseUri;
                combinedUri = new Uri(combinedUri.OriginalString + xmlFile);
            }
            catch (Exception ex)
            {
                _log.Error(new ProductUpdateFailedException(ex));
            }

            return combinedUri;
        }

        public static String GetCurrentAssemblyName()
        {
            return Assembly.GetEntryAssembly().GetName().Name;
        }

        public static Version GetCurrentVersion()
        {
            return Assembly.GetEntryAssembly().GetName().Version;
        }

        #endregion
    }
}