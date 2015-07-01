using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoundBoard.Synchronization
{
    public interface ISynchronizationWorker
    {
        SynchronizationContext SynchronizationContext { get; }

        void Wait();

        void Cancel();
    }
}
