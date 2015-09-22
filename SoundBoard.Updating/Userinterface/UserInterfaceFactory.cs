using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SoundBoard.AutoUpdate;
using SoundBoard.Updating.ViewModels;

namespace SoundBoard.Updating.Userinterface
{
    public class UpdateUserInterfaceFactory
    {
        private readonly UpdateManager _updateManager;

        public UpdateUserInterfaceFactory(UpdateManager updateManager)
        {
            _updateManager = updateManager;
        }

        public Window CreateUpdateWindow()
        {
            var updateViewModel = new MainViewModel(_updateManager);

            var updateWindow = new Updating.Userinterface.UpdateWindow();
            updateWindow.DataContext = updateViewModel;

            return updateWindow as Window;
        }

        public UserControl CreateUpdateInfoControl(IProductUpdate productUpdate)
        {
            var updateInfoControl = new Updating.Userinterface.UpdateInfoControl();

            updateInfoControl.DataContext = productUpdate;

            return updateInfoControl;
        }

        public UserControl CreateUpdateCheckControl()
        {
            return new Updating.Userinterface.UpdateCheckControl();
        }

        public UserControl CreateUpdateProgressControl(IProductUpdate productUpdate)
        {
            var updateProgressControl = new Updating.Userinterface.UpdateProgressControl();
            updateProgressControl.DataContext = new UpdateProgressViewModel(_updateManager, productUpdate);

            return updateProgressControl;
        }

        public UserControl CreateUpdateSkeletonControl(UpdatePropopsalViewModel updateProposalViewModel)
        {
            var updatePanel = new Updating.Userinterface.UpdateSkeletonControl();
            updatePanel.DataContext = updateProposalViewModel;

            return updatePanel;
        }

        public UserControl CreateUpdateSkeletonControl(ViewModelActionDelegater viewModelActionDelegater,IProductUpdate productUpdate)
        {
            var updatePanel = new Updating.Userinterface.UpdateSkeletonControl();
            var updateProposalViewModel = new UpdatePropopsalViewModel(viewModelActionDelegater, _updateManager, productUpdate);



            updatePanel.DataContext = updateProposalViewModel;

            return updatePanel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usercontrol"></param>
        /// <returns>false if the user skipped.</returns>
        public Task<bool> AwaitUpdater(UserControl usercontrol, IProductUpdate productUpdate)
        {
            var updateSkeletonControl = usercontrol as Updating.Userinterface.UpdateSkeletonControl;
            
            if(updateSkeletonControl == null)
                throw new InvalidCastException("Control is not an UpdateSkeletonControl");

            if(updateSkeletonControl.DataContext == null)
                throw new NullReferenceException("Datacontext of UpdateSkeletonControl not set");

            var updatePropopsalViewModel =  updateSkeletonControl.DataContext as UpdatePropopsalViewModel;
            
            if(updatePropopsalViewModel == null)
                throw new InvalidCastException("Datacontext of UpdateSkeletonControl is not of type UpdateProposalViewModel");

            var manualResetEvent = new ManualResetEvent(false);

            updatePropopsalViewModel.ViewModelInteraction.CloseWindow = () => manualResetEvent.Set();

            var awaiterTask = new Task<bool>(() => 
            {
                // Wait for the async task to complete.
                manualResetEvent.WaitOne();

                // Dispose the manual reset event.
                manualResetEvent.Dispose();

                // Determine if we should close.
                return productUpdate.IsManadatory;
                
            });

            awaiterTask.Start();

            return awaiterTask;

        }



    }
}
