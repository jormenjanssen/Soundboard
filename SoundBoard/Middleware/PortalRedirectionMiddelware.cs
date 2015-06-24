using Microsoft.Owin;
using System.Threading.Tasks;

namespace SoundBoard.Middleware
{
    class PortalRedirectionMiddelware : OwinMiddleware
    {
        private readonly string _url;

        public PortalRedirectionMiddelware(OwinMiddleware next,string url) : base(next)
        {
            _url = url;
        }

        public override Task Invoke(IOwinContext context)
        {
            if (context.Request.Path == PathString.Empty || context.Request.Path == new PathString("/"))
            {
                return Task.Run(() =>
                {
                    var response = new OwinResponse(context.Environment) {StatusCode = 502};
                    response.Redirect(_url);
                });
            }
            return Next.Invoke(context);
                
        }
    }

}