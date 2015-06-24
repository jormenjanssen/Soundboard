using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoard.Models
{
    public class SoundBoardLogo
    {
        public string ImageSource { get; set; }

        public static SoundBoardLogo Default
        {
            get
            {
                return new SoundBoardLogo
                {
                    ImageSource = "Default.png"
                };
            }
        }
    }
}
