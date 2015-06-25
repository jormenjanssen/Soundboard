namespace SoundBoard.Wpf
{
    #region Namespaces

    using System;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using PropertyChanged;
    using SoundBoard.Wpf.Client;
    using SoundBoard.Data;
    using System.Threading.Tasks;

    #endregion

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [ImplementPropertyChanged]
    public partial class MainWindow
    {
        #region Constructor

        private static readonly ICommand _playSoundBoardItemCommand = new RoutedUICommand("PlaySoundBoardItem", "PlaySoundBoardItemCommand", typeof(MainWindow));

        private readonly SynchronizationContext _syncContext = SynchronizationContext.Current;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public static ICommand PlaySoundBoardItemCommand
        {
            get { return _playSoundBoardItemCommand; }
        }

        public MainWindow()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow, OnCanResizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));
            CommandBindings.Add(new CommandBinding(PlaySoundBoardItemCommand, PlaySoundBoardItem));
            Task.Run(async () => await GetSoundBoardItemsAsync());
           
        }


        private void RunOnGuiThread(Action action)
        {
            if (_syncContext != SynchronizationContext.Current)
                _syncContext.Post(a => action(), null);
            else
                action();
        }

        public bool Connected { get; set; }

        private async Task GetSoundBoardItemsAsync()
        {
            while (!_tokenSource.Token.IsCancellationRequested)
            {
                using (var soundboarClient = new SoundBoardClient())
                {
                    try
                    {
                        var soundBoardItems = new ObservableCollection<SoundBoardItem>(soundboarClient.GetSoundBoardItems());
                        RunOnGuiThread(() =>
                        {
                            SoundBoardItems = soundBoardItems;
                            Connected = true;
                        });
                    }
                    catch (Exception exception)
                    {
                        //todo log!!
                        Connected = false;
                        SoundBoardItems = null;
                    }
                               
                }
                await Task.Delay(TimeSpan.FromSeconds(5), _tokenSource.Token);
            }
        }

        private void PlaySoundBoardItem(object sender, ExecutedRoutedEventArgs e)
        {
            var id = (Guid)e.Parameter;
            using (var soundboarClient = new SoundBoardClient())
            {
                try
                {
                    soundboarClient.AddToQueue(id);
                }
                catch (Exception exception)
                {
                    //Todo log the exception
                    ErrorMessage.Content = "Error playing soundbank item";
                    Timer tm = new Timer(state =>
                    {
                        _syncContext.Post(s =>
                        {
                            ErrorMessage.Content = "";
                        }, null);

                    }, null, TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(6));
                }

            }
        }

        #endregion

        #region Public properties

        public ObservableCollection<SoundBoardItem> SoundBoardItems { get; set; }

        #endregion

        #region  Private helper functions

        private void OnCanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ResizeMode != ResizeMode.NoResize;
        }

        private void OnCanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip;
        }

        private void OnCloseWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void OnMaximizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void OnMinimizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void OnRestoreWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        #endregion
    }
}