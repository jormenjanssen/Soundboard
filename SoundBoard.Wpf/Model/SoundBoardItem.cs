using System;
using PropertyChanged;
using SoundBoard.Data;

namespace SoundBoard.Wpf.Model
{
    [ImplementPropertyChanged]
    public class SoundBoardItem
    {
        #region Public properties

        public Guid Id { get; set; }

        public string Title { get; set; }

        public string File { get; set; }

        public int PlayCount { get; set; }

        public string Category { get; set; }

        public bool IsFavorite { get; set; }

        private void OnIsFavoriteChanged()
        {
            Console.WriteLine("Hello world");
        }

        public SoundBoardLogo SoundboardLogo { get; set; }

        #endregion
    }
}