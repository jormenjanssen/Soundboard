using System;
using System.Collections.Generic;

namespace SoundBoard.Data
{
    public static class QueueLog
    {
        #region Private fields

        private static readonly List<QueueLogInfo> _queueLog = new List<QueueLogInfo>();

        #endregion

        #region Public methods

        public static void LogQueue(SoundBoardItem item, string queuedBy)
        {
            if (item == null) return;
            item.PlayCount ++;
            _queueLog.Add(new QueueLogInfo(queuedBy, item));
        }


        public static List<QueueLogInfo> GetQueueLog()
        {
            return _queueLog;
        } 
        #endregion
    }

    public class QueueLogInfo
    {
        public QueueLogInfo(string queuedBy, SoundBoardItem item)
        {
            QueueTimestamp = DateTime.Now;
            QueuedBy = queuedBy;
            SampleName = item.Title;
            ItemId = item.Id;
        }

        public QueueLogInfo()
        {
        }

        public string SampleName { get; set; }
        public Guid ItemId { get; set; }
        public DateTime QueueTimestamp { get; set; }
        public string QueuedBy { get; set; }
    }
}