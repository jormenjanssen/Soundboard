using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NHotkey;
using NHotkey.Wpf;
using SoundBoard.Wpf.Client;

namespace SoundBoard.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        string _serverAddress = ConfigurationManager.AppSettings["ServerAddress"].TrimEnd('/') + "/api";
        protected override void OnStartup(StartupEventArgs e)
        {
            //HotkeyManager.Current.AddOrReplace("EmergencyStop", Key.F11, ModifierKeys.Control, EmergyStop);

           
            base.OnStartup(e);
        }

        private void EmergyStop(object sender, HotkeyEventArgs e)
        {
            using (var soundboarClient = new SoundBoardClient(_serverAddress))
            {
                soundboarClient.EmergencyStopAsync();
            }
        }
    }
}
