using Microsoft.Owin.Hosting;
using SoundBoard.Audio;
using SoundBoard.Helpers;
using System;
using System.Threading;
using System.Windows.Forms;

namespace SoundBoard
{
    class Program
    {
        public static Action musicAction;

        [STAThread]
        static void Main(string[] args)
        {

            string baseAddress = (string)new System.Configuration.AppSettingsReader().GetValue("portalBinding", typeof(string));


            // Start OWIN host 
            using (WebApp.Start<PortalStartup>(baseAddress))
            {
                Console.WriteLine("{0} Soundboard portal server started @ {1}", DateTime.Now, baseAddress);
                Console.WriteLine("{0} Starting the directory scanner ", DateTime.Now);
                DirectoryScanner.StartScanning();
                MediaQueueListener.StartListening();

                while(true)
                {
                    if (musicAction != null)
                        musicAction();

                    Thread.Sleep(500);
                }

            }

            Console.ReadLine(); 
        }
    }
}
