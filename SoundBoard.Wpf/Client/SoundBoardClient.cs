using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using SoundBoard.Data;

namespace SoundBoard.Wpf.Client
{
    public class SoundBoardClient : ApiClientBase
    {
        #region Constructor

        public SoundBoardClient()
            : base("http://localhost:9000/api/")
        {
            //just for compatibility
        }

        public SoundBoardClient(string baseUrl)
            : base(baseUrl)
        {
        }

        #endregion

        #region Public methods

        public void AddToQueue(Guid id)
        {
            Execute<SoundBoardItem>("Queue?id=" + Uri.EscapeDataString(id.ToString()));
        }

        public Task<bool> EmergencyStopAsync()
        {
            return Task.Run(() => Execute<bool>("Emergency"));
        }

        public IEnumerable<SoundBoardItem> GetQueue()
        {
            return Execute<IEnumerable<SoundBoardItem>>("Queue");
        }

        public IEnumerable<QueueLogInfo> GetQueueLog(DateTime dateTime)
        {
            return Execute<IEnumerable<QueueLogInfo>>("QueueLog?dateTime=" + Uri.EscapeDataString(dateTime.ToString(CultureInfo.InvariantCulture)));
        }

        public IEnumerable<SoundBoardItem> GetSoundBoardItems()
        {
            return Execute<IEnumerable<SoundBoardItem>>("Sound");
        }

        #endregion
    }
}