using System;

namespace SoundBoard.Data
{
    public class SoundBoardItem
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string File { get; set; }

        public int PlayCount { get; set; }

        public SoundBoardLogo SoundboardLogo { get; set; }
    }
}
