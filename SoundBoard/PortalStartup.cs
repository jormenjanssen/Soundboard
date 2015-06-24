using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using System.IO;
using SoundBoard.Middleware;

namespace SoundBoard
{
    public class PortalStartup
    {
        private const string WebPortalUrl = "/Webportal";

        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            appBuilder.UseWebApi(config);


            var appFolder = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "Webportal");

            appBuilder.UseFileServer(new Microsoft.Owin.StaticFiles.FileServerOptions
            {
                RequestPath = new PathString(WebPortalUrl),
                FileSystem = new PhysicalFileSystem(appFolder),
                EnableDirectoryBrowsing = true

            });

            appBuilder.Map(PathString.Empty, a => a.Use<PortalRedirectionMiddelware>(WebPortalUrl));
            appBuilder.Use<AdminMiddleware>();

            
        }
    }
}
