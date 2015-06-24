using System;
using System.Collections.Concurrent;

namespace SoundBoard.Models
{
    public interface ISoundBoardItemSource
    {
        SoundBoardItem TryGetById(Guid id);

        SoundBoardItem TryGetByFilename(string filename);

        ConcurrentDictionary<string,SoundBoardItem> Items { get; }
        
    }
}
