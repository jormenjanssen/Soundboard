using SoundBoard.AutoUpdate.Helpers;
using SoundBoard.AutoUpdate.Userinterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SoundBoard.AutoUpdate.ViewModels
{
    public class UpdatePropopsalViewModel : ViewModelBase
    {
        #region Private Fields

        private readonly IViewModelInteraction _viewModelInteraction;
        private readonly IProductUpdate _productUpdate;
        private readonly UpdateManager _updateManager;
        private readonly UpdateUserInterfaceFactory _userInterfaceFactory;

        private ICommand _UpdateCommand;
        private Control _centerPanel;
        private bool _isUpdateInProgress;

        #endregion

        #region Public Properties

        public IViewModelInteraction ViewModelInteraction
        {
            get { return _viewModelInteraction; }
        }

        public ImageSource ApplicationIcon
        {
            get { return AssemblyHelper.GetAssemblyApplicationImage(); }
            set { }
        }

        public ICommand UpdateCommand
        {
            get { return _UpdateCommand ?? new DelegateCommand(Update, CanUpdate); }
        }

        public ICommand SkipCommand
        {
            get { return _UpdateCommand ?? new DelegateCommand(Skip, CanSkip); }
        }

        public Control CenterPanel
        {
            get
            {
                return _centerPanel;
            }

            set
            {
                _centerPanel = value;
                RaiseNotifyPropertyChanged();
            }
        }

        #endregion

        #region Constructor

        public UpdatePropopsalViewModel(IViewModelInteraction viewModelInteraction ,UpdateManager updateManager, IProductUpdate productUpdate)
        {
            if (productUpdate == null)
                throw new ProductUpdateFailedException(new NullReferenceException("ProductUpdate!"));

            _viewModelInteraction = viewModelInteraction;
            _productUpdate = productUpdate;
            _updateManager = updateManager;
            _userInterfaceFactory = new UpdateUserInterfaceFactory(_updateManager);


            ShowUpdateInfo();
            
        }

        #endregion

        #region Private Methods

        public bool CanUpdate()
        {
            return _productUpdate.HasUpdate && !_isUpdateInProgress;
        }

        public bool CanSkip()
        {
            return !_productUpdate.IsManadatory && !_isUpdateInProgress;
        }

        private async void Update()
        {
            _isUpdateInProgress = true;
           CenterPanel = _userInterfaceFactory.CreateUpdateProgressControl(_productUpdate);

           try
           {
               var productUpdateResult = await _updateManager.UpdateProduct(_productUpdate);
           }
           catch(Exception ex)
           {
               TryCloseWindow();
           }
          
        }

        private void ShowUpdateInfo()
        {
            CenterPanel = new UpdateInfoControl();
            CenterPanel.DataContext = _productUpdate;
        }

        private void Skip()
        {
            TryCloseWindow();
        }

        private void TryCloseWindow()
        {
            if (_viewModelInteraction.CloseWindow != null)
                _viewModelInteraction.CloseWindow();
        }

        #endregion

    }
}
