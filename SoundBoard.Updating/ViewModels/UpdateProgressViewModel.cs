using System;
using System.Windows.Threading;
using SoundBoard.AutoUpdate;

namespace SoundBoard.Updating.ViewModels
{
    class UpdateProgressViewModel : ViewModelBase
    {
        #region Private Fields

        private readonly UpdateManager _updateManger;
        private readonly IProductUpdate _productUpdate;
        private readonly DispatcherTimer _dispatcherTimer;


        private int _progress;

        private long _bytesPerSec;

        #endregion

        #region Public Methods

        public int Progress
        {
            get { return _progress; }
            set
            {
                if(_progress != value)
                {
                    _progress = value;
                    RaiseNotifyPropertyChanged();
                }
            }
        }

        public long BytesPerSec
        {
            get { return _bytesPerSec; }
            set
            {
                if (value == 0)
                    return;

                if(_bytesPerSec != value)
                {
                    _bytesPerSec = value;
                    RaiseNotifyPropertyChanged();
                }
            }
        }

        #endregion

        #region Constructor

        public UpdateProgressViewModel(UpdateManager updateManger,IProductUpdate productUpdate)
        {
            _updateManger = updateManger;
            _productUpdate = productUpdate;
            _dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher.CurrentDispatcher);
            StartUpdateProgressTimer();

          
        }

        #endregion

        #region Private Methods

        private void StartUpdateProgressTimer()
        {
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
            _dispatcherTimer.Start();
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if(_productUpdate.Progress == 100)
            {
                _dispatcherTimer.Tick -= _dispatcherTimer_Tick;
                _dispatcherTimer.Stop();
            }

            Progress = _productUpdate.Progress;
            BytesPerSec = _productUpdate.BytesPerSec;

        }

        #endregion


    }
}
