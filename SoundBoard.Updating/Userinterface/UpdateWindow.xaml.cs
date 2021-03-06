﻿using System;
using System.Windows;
using SoundBoard.Updating.ViewModels;

namespace SoundBoard.Updating.Userinterface
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        private IViewModelInteraction _viewModelInteraction;
        private bool _activatedOnce;

        public UpdateWindow()
        {
            InitializeComponent();
           // SetUiDefaults();

            this.DataContextChanged += (s, e) =>
            {
                _viewModelInteraction = e.NewValue as IViewModelInteraction;

                if (_viewModelInteraction != null)
                {
                    _viewModelInteraction.ShowWindow = () =>
                    {
                        return this.ShowDialog();
                    };

                    _viewModelInteraction.CloseWindow = () =>
                    {
                        this.Dispatcher.Invoke(() => this.Close());
                    };

                }



            };

        }

        protected override void OnActivated(EventArgs e)
        {
            if (_viewModelInteraction != null && !_activatedOnce)
            {
                _viewModelInteraction.OnWindowLoaded();
                _activatedOnce = true;
            }

            base.OnActivated(e);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_viewModelInteraction != null)
            {
                // Close the main application because user tries to skip the update mechanism.
                if (_viewModelInteraction.ShutdownOnClose())
                    Environment.Exit(0);

                _viewModelInteraction.CloseWindow = null;
                _viewModelInteraction.ShowWindow = null;

                _viewModelInteraction = null;
            }

            base.OnClosing(e);
        }

        #region Private Methods

        private void SetUiDefaults()
        {
            ResizeMode = System.Windows.ResizeMode.CanMinimize;

        }
        #endregion




    }
}
