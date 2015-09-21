using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using SoundBoard.Data;

namespace SoundBoard.Controllers
{
    public class QueueLogController : ApiController
    {
        #region Public methods

        public IEnumerable<QueueLogInfo> Get(DateTime dateTime)
        {
            return QueueLog.GetQueueLog().Where(d => d.QueueTimestamp.TrimMilliseconds() > dateTime);
        }



        #endregion
    }


    public static class DateTimeHelper
    {
        public static DateTime TrimMilliseconds(this DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0);
    }
}

}