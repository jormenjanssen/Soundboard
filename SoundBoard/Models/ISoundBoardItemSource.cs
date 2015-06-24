using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoard.Models
{
    public interface ISoundBoardItemSource
    {
        SoundBoardItem TryGetById(Guid id);

        SoundBoardItem TryGetByFilename(string filename);

        ConcurrentDictionary<string,SoundBoardItem> Items { get; }
        
    }
}
