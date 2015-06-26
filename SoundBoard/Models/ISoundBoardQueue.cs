using SoundBoard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoard.Models
{
    public interface ISoundBoardQueue
    {
        void Enqueue(SoundBoardItem soundBoardItem);

        void TryInvallidateQueuedItem();

        IEnumerable<SoundBoardItem> SoundboardQueue { get; }

        MediaQueue MediaQueue { get; }
    }
}
