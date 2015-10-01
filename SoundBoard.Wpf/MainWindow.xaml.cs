using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PropertyChanged;
using SoundBoard.Data;
using SoundBoard.Updating;
using SoundBoard.Wpf.Client;
using SoundBoard.Wpf.Utility;

namespace SoundBoard.Wpf
{

    #region Namespaces

    #endregion

    public class AccentColor
    {
        #region Constructor

        public AccentColor(string name, Color color)
        {
            Name = name;
            SolidColorBrush = new SolidColorBrush(color);
        }

        #endregion

        #region Public properties

        public string Name { get; set; }
        public SolidColorBrush SolidColorBrush { get; set; }

        #endregion
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [ImplementPropertyChanged]
    public partial class MainWindow
    {
        #region Private fields

        private static ICommand _editNameCommand = new RoutedUICommand("EditName", "EditNameCommand", typeof(MainWindow));

        // private string _filterText;
        private readonly string _serverAddress;
        private readonly SynchronizationContext _syncContext = SynchronizationContext.Current;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public List<AccentColor> AccentColors { get; } = new List<AccentColor>
        {
            new AccentColor("Darken", Color.FromRgb(29, 29, 29)),
            new AccentColor("Yellow", Color.FromRgb(244, 179, 0)),
            new AccentColor("Orange", Color.FromRgb(227, 162, 26)),
            new AccentColor("LightGreen", Color.FromRgb(90, 142, 34)),
            new AccentColor("Green", Color.FromRgb(0, 163, 0)),
            new AccentColor("Teal", Color.FromRgb(0, 171, 169)),
            new AccentColor("Blue", Color.FromRgb(45, 137, 239)),
            new AccentColor("DarkBlue", Color.FromRgb(43, 87, 151)),
            new AccentColor("Red", Color.FromRgb(238, 17, 17)),
            new AccentColor("DarkRed", Color.FromRgb(185, 29, 71)),
            new AccentColor("Purple", Color.FromRgb(126, 56, 120)),
            new AccentColor("DarkPurple", Color.FromRgb(96, 60, 186))
        };

        #endregion

        #region Constructor

        public MainWindow()
        {
            UpdateManager.ApplyUpdateIfAvailable(true, 10);

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

            SelectedAccentColor = AccentColors.First(d => d.Name == (!string.IsNullOrEmpty(settings.PreferedColorSchema) ? settings.PreferedColorSchema : "Blue"));

            InitializeComponent();
            _serverAddress = ConfigurationManager.AppSettings["ServerAddress"].TrimEnd('/') + "/api";
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow, OnCanResizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));
            CommandBindings.Add(new CommandBinding(PlaySoundBoardItemCommand, PlaySoundBoardItem));
            CommandBindings.Add(new CommandBinding(ToggleLoggingCommand, ToggleLogging));
            CommandBindings.Add(new CommandBinding(EditNameCommand, EditName));
            Task.Run(async () => await GetSoundBoardItemsAsync());
            Task.Run(async () => await GetCurrentQueue());
        }

        #endregion

        #region Public properties

        public AccentColor SelectedAccentColor { get; set; }

        private void OnSelectedAccentColorChanged()
        {
            var settings = SettingsHelper.GetSettings();
            settings.PreferedColorSchema = SelectedAccentColor.Name;
            SettingsHelper.StoreSettings(settings);
        }

        public string Filter { get; set; }

        public List<SoundBoardItem> FilteredItems
        {
            get
            {
                if (SoundBoardItems == null)
                    return new List<SoundBoardItem>();
                return !string.IsNullOrEmpty(Filter) ? SoundBoardItems.Where(d => d.Title.ToLower().Contains(Filter.ToLower()) || d.Category.ToLower().Contains(Filter.ToLower())).ToList() : SoundBoardItems.ToList();
            }
        }

        public static ICommand PlaySoundBoardItemCommand { get; } = new RoutedUICommand("PlaySoundBoardItem", "PlaySoundBoardItemCommand", typeof(MainWindow));


        public static ICommand ToggleLoggingCommand { get; } = new RoutedUICommand("ToggleLogging", "ToggleLoggingCommand", typeof(MainWindow));


        public bool ShowLogging { get; set; }
        public bool Connected { get; set; }
        public ObservableCollection<SoundBoardItem> SoundBoardItems { get; set; }
        public ObservableCollection<QueueLogInfo> QueueLogInfos { get; set; }


        public ObservableCollection<SoundBoardItem> CurrentQueue { get; set; }

        public int QueueCount => CurrentQueue?.Count ?? 0;

        public bool EmergencyOn { get; set; }

        public static ICommand EditNameCommand
        {
            get { return _editNameCommand; }
        }

        public SoundBoardItem SelectedSoundBoardItem { get; set; }

        #endregion

        #region  Private helper functions

        private void EditName(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            var settings = SettingsHelper.GetSettings();
            var inputDialog = new InputDialog("Please enter your name:", settings.Username);
            if (inputDialog.ShowDialog() == true)
            {
                settings.Username = inputDialog.Answer;
                SettingsHelper.StoreSettings(settings);
            }
            while (string.IsNullOrEmpty(settings.Username))
            {
                inputDialog = new InputDialog("Please enter your name:", settings.Username);
                if (inputDialog.ShowDialog() != true) continue;
                settings.Username = inputDialog.Answer;
                SettingsHelper.StoreSettings(settings);
            }
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
                        var emergencyOn = await soundboardClient.GetEmergencyStatusAsync();
                        Debug.WriteLine(queue.Count);
                        RunOnGuiThread(() =>
                        {
                            CurrentQueue = queue;
                            Connected = true;
                            EmergencyOn = emergencyOn;
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

        private void ItemsListBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && SelectedSoundBoardItem != null)
                PlayItem(SelectedSoundBoardItem.Id);
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

        private void PlaySoundBoardItem(object sender, ExecutedRoutedEventArgs e)
        {
            var id = (Guid) e.Parameter;
            PlayItem(id);
        }

        private void RunOnGuiThread(Action action)
        {
            if (_syncContext != SynchronizationContext.Current)
                _syncContext.Post(a => action(), null);
            else
                action();
        }

        private void ToggleLogging(object sender, ExecutedRoutedEventArgs e)
        {
            ShowLogging = !ShowLogging;
        }

        #endregion
    }
}