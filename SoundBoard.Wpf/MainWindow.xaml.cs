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
using System.Windows.Interop;
using System.Windows.Media;
using AutoMapper;
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

        private readonly DelayedExecution _delayedExecution = new DelayedExecution(TimeSpan.FromMilliseconds(500));

        private static readonly ICommand _editNameCommand = new RoutedUICommand("EditName", "EditNameCommand", typeof(MainWindow));

        private readonly string _serverAddress;
        private readonly Settings _settings = SettingsHelper.GetSettings();
        private readonly SynchronizationContext _syncContext = SynchronizationContext.Current;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        #endregion

        #region Constructor

        public MainWindow()
        {
            UpdateManager.ApplyUpdateIfAvailable(true, 10);
            Mapper.CreateMap<SoundBoardItem, Model.SoundBoardItem>().ForMember(s => s.IsFavorite, source => source.MapFrom(s => _settings.Favorites.Any(d => d == s.Id)));


            while (string.IsNullOrEmpty(_settings.Username))
            {
                var inputDialog = new InputDialog("Please enter your name:");
                if (inputDialog.ShowDialog() == true)
                {
                    _settings.Username = inputDialog.Answer;
                    SettingsHelper.StoreSettings(_settings);
                }
                else
                {
                    //close the app, we don't want anonymous users
                    Close();
                }
            }

            SelectedAccentColor = AccentColors.First(d => d.Name == (!string.IsNullOrEmpty(_settings.PreferedColorSchema) ? _settings.PreferedColorSchema : "Blue"));

            InitializeComponent();

            _serverAddress = ConfigurationManager.AppSettings["ServerAddress"].TrimEnd('/') + "/api";
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow, OnCanResizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));
            CommandBindings.Add(new CommandBinding(PlaySoundBoardItemCommand, PlaySoundBoardItem));
            CommandBindings.Add(new CommandBinding(ToggleFavoriteCommand, ToggleFavorite));
            CommandBindings.Add(new CommandBinding(ToggleLoggingCommand, ToggleLogging));
            CommandBindings.Add(new CommandBinding(EditNameCommand, EditName));

            Task.Run(async () => await GetSoundBoardItemsAsync());
            Task.Run(async () => await GetCurrentQueue());
        }

        #endregion

        #region Public properties

        public List<AccentColor> AccentColors { get; } = new List<AccentColor>
        {
            new AccentColor("Darken", Color.FromRgb(29, 29, 29)),
            new AccentColor("Yellow", Color.FromRgb(244, 179, 0)),
            new AccentColor("Orange", Color.FromRgb(220, 60, 0)),
            new AccentColor("LightGreen", Color.FromRgb(59, 142, 34)),
            new AccentColor("Green", Color.FromRgb(16, 124, 16)),
            new AccentColor("Teal", Color.FromRgb(0, 171, 169)),
            new AccentColor("Blue", Color.FromRgb(0, 120, 215)),
            new AccentColor("DarkBlue", Color.FromRgb(43, 87, 151)),
            new AccentColor("Red", Color.FromRgb(209, 52, 56)),
            new AccentColor("DarkRed", Color.FromRgb(185, 29, 71)),
            new AccentColor("Purple", Color.FromRgb(128, 57, 132)),
            new AccentColor("DarkPurple", Color.FromRgb(96, 60, 186))
        };

        public AccentColor SelectedAccentColor { get; set; }

        public string Filter { get; set; }

        public IList<ViewType> ViewTypes { get; } = Enum.GetValues(typeof(ViewType)).Cast<ViewType>().ToList();
        public ViewType ViewType { get; set; } = ViewType.All;

        public ObservableCollection<Model.SoundBoardItem> FilteredItems { get; } = new ObservableCollection<Model.SoundBoardItem>();

        public static ICommand PlaySoundBoardItemCommand { get; } = new RoutedUICommand("PlaySoundBoardItem", "PlaySoundBoardItemCommand", typeof(MainWindow));
        public static ICommand ToggleFavoriteCommand { get; } = new RoutedUICommand("ToggleFavorites", "ToggleFavoriteCommand", typeof(MainWindow));
        public static ICommand ToggleLoggingCommand { get; } = new RoutedUICommand("ToggleLogging", "ToggleLoggingCommand", typeof(MainWindow));
        public bool ShowLogging { get; set; }
        public bool Connected { get; set; }
        public ObservableCollection<Model.SoundBoardItem> SoundBoardItems { get; set; }
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
                        var soundBoardItems = Mapper.Map<IList<SoundBoardItem>, IList<Model.SoundBoardItem>>(soundboarClient.GetSoundBoardItems().ToList());
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
                            {
                                SoundBoardItems = new ObservableCollection<Model.SoundBoardItem>(soundBoardItems);
                                FilteredItems.UpdateFrom(SoundBoardItems);
                            }
                            else
                                SoundBoardItems.UpdateFrom(soundBoardItems, (a, b) => a.Id == b.Id);

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
                await Task.Delay(TimeSpan.FromSeconds(2), _tokenSource.Token);
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

        private void OnSelectedAccentColorChanged()
        {
            _settings.PreferedColorSchema = SelectedAccentColor.Name;
            SettingsHelper.StoreSettings(_settings);
        }

        private void OnViewTypeChanged()
        {
            UpdateFilteredList();
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

        private void ToggleFavorite(object sender, ExecutedRoutedEventArgs e)
        {
            var id = (Guid) e.Parameter;
            var item = SoundBoardItems.FirstOrDefault(d => d.Id == id);
            if (item == null) return;
            item.IsFavorite = !item.IsFavorite;

            if (item.IsFavorite)
                _settings.Favorites.Add(id);
            else
            {
                _settings.Favorites.Remove(id);
                if (ViewType == ViewType.Favorites)
                    UpdateFilteredList();
            }
            SettingsHelper.StoreSettings(_settings);
        }

        private void ToggleLogging(object sender, ExecutedRoutedEventArgs e)
        {
            ShowLogging = !ShowLogging;
        }

        private void UpdateFilteredList()
        {
            RunOnGuiThread(() =>
            {
                if (SoundBoardItems == null)
                    FilteredItems.Clear();
                switch (ViewType)
                {
                    case ViewType.All:
                        FilteredItems.UpdateFrom(!string.IsNullOrEmpty(Filter) ? SoundBoardItems?.Where(d => d.Title.ToLower().Contains(Filter.ToLower()) || d.Category.ToLower().Contains(Filter.ToLower())).ToList() : SoundBoardItems?.ToList());
                        break;
                    case ViewType.Favorites:
                        FilteredItems.UpdateFrom(!string.IsNullOrEmpty(Filter) ? SoundBoardItems?.Where(d => d.Title.ToLower().Contains(Filter.ToLower()) || d.Category.ToLower().Contains(Filter.ToLower())).Where(d => d.IsFavorite).ToList() : SoundBoardItems?.Where(d => d.IsFavorite).ToList());
                        break;
                }
            });
        }

        #endregion

        #region Public methods

        public void OnFilterChanged()
        {
            UpdateFilteredList();
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == SingleInstance.WM_SHOWFIRSTINSTANCE)
            {
                Show();
                WindowState = WindowState.Normal;
            }
            return IntPtr.Zero;
        }

        #endregion
    }

    public enum ViewType
    {
        All,
        Favorites
    }
}