using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SoundBoard.AutoUpdate.ViewModels
{
    public class ViewModelActionDelegater : IViewModelInteraction, IDisposable
    {
        #region Private Fields

        private Func<bool?> _shutdownOnClose;
        private Action _closeWindow;
        private Action _onWindowLoaded;
        private WindowActionBinder _windowActionBinder;

        #endregion

        #region Public Properties

        public Func<bool?> ShutDownOnClose
        {
            set { _shutdownOnClose = value; }
        }

        public Action OnWindowLoaded
        {
            set { _onWindowLoaded = value; }
        }

        #endregion

        #region Constructor

        public ViewModelActionDelegater()
        {

        }

        #endregion

        #region IViewModelInteraction implementation

        public Func<bool?> ShowWindow
        {
            set { throw new NotImplementedException(); }
        }

        public Action CloseWindow
        {
            get
            {
                return _closeWindow;
            }
            set
            {
                _closeWindow = value;
            }
        }

        public Func<bool> ShutdownOnClose
        {
            get { throw new NotImplementedException(); }
        }

        void IViewModelInteraction.OnWindowLoaded()
        {

        }

        public void TryCloseWindow()
        {
            if (CloseWindow != null)
                CloseWindow();
        }

        #endregion

        public void TryAttachCloseEvent(Window window)
        {
            var windowActionBinder = new WindowActionBinder(window);
            var closeEventAttached = windowActionBinder.TryAttachClosingEvent(w => TryCloseWindow());
                Debug.Assert(closeEventAttached);

                _windowActionBinder = windowActionBinder;

        }




        public void Dispose()
        {
            if (_windowActionBinder != null)
                _windowActionBinder.Dispose();
        }
    }
}
