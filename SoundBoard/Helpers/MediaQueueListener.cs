using SoundBoard.Audio;
using SoundBoard.Data;
using SoundBoard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoundBoard.Helpers
{
    class MediaQueueListener : ISoundBoardQueue
    {
        #region Private fields

        private Player _mp3Player;
        private MediaQueue _mediaQueue;
        private static MediaQueueListener _mediaQueueListener;

        private SynchronizationContext _synchronizationContext;

        #endregion

        #region Public properties

        public static bool EmergencyFlag { get; set; }

        public IEnumerable<SoundBoardItem> SoundboardQueue
        {
            get
            {
                return _mediaQueue.ItemQueue.AsEnumerable();
            }
        }

        public MediaQueue MediaQueue
        {
            get
            {
                return _mediaQueue;
            }
        }

        #endregion

        #region Constructor

        private MediaQueueListener()
        {
            _synchronizationContext = Program.SynchronizationContext;
            _mediaQueue = new MediaQueue();
            _mp3Player = new Player();
            QueueLoop();
        }

        static MediaQueueListener()
        {
            _mediaQueueListener = new MediaQueueListener();
        }

        #endregion

        #region Public methods

        public static void StartListening()
        {
            // nop
        }
        
        public static ISoundBoardQueue GetQueue()
        {
            return _mediaQueueListener;
        }

        public void Enqueue(SoundBoardItem soundBoardItem)
        {
            _mediaQueue.ItemQueue.Enqueue(soundBoardItem);
            TaskExtensions.Signal();
        }

        public void TryInvallidateQueuedItem()
        {
            throw new NotImplementedException();
        }

        private async void QueueLoop()
        {
            while (true)
            {
                await TaskExtensions.EventTask();
                while (_mediaQueue.HasMediaItemsQueued)
                {
                    SoundBoardItem soundBoardItem;

                    var isPlaying = false;
                    isPlaying = _mp3Player.IsPlaying();
                    

                    if (!isPlaying)
                    {
                        if (_mediaQueue.ItemQueue.TryPeek(out soundBoardItem))
                        {
                            _synchronizationContext.Post((s) =>
                            {
                                Console.WriteLine("mp3 status : {0}", _mp3Player.Status());
                                _mp3Player.Close();
                                _mp3Player.MasterVolume = 1000;
                                _mp3Player.Open(soundBoardItem.File);
                                _mp3Player.Play(false);
                                Console.WriteLine("mp3 status : {0}", _mp3Player.Status());
                                while (_mp3Player.IsPlaying())
                                {
                                    if (EmergencyFlag)
                                        _mp3Player.Close();

                                    while (EmergencyFlag)
                                        Thread.Sleep(100);

                                    Thread.Sleep(TimeSpan.FromMilliseconds(200));
                                }
                                _mediaQueue.ItemQueue.TryDequeue(out soundBoardItem);
                               
                            }, false);
                            
                        }
                    }
                    await Task.Delay(750);
                }
            }
        }

        #endregion

        #region Private methods



        #endregion



    }
}
