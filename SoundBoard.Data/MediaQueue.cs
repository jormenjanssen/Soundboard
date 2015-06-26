using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoard.Data
{
    public class MediaQueue
    {
        public ConcurrentQueue<SoundBoardItem> ItemQueue { get; set; }

        public bool HasMediaItemsQueued
        {
            get { return !ItemQueue.IsEmpty; }
        }

        public MediaQueue()
        {
            ItemQueue = new ConcurrentQueue<SoundBoardItem>();
        }

    }
}
