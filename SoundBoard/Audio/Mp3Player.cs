using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SoundBoard.Audio
{
    class Player
    {
        private string Pcommand;
        private bool isOpen;

        [DllImport("winmm.dll")]
        private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, int bla);

        /// <summary>
        /// Not much to conctruct here
        /// </summary>
        public Player()
        {
        }

        /// <summary>
        /// Stops currently playing audio file
        /// </summary>
        public void Close()
        {
            Pcommand = "close MediaFile";
            mciSendString(Pcommand, null, 0, 0);
            isOpen = false;
        }

        /// <summary>
        /// Opens audio file to play
        /// </summary>
        /// <param name="sFileName">This is the audio file's path and filename</param>
        public void Open(string sFileName)
        {
            Pcommand = "open \"" + sFileName + "\" type mpegvideo alias MediaFile";
            mciSendString(Pcommand, null, 0, 0);
            isOpen = true;
        }

        /// <summary>
        /// Plays selected audio file
        /// </summary>
        /// <param name="loop">If True,audio file will repeat</param>
        public void Play(bool loop)
        {
            if (isOpen)
            {
                Pcommand = "play MediaFile";
                if (loop)
                    Pcommand += " REPEAT";
                mciSendString(Pcommand, null, 0, 0);
            }
        }

        /// <summary>
        /// Pauses currently playing audio file
        /// </summary>
        public void Pause()
        {
            Pcommand = "pause MediaFile";
            mciSendString(Pcommand, null, 0, 0);
        }

        /// <summary>
        /// Returns the current status player: playing,paused,stopped etc.
        /// </summary>
        public string Status()
        {
            int i = 128;
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(i);
            mciSendString("status MediaFile mode", stringBuilder, i, 1);
            return stringBuilder.ToString();
        }


        public bool IsPlaying()
        {
            return Status().ToLower() == "playing";
        }
        /// <summary>
        /// Get/Set Lelf Volume Factor
        /// </summary>
        public int LeftVolume
        {
            get
            {
                return 0; //Guess could be used to return Volume level: I don't need it
            }
            set
            {
                mciSendString(string.Concat("setaudio MediaFile left volume to ", value), null, 0, 0);
            }
        }

        /// <summary>
        /// Get/Set Right Volume Factor
        /// </summary>
        public int RightVolume
        {
            get
            {
                return 0; //Guess could be used to return Volume level: I don't need it
            }
            set
            {
                mciSendString(string.Concat("setaudio MediaFile right volume to ", value), null, 0, 0);
            }
        }

        /// <summary>
        /// Get/Set Main Volume Factor
        /// </summary>
        public int MasterVolume
        {
            get
            {
                return 0; //Guess could be used to return Volume level: I don't need it
            }
            set
            {
                mciSendString(string.Concat("setaudio MediaFile volume to ", value), null, 0, 0);
            }
        }

        /// <summary>
        /// Get/Set Bass Volume Factor
        /// </summary>
        public int Bass
        {
            get
            {
                return 0;
            }
            set
            {
                mciSendString(string.Concat("setaudio MediaFile bass to ", value), null, 0, 0);
            }
        }

        /// <summary>
        /// Get/Set Treble Volume Factor
        /// </summary>
        public int Treble
        {
            get
            {
                return 0;
            }
            set
            {
                mciSendString(string.Concat("setaudio MediaFile treble to ", value), null, 0, 0);
            }
        }
    }
}
