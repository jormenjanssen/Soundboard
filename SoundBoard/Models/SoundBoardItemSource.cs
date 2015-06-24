using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoard.Models
{
    class SoundBoardItemSource : ISoundBoardItemSource
    {
        private static SoundBoardItemSource _soundBoardItemSource;
        private readonly ConcurrentDictionary<string, SoundBoardItem> _soundBoardDictionary;

        public ConcurrentDictionary<string, SoundBoardItem> Dictionary
        {
            get { return _soundBoardDictionary; }
        }

        public ConcurrentDictionary<string,SoundBoardItem> Items
        {
            get
            {
                return _soundBoardDictionary;
            }
        }

        static SoundBoardItemSource()
        {
            _soundBoardItemSource = new SoundBoardItemSource();
        }

        private SoundBoardItemSource ()
	    {
            _soundBoardDictionary = new ConcurrentDictionary<string, SoundBoardItem>();
	    }

        public static ISoundBoardItemSource GetInstance()
        {
            return _soundBoardItemSource;
        }

        public SoundBoardItem TryGetById(Guid id)
        {
            var soundBoardItem = _soundBoardDictionary.FirstOrDefault(w => w.Value.Id == id);
            if (soundBoardItem.Value == null)
                return null;

            return soundBoardItem.Value;
        }

        public SoundBoardItem TryGetByFilename(string filename)
        {
            SoundBoardItem soundBoardItem = null;

            if(Items.TryGetValue(filename,out soundBoardItem))
            {
                if(soundBoardItem != null)
                    return soundBoardItem;

                return null;
            }

            return null;

        }

        
    }
}
