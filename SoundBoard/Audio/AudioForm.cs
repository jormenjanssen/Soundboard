using SoundBoard.Data;
using SoundBoard.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoundBoard.Audio
{
    public partial class AudioForm : Form
    {
        private readonly MediaQueue _mediaQueue;
        private readonly Player _mp3Player;

        private SynchronizationContext FormSynchronizationContext;

        public AudioForm(MediaQueue mediaQueue)
        {
            InitializeComponent();
            _mediaQueue = mediaQueue;
            _mp3Player = new Player();
            FormSynchronizationContext = SynchronizationContext.Current;
            this.Opacity = 0;
        }

        private void AudioForm_Load(object sender, EventArgs e)
        {
           
        }

        

        private void TestTimer_Tick(object sender, EventArgs e)
        {
            TestTimer.Stop();
            //_mp3Player.MasterVolume = 100;
            //_mp3Player.LeftVolume = 100;
            //_mp3Player.RightVolume = 100;
            //_mp3Player.Treble = 50;
            //_mp3Player.Bass = 50;
            //_mp3Player.Open(@"C:\soundboard\Media\RobGeus.mp3");
            //_mp3Player.Play(true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _mp3Player.Open(@"C:\soundboard\Media\RobGeus.mp3");
            _mp3Player.Play(false);
        }
    }
}
