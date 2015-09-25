using System;
using System.Linq;
using System.Web.Http;
using SoundBoard.Data;
using SoundBoard.Helpers;

namespace SoundBoard.Controllers
{
    public class EmergencyController : ApiController
    {
        #region Public methods

        private static string _lockedByName;
        public bool Get()
        {
            var username = Request.Headers.Contains("username") ? Request.Headers.GetValues("username").First() : "anonymous";

          
            if ((MediaQueueListener.EmergencyFlag && _lockedByName.Equals(username, StringComparison.InvariantCultureIgnoreCase)) || !MediaQueueListener.EmergencyFlag)
            {
                if (!MediaQueueListener.EmergencyFlag)
                    QueueLog.LogEmergency(username);
                _lockedByName = username;
                MediaQueueListener.EmergencyFlag = !MediaQueueListener.EmergencyFlag;
            }

            return MediaQueueListener.EmergencyFlag;
        }


        public bool Get(bool status)
        {
            return MediaQueueListener.EmergencyFlag;
        }
        #endregion
    }
}