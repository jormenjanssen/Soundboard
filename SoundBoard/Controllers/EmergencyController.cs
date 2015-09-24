using System.Web.Http;
using SoundBoard.Helpers;

namespace SoundBoard.Controllers
{
    public class EmergencyController : ApiController
    {
        #region Public methods

        public bool Get()
        {
            MediaQueueListener.EmergencyFlag = !MediaQueueListener.EmergencyFlag;
            return MediaQueueListener.EmergencyFlag;
        }

        #endregion
    }
}