using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoundBoard.Synchronization
{
    class SynchronizationWorker : ISynchronizationWorker
    {
        private readonly ManualResetEvent _manualThreadWaitEvent;
        private readonly ManualResetEvent _manualSendWorkerReleaseEvent;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CancellationToken _cancellationToken;

        private readonly SynchronizationContext _synchronizationContext;

        private readonly object _sendLock = new object();
        private readonly object _eventSignalLock = new object();

        private readonly ConcurrentQueue<Action> _postSynchronizationQueue;

        private bool _isSignaled = false;
        private Action _onSyncEvent = null;
        

        public SynchronizationContext SynchronizationContext
        {
            get
            {
                return _synchronizationContext;
            }
        }

        private SynchronizationWorker()
        {
            _manualThreadWaitEvent = new ManualResetEvent(false);

            _manualSendWorkerReleaseEvent = new ManualResetEvent(false);
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _synchronizationContext = new DispatchSynchronizationContext(OnPost, OnSend);
            _postSynchronizationQueue = new ConcurrentQueue<Action>();

            SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
        }

        public static ISynchronizationWorker Create()
        {
            return new SynchronizationWorker();
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Wait()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                _manualThreadWaitEvent.WaitOne();
                _isSignaled = true;

                while (!_cancellationToken.IsCancellationRequested)
                {
                    // Process a single send event.
                    if (_onSyncEvent != null)
                    {
                        _onSyncEvent();
                        _onSyncEvent = null;
                        _manualSendWorkerReleaseEvent.Set();
                    }
                    
                    Action postSynchronizationEvent;

                    // Process a single post event.
                    if(_postSynchronizationQueue.TryDequeue(out postSynchronizationEvent))
                        postSynchronizationEvent();

                }
                
                lock(_eventSignalLock)
                    _isSignaled = false;
            }
        }

        #region Private methods

        private void OnSend(SendOrPostCallback sendOrPostCallback,object state)
        {
            // Prevent other threads from sending.
            lock(_sendLock)
            {
                _onSyncEvent = new Action(() => sendOrPostCallback(state));

                // Try signal the manual reset event.
                TrySignal();

                // Wait in sync.
                _manualSendWorkerReleaseEvent.WaitOne();
            }
        }


        private void OnPost(SendOrPostCallback sendOrPostCallback,object state)
        {
            _postSynchronizationQueue.Enqueue(new Action(() => sendOrPostCallback(state)));

            // Try signal the manual reset event.
            TrySignal();
        }

        private void TrySignal()
        {
            // Prevent other post or sending events from signaling the manual reset event
            lock (_eventSignalLock)
            {
                if (!_isSignaled)
                {
                    _isSignaled = true;
                    _manualThreadWaitEvent.Set();
                }  
            }
        }

        #endregion

    }
}
