using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoundBoard.Synchronization
{
    class DispatchSynchronizationContext : SynchronizationContext
    {
        private readonly Action<SendOrPostCallback,object> _onPost;
        private readonly Action<SendOrPostCallback, object> _onSend;

        public DispatchSynchronizationContext(Action<SendOrPostCallback, object> OnPost, Action<SendOrPostCallback, object> OnSend)
        {
            _onPost = OnPost;
            _onSend = OnSend;
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            _onSend(d, state);
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _onPost(d, state);
        }

    }
}
