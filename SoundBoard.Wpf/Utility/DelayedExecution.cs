using System;
using System.Diagnostics;
using System.Threading;

namespace SoundBoard.Wpf.Utility
{
    public class DelayedExecution
    {
        #region Private fields

        private Action _action;
        private Timer _delayedExecutionTimer;
        private DateTime? _firstStart;
        private DateTime? _lastStart;
        private TimeSpan? _maxTotalTimeOut;

        private readonly object _sync = new object();

        private TimeSpan _timeOut;

        #endregion

        #region Constructor

        public DelayedExecution()
            : this(TimeSpan.Zero, false)
        {
        }

        public DelayedExecution(bool isSynchronized)
            : this(TimeSpan.Zero, isSynchronized)
        {
        }

        public DelayedExecution(TimeSpan timeOut)
            : this(timeOut, false)
        {
        }

        public DelayedExecution(TimeSpan timeOut, bool isSynchronized)
        {
            TimeOut = timeOut;
            if (isSynchronized)
                SynchronizationContext = SynchronizationContext.Current;
        }

        #endregion

        #region Public properties

        public TimeSpan TimeOut
        {
            get
            {
                lock (_sync)
                {
                    return _timeOut;
                }
            }
            set
            {
                // Make sure the time-out is non-negative
                if (value < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("value", "TimeOut cannot be negative.");

                lock (_sync)
                {
                    // Set new time-out
                    if (value != _timeOut)
                    {
                        // Update the time-out
                        _timeOut = value;

                        // Reset the timer
                        SetTimer();
                    }
                }
            }
        }

        public TimeSpan? MaxTotalTimeOut
        {
            get
            {
                lock (_sync)
                {
                    return _maxTotalTimeOut;
                }
            }
            set
            {
                // Make sure the time-out is non-negative
                if (value.HasValue && (value.Value < TimeSpan.Zero))
                    throw new ArgumentOutOfRangeException("value", "MaxTotalTimeOut cannot be negative.");

                lock (_sync)
                {
                    // Set new time-out
                    if (value != _maxTotalTimeOut)
                    {
                        // Update the time-out
                        _maxTotalTimeOut = value;

                        // Reset the timer
                        SetTimer();
                    }
                }
            }
        }

        public SynchronizationContext SynchronizationContext { get; set; }

        #endregion

        #region  Private helper functions

        private void OnDelayedExecutionTimerElapsed()
        {
            Action performAction;
            lock (_sync)
            {
                // Remember values, before they can be changed
                performAction = _action;

                // Stop the current timer
                StopTimer();
            }

            // Execute the _action
            if (performAction != null)
            {
                // Execute the _action in the proper context
                if ((SynchronizationContext != null) && (SynchronizationContext.Current != SynchronizationContext))
                    SynchronizationContext.Send(state => performAction(), null);
                else
                    performAction();
            }
        }

        private void SetTimer()
        {
            SetTimer(DateTime.UtcNow);
        }

        private void SetTimer(DateTime utcNow)
        {
            // Check if we need to set the timer
            if (_action != null)
            {
                // Make sure '_firstStart' and '_lastStart' have been set
                // (should have been done in the 'Execute' method).
                Debug.Assert(_firstStart != null, "FirstStart should have been set.");
                Debug.Assert(_lastStart != null, "LastStart should have been set.");

                // Determine when the timer should fire
                var fireMoment = _lastStart.Value + _timeOut;

                // Check if a maximum time-out has been specified
                if (_maxTotalTimeOut.HasValue)
                {
                    var lastFireMoment = _firstStart.Value + _maxTotalTimeOut.Value;
                    if (fireMoment > lastFireMoment)
                        fireMoment = lastFireMoment;
                }

                // Determine the interval from the current moment
                var interval = fireMoment - utcNow;
                if (interval <= TimeSpan.Zero)
                {
                    // Call the method directly
                    OnDelayedExecutionTimerElapsed();
                }
                else
                {
                    // Set the timer
                    if (_delayedExecutionTimer == null)
                        _delayedExecutionTimer = new Timer(delegate { OnDelayedExecutionTimerElapsed(); });

                    // Set interval and enable timer
                    _delayedExecutionTimer.Change((int) interval.TotalMilliseconds, Timeout.Infinite);
                }
            }
        }

        private void StopTimer()
        {
            // Stop the timer
            if (_delayedExecutionTimer != null)
                _delayedExecutionTimer.Change(Timeout.Infinite, Timeout.Infinite);

            // Reset all timers
            _firstStart = null;
            _lastStart = null;

            // Reset _action
            _action = null;
        }

        #endregion

        #region Public methods

        public void Execute(Action action)
        {
            // An _action must be specified
            if (action == null)
                throw new ArgumentNullException("action");

            lock (_sync)
            {
                // Determine this moment
                var utcNow = DateTime.UtcNow;

                // Set first start to now, if it hasn't set before.
                if (!_firstStart.HasValue)
                    _firstStart = utcNow;

                // Last start should always be set
                _lastStart = utcNow;

                // Set the current _action
                _action = action;

                // Reset the timer
                SetTimer(utcNow);
            }
        }

        public void Reset()
        {
            lock (_sync)
            {
                // Stop the timer
                StopTimer();
            }
        }

        #endregion
    }
}