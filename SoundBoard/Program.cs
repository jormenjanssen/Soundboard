using Microsoft.Owin.Hosting;
using SoundBoard.Audio;
using SoundBoard.Helpers;
using SoundBoard.Synchronization;
using System;
using System.Threading;
using System.Windows.Forms;

namespace SoundBoard
{
    class Program
    {
        private readonly ISynchronizationWorker _synchronizationWorker;
        private static SynchronizationContext _synchronizationContext;

        public static SynchronizationContext SynchronizationContext
        {
            get { return _synchronizationContext; }
        }

        [STAThread]
        static void Main(string[] args)
        {
            var program = new Program();
            program.LaunchServer();
            
        }

        public Program()
        {
           _synchronizationWorker = SynchronizationWorker.Create();
           _synchronizationContext = _synchronizationWorker.SynchronizationContext;
        }

        public void LaunchServer()
        {
            string baseAddress = (string)new System.Configuration.AppSettingsReader().GetValue("portalBinding", typeof(string));

            // Start OWIN host 
            using (WebApp.Start<PortalStartup>(baseAddress))
            {
                Console.WriteLine("{0} Soundboard portal server started @ {1}", DateTime.Now, baseAddress);
                Console.WriteLine("{0} Starting the directory scanner ", DateTime.Now);
                DirectoryScanner.StartScanning();
                MediaQueueListener.StartListening();

                _synchronizationWorker.Wait();

            }
        }
    }
}
