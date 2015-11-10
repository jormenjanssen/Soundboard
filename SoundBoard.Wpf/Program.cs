using System;
using System.Windows;
using SoundBoard.Wpf.Utility;

namespace SoundBoard.Wpf
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            if (!SingleInstance.Start())
            {
                SingleInstance.ShowFirstInstance();
                return;
            }
            try
            {
                App.Main();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            SingleInstance.Stop();
        }
    }
}
