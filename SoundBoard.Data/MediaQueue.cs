using System.Collections.Concurrent;

namespace SoundBoard.Data
{
    public class MediaQueue
    {
        #region Constructor

        public MediaQueue()
        {
            ItemQueue = new ConcurrentQueue<SoundBoardItem>();
        }

        #endregion

        #region Public properties

        public ConcurrentQueue<SoundBoardItem> ItemQueue { get; set; }

        public bool HasMediaItemsQueued
        {
            get { return !ItemQueue.IsEmpty; }
        }

        #endregion
    }
}