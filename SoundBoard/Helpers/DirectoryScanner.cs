using SoundBoard.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoard.Helpers
{
    public class DirectoryScanner
    {
        private readonly static DirectoryScanner _directoryScanner;

        public static string ScanDirectory
        {
            get
            {
                string baseAddress = (string)new System.Configuration.AppSettingsReader().GetValue("SoundsDirectory", typeof(string));

                if(!Directory.Exists(baseAddress))
                    throw new DirectoryNotFoundException(string.Format("directory {0} does not exsist!",baseAddress));

                return baseAddress;
            }
        }

        public static IEnumerable<string> MediaFilter
        {
            get
            {
                string mediaFilter = (string)new System.Configuration.AppSettingsReader().GetValue("MediaFilter", typeof(string));
                return mediaFilter.Split(';');
            }
        }

        static DirectoryScanner()
        {
            _directoryScanner = new DirectoryScanner();
        }

        private DirectoryScanner()
        {

        }

        private async void ScanContinues()
        {
            while (true)
            {
                await ScanTask();
                await Task.Delay(new TimeSpan(0, 0, 15));
            }

        }

        public static void StartScanning()
        {
            _directoryScanner.ScanContinues();
        }

        private Task ScanTask()
        {
            var scanTask = new Task(() => 
            {
                string scanDirectory = string.Empty;
                IEnumerable<string> mediaFilter;

                try
                {
                    scanDirectory = ScanDirectory;
                    mediaFilter = MediaFilter;
                }
                catch(Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} Directory Scanner task failed!", DateTime.Now);
                    Console.WriteLine("{0} Task message: {1}",DateTime.Now,ex.Message);
                    Console.WriteLine("{0} Task stacktrace: {1}",DateTime.Now,ex.StackTrace);
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }

                Console.WriteLine("{0} Directory Scanner task started",DateTime.Now, scanDirectory);
                Scan(scanDirectory, MediaFilter);

            });

            scanTask.Start();

            return scanTask;
        }

        private void Scan(string directory,IEnumerable<string> filter)
        {
            int newMediaItems = 0;
            int mediaItemsFound = 0;
            int mediaItemUpdated = 0;
            int fileItemsSkipped = 0;

            var soundBoardItemSource = SoundBoardItemSource.GetInstance();

            
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("{0} Started media scan with filter: [{1}]", DateTime.Now, GetFormatterFilterString(filter));
            
            
            var beginTime = DateTime.Now;

            foreach(var file in System.IO.Directory.GetFiles(directory,"*.*",SearchOption.AllDirectories))
            {
                if (filter.Any(a => file.EndsWith(a)))
                {
                    SoundBoardItem soundBoardItem = null;
                    if(soundBoardItemSource.Items.TryGetValue(file,out soundBoardItem))
                    {

                    }
                    else
                    {
                        soundBoardItem = new SoundBoardItem
                        {
                            Id = Guid.NewGuid(),
                            SoundboardLogo = SoundBoardLogo.Default,
                            Title = file

                        };

                        if (soundBoardItemSource.Items.TryAdd(file,soundBoardItem))
                            Console.WriteLine("{0} Added item {1} to media libary",DateTime.Now,soundBoardItem.Title);
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("{0} Failed to add item {1} to media libary!");
                            Console.ForegroundColor = ConsoleColor.Green;
                        }

                        newMediaItems++;
                    }
                    mediaItemsFound ++;
                }

            }

            foreach(var item in soundBoardItemSource.Items)
            {
                if(!File.Exists(item.Key))
                {
                    SoundBoardItem soundBoardItem = null;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("{0} Removing item {1} from media libary because it does not longer exsist on disk",DateTime.Now,item);

                    if (!soundBoardItemSource.Items.TryRemove(item.Key, out soundBoardItem))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("{0} Failed to remove item {1} from media libary!");
                        Console.ForegroundColor = ConsoleColor.Green;
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                }
            }

            var endTime = DateTime.Now;

            Console.WriteLine("{0} Items Found: [{1}] Items updated: [{2}] Items skipped: [{3}]", DateTime.Now,mediaItemsFound,mediaItemUpdated,fileItemsSkipped);
            Console.WriteLine("{0} Finished media scan in [{1}]", DateTime.Now, endTime - beginTime);

            Console.ForegroundColor = ConsoleColor.White;

            
        }
        
        private string GetFormatterFilterString(IEnumerable<string> filter)
        {
            string myFilter = string.Empty;

            foreach (var filterString in filter)
            {
                myFilter = myFilter + filterString + ", ";
            }

            myFilter = myFilter.Remove(myFilter.Length - 2, 2);

            return myFilter;
        }
    }
}
