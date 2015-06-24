using SoundBoard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace SoundBoard.Controllers
{
    public class QueueController : ApiController
    {
        private readonly ISoundBoardItemSource _soundBoardItemSource;

        public QueueController(ISoundBoardItemSource soundBoardItemSource)
        {
            _soundBoardItemSource = _soundBoardItemSource;
        }

        public SoundBoardItem Get(Guid? id)
        {
            if (!id.HasValue)
                throw new ArgumentException("Id is not a vallid guid!");

            SoundBoardItem soundBoardItem = null;

            var soundBoardKeyValueItem = _soundBoardItemSource.TryGetById(id.Value);
            if (soundBoardKeyValueItem == null)
                throw new MediaNotFoundException();


           
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("{0} User requested media item {1} for queuing", DateTime.Now,soundBoardItem.Title );
            Console.ForegroundColor = ConsoleColor.White;

            return soundBoardItem;
        }
    }
}
