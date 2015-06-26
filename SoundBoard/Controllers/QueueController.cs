using SoundBoard.Data;
using SoundBoard.Helpers;
using SoundBoard.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Http;

namespace SoundBoard.Controllers
{
    public class QueueController : ApiController
    {
        private readonly ISoundBoardItemSource _soundBoardItemSource;
        private readonly ISoundBoardQueue _mediaQueue;

        public QueueController()
        {
            _soundBoardItemSource = SoundBoardItemSource.GetInstance();
            _mediaQueue = MediaQueueListener.GetQueue();
        }

        public SoundBoardItem Get(Guid? id)
        {
            if (!id.HasValue)
                throw new ArgumentException("Id is not a vallid guid!");

            var soundBoardItem = _soundBoardItemSource.TryGetById(id.Value);
            if (soundBoardItem == null)
                throw new MediaNotFoundException();


           
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("{0} User requested media item {1} for queuing", DateTime.Now, soundBoardItem.Title);
            Console.ForegroundColor = ConsoleColor.White;

            _mediaQueue.Enqueue(soundBoardItem);

            return soundBoardItem;
        }

        public IEnumerable<SoundBoardItem> Get()
        {
            return _mediaQueue.SoundboardQueue;
        }


    }
}
