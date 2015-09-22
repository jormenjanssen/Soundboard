using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using SoundBoard.AutoUpdate;
using SoundBoard.Updating.Helpers;
using SoundBoard.Updating.Userinterface;

namespace SoundBoard.Updating.ViewModels
{
    public class MainViewModel : ViewModelBase, IViewModelInteraction
    {
        private readonly UpdateManager _updateManager;
        private readonly UpdateUserInterfaceFactory _userInterfaceFactory;

        private UpdatePropopsalViewModel _updateProposalViewModel;
        private UserControl _updateWindowControl;
        private Size _size;
        private bool _isShowing;

        #region Public Properties

        public UpdatePropopsalViewModel UpdateProposal
        {
            get { return _updateProposalViewModel; }
            set
            {
                _updateProposalViewModel = value;
                RaiseNotifyPropertyChanged();
            }
        }

        public UserControl Content
        {
            get
            { 
                return _updateWindowControl; 
            }
            set
            {
                _updateWindowControl = value;
                RaiseNotifyPropertyChanged();
                DesiredSize = new Size(_updateWindowControl.Width, _updateWindowControl.Height);
            
            }
        }

        public Size DesiredSize
        {
            get { return _size; }
            set
            {
                _size = value;
                RaiseNotifyPropertyChanged();
            }
        }

        #endregion

        #region Constructor

        public MainViewModel(UpdateManager updateManager)
        {
            _updateManager = updateManager;
            _userInterfaceFactory = new UpdateUserInterfaceFactory(updateManager);
        }

        public async void UpdateApplication()
        {
            ShowCheckingForUpdatesScreen();
            var productUpdate = await _updateManager.CheckForUpdatesAsync()
                                                    .DelayIfNeeded(new TimeSpan(0,0,3));
            if (productUpdate.HasUpdate)
                ShowUpdateAcceptation(productUpdate);
            else
                TryCloseWindow();
        }

        #endregion

        #region Private Methods

        #endregion

        #region Public Methods

        public void ShowCheckingForUpdatesScreen()
        {
            Content = _userInterfaceFactory.CreateUpdateCheckControl();
        }

        public void ShowUpdateAcceptation(IProductUpdate productUpdate)
        {
            _updateProposalViewModel = new UpdatePropopsalViewModel(this,_updateManager,productUpdate);
            Content = _userInterfaceFactory.CreateUpdateSkeletonControl(_updateProposalViewModel);
        }

        public void OnWindowLoaded()
        {
            UpdateApplication();
        }

        #endregion

        #region IMainViewModelInteraction Implementation

        public Func<bool?> ShowWindow { private get; set; }

        public Action CloseWindow { get; set; }

        public bool? TryShowWindow()
        {
            var canOpenWindow = Thread.CurrentThread.GetApartmentState() == ApartmentState.STA;

            if (ShowWindow != null && !_isShowing && canOpenWindow)
            {
                _isShowing = true;
                return ShowWindow();
            }

            return null;
        }

        public void TryCloseWindow()
        {
            if (CloseWindow != null)
                CloseWindow();
        }

        public Func<bool> ShutdownOnClose
        {
            get
            {
                if (_updateProposalViewModel == null)
                    return () => false;
                else
                    return () => (_updateProposalViewModel.CanUpdate() && !_updateProposalViewModel.CanSkip());
            }

        }

        #endregion






       
    }
}
