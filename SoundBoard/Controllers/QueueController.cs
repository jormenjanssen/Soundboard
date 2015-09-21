using SoundBoard.Data;
using SoundBoard.Helpers;
using SoundBoard.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;

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
            var username = Request.Headers.Contains("username") ? Request.Headers.GetValues("username").First() : "anonymous";
            
            if (!id.HasValue)
                throw new ArgumentException("Id is not a vallid guid!");

            var soundBoardItem = _soundBoardItemSource.TryGetById(id.Value);
            if (soundBoardItem == null)
                throw new MediaNotFoundException();


           
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("{0} User {1} requested media item {2} for queuing", DateTime.Now, username, soundBoardItem.Title);
            Console.ForegroundColor = ConsoleColor.White;
            QueueLog.LogQueue(soundBoardItem, username);
            _mediaQueue.Enqueue(soundBoardItem);

            return soundBoardItem;
        }

        public IEnumerable<SoundBoardItem> Get()
        {
            return _mediaQueue.SoundboardQueue;
        }


    }
}
