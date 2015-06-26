using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoundBoard.Helpers
{
    class TaskExtensions
    {
        private static ConcurrentBag<WeakReference<ManualResetEvent>> _unsignaledTaskCollection = new ConcurrentBag<WeakReference<ManualResetEvent>>();

        public static Task EventTask()
        {
            var task = new Task(() =>
            {
                using (var manualResetEvent = new ManualResetEvent(false))
                {
                    _unsignaledTaskCollection.Add(new WeakReference<ManualResetEvent>(manualResetEvent));
                    manualResetEvent.WaitOne();
                }

            });

            task.Start();

            return task;
        }

        public static void Signal()
        {
            foreach(var unsignaledTask in _unsignaledTaskCollection)
            {
                ManualResetEvent manualResetEvent;
                WeakReference<ManualResetEvent> manualResetEventReference;

                if(unsignaledTask.TryGetTarget(out manualResetEvent))
                {
                    manualResetEvent.Set();
                    _unsignaledTaskCollection.TryTake(out manualResetEventReference);
                }
                else
                {
                    _unsignaledTaskCollection.TryTake(out manualResetEventReference);
                }
            }
        }
    }
}
