using SoundBoard.Data;
using SoundBoard.Models;
using System.Collections.Generic;
using System.Web.Http;

namespace SoundBoard.Controllers
{
    public class SoundController : ApiController
    {
        public IEnumerable<SoundBoardItem> Get()
        {
            if (!Request.Headers.Contains("version"))
                return null;

            var soundBoardItemSource = SoundBoardItemSource.GetInstance();
            return soundBoardItemSource.Items.Values;
        }

    }
}
