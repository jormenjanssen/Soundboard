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
                await SoundBoard.Helpers.TaskExtensions.EventTask();
                while (_mediaQueue.HasMediaItemsQueued)
                {
                    SoundBoardItem soundBoardItem;

                    if (_mediaQueue.ItemQueue.TryDequeue(out soundBoardItem))
                    {
                        _synchronizationContext.Post((s) =>
                        {
                            _mp3Player.Close();
                            _mp3Player.MasterVolume = 1000;
                            _mp3Player.Open(soundBoardItem.File);
                            _mp3Player.Play(false);
                        }, false);

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
