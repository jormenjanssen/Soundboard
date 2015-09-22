namespace SoundBoard.Updating.Internal.Streams
{
    public class StreamProgressResult
    {
        private readonly int _progress;
        private readonly long _bytesPerSec;

        public int Progress
        {
            get { return _progress; }
        }

        public long BytesPerSec
        {
            get
            {
                return _bytesPerSec;
            }
        }

        public StreamProgressResult(int progress, long bytesPerSec)
        {
            _progress = progress;
            _bytesPerSec = bytesPerSec;
        }


    }
}
