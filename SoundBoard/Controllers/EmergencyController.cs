using SoundBoard.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace SoundBoard.Controllers
{
    public class EmergencyController : ApiController
    {
        public bool Get()
        {
            return MediaQueueListener.EmergencyFlag;
        }

        public bool Get(bool enableOrDisable)
        {
            if (enableOrDisable)
                MediaQueueListener.EnableEmergencyFlag();
            else
                MediaQueueListener.DisableEmergencyFlag();

            return MediaQueueListener.EmergencyFlag;
        }




    }
}
