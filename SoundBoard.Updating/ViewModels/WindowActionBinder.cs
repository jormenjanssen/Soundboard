using System;
using System.ComponentModel;
using System.Windows;

namespace SoundBoard.Updating.ViewModels
{
    class WindowActionBinder : IDisposable
    {
        private WeakReference<Window> _bindedWindow;
        private Action<Window> _windowAction;

        public WindowActionBinder(Window window)
        {
            _bindedWindow = new WeakReference<Window>(window);
        }

        public bool TryAttachClosingEvent(Action<Window> windowAction)
        {
            Window window = null;
            
            if( _bindedWindow.TryGetTarget(out window))
            {
                _windowAction = windowAction;
                window.Closing += OnWindowClose;
                return true;
            }

            return false;

        }

        public void Dispose()
        {
            Window window;

            if (_bindedWindow.TryGetTarget(out window))
            {
                window.Closing -= OnWindowClose;
            }
        }

        private void OnWindowClose(object sender, CancelEventArgs e)
        {
            Window window = null;

            if (_bindedWindow.TryGetTarget(out window))
                _windowAction(window);

            e.Cancel = true;
        }

    }
}
