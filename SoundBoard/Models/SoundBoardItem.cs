using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoard.Models
{
    public class SoundBoardItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }

        public SoundBoardLogo SoundboardLogo { get; set; }
    }
}
