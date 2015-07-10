namespace SoundBoard.Synchronization
{
    #region Namespaces

    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    #endregion

    internal class SynchronizationWorker : ISynchronizationWorker
    {
        #region Private fields

        private readonly CancellationToken _cancellationToken;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly object _eventSignalLock = new object();
        private bool _isSignaled = false;
        private readonly ManualResetEvent _manualSendWorkerReleaseEvent;
        private readonly ManualResetEvent _manualThreadWaitEvent;
        private Action _onSyncEvent = null;
        private readonly ConcurrentQueue<Action> _postSynchronizationQueue;
        private readonly object _sendLock = new object();
        private readonly SynchronizationContext _synchronizationContext;

        #endregion

        #region Constructor

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

        #endregion

        #region  Private helper functions

        private void OnPost(SendOrPostCallback sendOrPostCallback, object state)
        {
            _postSynchronizationQueue.Enqueue(new Action(() => sendOrPostCallback(state)));

            // Try signal the manual reset event.
            TrySignal();
        }

        private void OnSend(SendOrPostCallback sendOrPostCallback, object state)
        {
            // Prevent other threads from sending.
            lock (_sendLock)
            {
                _onSyncEvent = new Action(() => sendOrPostCallback(state));

                // Try signal the manual reset event.
                TrySignal();

                // Wait in sync.
                _manualSendWorkerReleaseEvent.WaitOne();
            }
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

        #region Public methods

        public static ISynchronizationWorker Create()
        {
            return new SynchronizationWorker();
        }

        #endregion

        #region ISynchronizationWorker Members

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public SynchronizationContext SynchronizationContext
        {
            get { return _synchronizationContext; }
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
                    if (_postSynchronizationQueue.TryDequeue(out postSynchronizationEvent))
                        postSynchronizationEvent();
                }

                lock (_eventSignalLock)
                    _isSignaled = false;
            }
        }

        #endregion
    }
}