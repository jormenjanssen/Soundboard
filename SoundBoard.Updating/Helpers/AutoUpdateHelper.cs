using System;
using System.Configuration;
using log4net;
using SoundBoard.AutoUpdate;

namespace SoundBoard.Updating.Helpers
{
    internal static class AutoUpdateHelper
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Uri GetProductUpdateSourceUri()
        {
            var orginalSource = UpdateManager.AutoUpdateUrl;
            string updateSource = null;

            try
            {
               updateSource = ConfigurationManager.AppSettings["UpdateSource"];
            }
            catch(Exception ex)
            {
                _log.Warn("UpdateSource variable is not set in application configuration, using default instead",ex);
            }

            if (updateSource == null)
                return AssemblyHelper.GetAssemblyUrl(orginalSource);

            return AssemblyHelper.GetAssemblyUrl(updateSource);
               
        }


    }
}
