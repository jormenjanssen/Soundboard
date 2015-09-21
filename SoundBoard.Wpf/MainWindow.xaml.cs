﻿namespace SoundBoard.Wpf
{
    #region Namespaces

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using PropertyChanged;
    using SoundBoard.Data;
    using SoundBoard.Wpf.Client;
    using SoundBoard.Wpf.Utility;

    #endregion

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [ImplementPropertyChanged]
    public partial class MainWindow
    {
        #region Private fields

       // private string _filterText;
        private readonly string _serverAddress;
        private readonly SynchronizationContext _syncContext = SynchronizationContext.Current;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        #endregion

        #region Constructor

        public string Filter { get; set; }

        public List<SoundBoardItem> FilteredItems
        {
            get
            {
                if (SoundBoardItems == null)
                    return new List<SoundBoardItem>();
                return !string.IsNullOrEmpty(Filter) ? SoundBoardItems.Where(d => d.Title.ToLower().Contains(Filter.ToLower())).ToList() : SoundBoardItems.ToList();
            }
        }

        public MainWindow()
        {
            var settings = SettingsHelper.GetSettings();
            while (string.IsNullOrEmpty(settings.Username))
            {
                var inputDialog = new InputDialog("Please enter your name:");
                if (inputDialog.ShowDialog() == true)
                {
                    settings.Username = inputDialog.Answer;
                    SettingsHelper.StoreSettings(settings);
                }
                else
                {
                    //close the app, we don't want anonymous users
                    Close();
                }
            }

            InitializeComponent();
            _serverAddress = ConfigurationManager.AppSettings["ServerAddress"].TrimEnd('/') + "/api";
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow, OnCanResizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));
            CommandBindings.Add(new CommandBinding(PlaySoundBoardItemCommand, PlaySoundBoardItem));
            CommandBindings.Add(new CommandBinding(ToggleLoggingCommand, ToggleLogging));
            Task.Run(async () => await GetSoundBoardItemsAsync());
            Task.Run(async () => await GetCurrentQueue());
        }

        private void ToggleLogging(object sender, ExecutedRoutedEventArgs e)
        {
            ShowLogging = !ShowLogging;
            Logging.Visibility = ShowLogging ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Public properties

        public static ICommand PlaySoundBoardItemCommand { get; } = new RoutedUICommand("PlaySoundBoardItem", "PlaySoundBoardItemCommand", typeof (MainWindow));


        public static ICommand ToggleLoggingCommand { get; } = new RoutedUICommand("ToggleLogging", "ToggleLoggingCommand", typeof(MainWindow));


        public bool ShowLogging { get; set; }
        public bool Connected { get; set; }
        public ObservableCollection<SoundBoardItem> SoundBoardItems { get; set; }
        public ObservableCollection<QueueLogInfo> QueueLogInfos { get; set; }

        #endregion

        #region  Private helper functions

        private async Task GetSoundBoardItemsAsync()
        {
            while (!_tokenSource.Token.IsCancellationRequested)
            {
                using (var soundboarClient = new SoundBoardClient(_serverAddress))
                {
                    try
                    {
                        var soundBoardItems = soundboarClient.GetSoundBoardItems();
                        IEnumerable<QueueLogInfo> logs = new List<QueueLogInfo>();
                        try
                        {
                            logs = soundboarClient.GetQueueLog(QueueLogInfos != null && QueueLogInfos.Any() ? QueueLogInfos.Max(d => d.QueueTimestamp) : DateTime.Today);
                        }
                        catch (Exception ex)
                        {
                            //Old server connection??
                        }
                        
                        RunOnGuiThread(() =>
                        {
                            if (SoundBoardItems == null)
                                SoundBoardItems = new ObservableCollection<SoundBoardItem>(soundBoardItems);
                            else
                                SoundBoardItems.UpdateFrom(soundBoardItems);

                            if (QueueLogInfos == null)
                                QueueLogInfos = new ObservableCollection<QueueLogInfo>(logs);
                            else
                            {
                                foreach (var logItem in logs)
                                {
                                    QueueLogInfos.Add(logItem);
                                }
                                
                            }
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
                await Task.Delay(TimeSpan.FromMilliseconds(1500), _tokenSource.Token);
            }
        }


        public ObservableCollection<SoundBoardItem> CurrentQueue { get; set; }

        public int QueueCount
        {
            get { return CurrentQueue != null ? CurrentQueue.Count : 0; }
        }

        private async Task GetCurrentQueue()
        {
            while (!_tokenSource.Token.IsCancellationRequested)
            {
                using (var soundboardClient = new SoundBoardClient(_serverAddress))
                {
                    try
                    {
                        var queue = new ObservableCollection<SoundBoardItem>(soundboardClient.GetQueue());
                        Debug.WriteLine(queue.Count);
                        RunOnGuiThread(() =>
                        {
                            CurrentQueue = queue;
                            Connected = true;
                        });

                    }
                    catch (Exception exception)
                    {
                        //todo log!!
                        Connected = false;
                        CurrentQueue = null;
                    }
                }
                await Task.Delay(TimeSpan.FromMilliseconds(500), _tokenSource.Token);
            }
        }

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

        private void PlaySoundBoardItem(object sender, ExecutedRoutedEventArgs e)
        {
            var id = (Guid) e.Parameter;
            PlayItem(id);
        }

        private void PlayItem(Guid id)
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
                    var tm = new Timer(state => { _syncContext.Post(s => { ErrorMessage.Content = ""; }, null); }, null, TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(6));
                }
            }
        }

        private void RunOnGuiThread(Action action)
        {
            if (_syncContext != SynchronizationContext.Current)
                _syncContext.Post(a => action(), null);
            else
                action();
        }

        #endregion

        public SoundBoardItem SelectedSoundBoardItem { get; set; }

        private void ItemsListBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter && SelectedSoundBoardItem != null)
                PlayItem(SelectedSoundBoardItem.Id);
                
        }
    }
}