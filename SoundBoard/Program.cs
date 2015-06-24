using Microsoft.Owin.Hosting;
using SoundBoard.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoard
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseAddress = (string) new System.Configuration.AppSettingsReader().GetValue("portalBinding", typeof(string));


            // Start OWIN host 
            using (WebApp.Start<PortalStartup>(url: baseAddress))
            {
                Console.WriteLine("{0} Soundboard portal server started @ {1}",DateTime.Now,baseAddress);
                Console.WriteLine("{0} Starting the directory scanner ",DateTime.Now);
                DirectoryScanner.StartScanning();
                    
                Console.ReadLine();
            }

            Console.ReadLine(); 
        }
    }
}
