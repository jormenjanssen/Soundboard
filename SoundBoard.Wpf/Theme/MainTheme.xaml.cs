using SoundBoard.Wpf.Properties;

namespace SoundBoard.Wpf.Theme
{
    #region Namespaces

    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    #endregion

    public partial class MainTheme
    {
        #region Private fields

        private TextBox _filterbox;
        private ListBox _itemsListBox;

        #endregion

        #region Constructor

        public MainTheme()
        {
            InitializeComponent();
        }

        #endregion

        #region  Private helper functions

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None) return;
            if (e.SystemKey != Key.F10)
                switch (e.Key)
                {
                    case Key.Up:
                    case Key.Right:
                    case Key.Down:
                    case Key.Left:
                        FocusListBox();
                        break;
                    case Key.Enter:
                        break;
                    default:
                        if (_filterbox == null)
                            _filterbox = UIHelper.FindChild<TextBox>(Application.Current.MainWindow, "Filter");
                        if (_filterbox != null)
                            _filterbox.Focus();
                        break;
                }
        }

        private void FocusListBox()
        {
            if (_itemsListBox == null)
                _itemsListBox = UIHelper.FindChild<ListBox>(Application.Current.MainWindow, "ItemsListBox");
            if (_itemsListBox != null)
                _itemsListBox.Focus();
        }

        #endregion

        private void Filter_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Right:
                case Key.Down:
                case Key.Left:
                    FocusListBox();
                    Send(e.Key);
                    break;
                case Key.Enter:
                    FocusListBox();
                    Send(e.Key);
                    break;
            }
        }

        private void Send(Key key)
        {
            if (Keyboard.PrimaryDevice != null)
            {
                if (Keyboard.PrimaryDevice.ActiveSource != null)
                {
                    var e = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, key)
                    {
                        RoutedEvent = Keyboard.KeyDownEvent
                    };
                    InputManager.Current.ProcessInput(e);

                    // Note: Based on your requirements you may also need to fire events for:
                    // RoutedEvent = Keyboard.PreviewKeyDownEvent
                    // RoutedEvent = Keyboard.KeyUpEvent
                    // RoutedEvent = Keyboard.PreviewKeyUpEvent
                }
            }
        }
    }




    public static class UIHelper
    {
        #region Public methods

        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                var childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        #endregion
    }
}