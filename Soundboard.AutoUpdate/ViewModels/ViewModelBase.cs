namespace SoundBoard.AutoUpdate
{
    #region Namespaces

    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    #endregion

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region  Private helper functions

        /// <summary>
        /// If the property value changes, the oldValue will be replaced by the newValue
        /// and the INotifyPropertyChanged.PropertyChanged event will be raised.
        /// </summary>
        /// <typeparam name="TProperty">Type of the property.</typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="oldValue">Old value of the property.</param>
        /// <param name="newValue">New value of the property.</param>
        /// <returns><c>true</c> if the property value changed, otherwise <c>false</c>.</returns>
        protected bool ChangeProperty<TProperty>(ref TProperty oldValue, TProperty newValue, [CallerMemberName] string propertyName = "")
        {
            return this.SetProperty(PropertyChanged, ref oldValue, newValue, propertyName);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Public methods

        public void RaiseNotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            OnPropertyChanged(propertyName);
        }

        public void RaiseNotifyPropertyChanged<TProperty>(Expression<Func<TProperty>> projection)
        {
            var memberExpression = (MemberExpression) projection.Body;
            OnPropertyChanged(memberExpression.Member.Name);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public static class ProperyChangedHelper
    {
        #region Public methods

        public static bool SetProperty<T1, T2>(this T2 self, PropertyChangedEventHandler handler, ref T1 field, T1 newValue, [CallerMemberName] string propertyName = null) where T2 : INotifyPropertyChanged
        {
            if ((field == null && newValue != null) || ((field != null) && !field.Equals(newValue)))
            {
                field = newValue;

                if (handler != null)
                    handler(self, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }

        public static void RaiseNotifyPropertyChanged<T, TProperty>(this T self, PropertyChangedEventHandler handler, Expression<Func<TProperty>> projection)
        {
            var memberExpression = (MemberExpression)projection.Body;
            if (handler != null)
                handler(self, new PropertyChangedEventArgs(memberExpression.Member.Name));
        }

        public static void RaiseNotifyPropertyChanged<T>(this T self, PropertyChangedEventHandler handler, [CallerMemberName] string propertyName = "")
        {
            if (handler != null)
                handler(self, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}