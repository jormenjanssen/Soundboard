using System;

namespace SoundBoard.Models
{
    public class SoundBoardItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }

        public SoundBoardLogo SoundboardLogo { get; set; }
    }
}
