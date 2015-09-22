using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoard.AutoUpdate.Helpers
{
    public static class TaskHelper
    {
        public static Task<T> DelayIfNeeded<T>(this Task<T> task,TimeSpan ts)
        {
            var currentTimebeforeStart = DateTime.UtcNow;

            var waitableTask = task.ContinueWith((t) =>
            {
                var currentTimeAfterTask = DateTime.UtcNow;
                var timeSpanBetweenTask = currentTimeAfterTask - currentTimebeforeStart;

                if (timeSpanBetweenTask < ts)
                {
                    var timeToWait = ts - timeSpanBetweenTask;
                    Task.Delay(timeToWait)
                        .Wait();
                }
                        
                return t.Result;
            });

            return waitableTask;
        }
    }
}
