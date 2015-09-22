using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoard.AutoUpdate.Helpers
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
