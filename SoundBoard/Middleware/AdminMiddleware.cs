using Microsoft.Owin;
using System;
using System.Threading.Tasks;

namespace SoundBoard.Middleware
{
    class AdminMiddleware : OwinMiddleware
    {
        public AdminMiddleware(OwinMiddleware OwinMiddleware): base(OwinMiddleware)
        {

        }

        public override Task Invoke(IOwinContext context)
        {
            throw new NotImplementedException();
        }
    }
}
