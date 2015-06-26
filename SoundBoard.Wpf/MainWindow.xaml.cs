namespace SoundBoard.Wpf
{
    #region Namespaces

    using System;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using System.Threading.Tasks;
    using NHotkey;
    using NHotkey.Wpf;
    using PropertyChanged;
    using SoundBoard.Wpf.Client;
    using SoundBoard.Data;

    #endregion

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [ImplementPropertyChanged]
    public partial class MainWindow
    {
        #region Constructor

        private static readonly ICommand _playSoundBoardItemCommand = new RoutedUICommand("PlaySoundBoardItem", "PlaySoundBoardItemCommand", typeof(MainWindow));
        private string _serverAddress;
        private readonly SynchronizationContext _syncContext = SynchronizationContext.Current;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public static ICommand PlaySoundBoardItemCommand
        {
            get { return _playSoundBoardItemCommand; }
        }

        public MainWindow()
        {
            InitializeComponent();
            _serverAddress = ConfigurationManager.AppSettings["ServerAddress"].TrimEnd('/') + "/api";
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow, OnCanResizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));
            CommandBindings.Add(new CommandBinding(PlaySoundBoardItemCommand, PlaySoundBoardItem));
            Task.Run(async () => await GetSoundBoardItemsAsync());

            for (var digit = 0; digit <= 9; ++digit)
            {
                HotkeyManager.Current.AddOrReplace("Sound-" + digit, Key.D0 + digit, ModifierKeys.Control | ModifierKeys.Shift, OnSound);
            }
        }

        private void OnSound(object sender, HotkeyEventArgs e)
        {
            var index = e.Name.IndexOf('-');
            if (index >= 0)
            {
                var sound = int.Parse(e.Name.Substring(index + 1));
                if ((sound >= 0) && (sound < SoundBoardItems.Count))
                    PlaySoundboardId(SoundBoardItems[sound].Id);
            }
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
                using (var soundboarClient = new SoundBoardClient(_serverAddress))
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

        private void PlaySoundboardId(Guid id)
        {
            using (var soundboarClient = new SoundBoardClient(_serverAddress))
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

        private void PlaySoundBoardItem(object sender, ExecutedRoutedEventArgs e)
        {
            PlaySoundboardId((Guid)e.Parameter);
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