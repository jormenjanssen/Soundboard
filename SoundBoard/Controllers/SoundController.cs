using SoundBoard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace SoundBoard.Controllers
{
    public class SoundController : ApiController
    {
        public IEnumerable<SoundBoardItem> Get()
        {
            var soundBoardItemSource = SoundBoardItemSource.GetInstance();
            return soundBoardItemSource.Items.Values;
        }

    }
}
