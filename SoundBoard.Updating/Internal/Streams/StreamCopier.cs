using System;
using System.IO;
using System.Threading;

namespace SoundBoard.Updating.Internal.Streams
{
    public class StreamCopier : IDisposable
    {
        public const int ReadBufferSize = 4096;
        protected int WriteBufferSize = 32768;
        
        private readonly Stream _targetStream;
        private readonly Stream _sourceStream;

        private Stream _bufferStream;

        public bool CopyCompleted { get; private set; }

        public StreamCopier(Stream sourceStream, Stream targetStream)
        {
            _targetStream = targetStream;
            _sourceStream = sourceStream;
        }

        public bool CopyStream(IProgress<StreamProgressResult> streamProgressResult,CancellationToken cancellationToken, long expectedSize = -1)
        {

            if (_sourceStream == null || _targetStream == null)
                throw new NullReferenceException("Source or Target Null!");

            _bufferStream = new MemoryStream();
            var offset = 0;
            var bytesToRead = ReadBufferSize;
            long downloaded = 0;

            while(!cancellationToken.IsCancellationRequested)
            {
                var buffer = new byte[ReadBufferSize];
                var numBytesToRead = _sourceStream.Read(buffer, offset, bytesToRead);

                var begin = DateTime.UtcNow;

                while (numBytesToRead > 0)
                {
                    _bufferStream.Write(buffer, offset, numBytesToRead);
                    
                    if (_bufferStream.Position > WriteBufferSize)
                    {
                        var ts = DateTime.UtcNow - begin;
                        int seconds = ts.Seconds;

                        // Prevent divide by zero.
                        if (downloaded == 0)
                            downloaded = 1;
                        
                        // Prevent divide by zero.
                        if(seconds == 0)
                            seconds = 1;

                        var kbytesPerSec = downloaded / seconds;

                        streamProgressResult.Report(new StreamProgressResult(CalculateProgress(downloaded, expectedSize),kbytesPerSec));
                   
                    }

                    downloaded += numBytesToRead;
                    numBytesToRead = _sourceStream.Read(buffer, offset, bytesToRead);
                }

                _bufferStream.Position = 0;

                numBytesToRead = _bufferStream.Read(buffer, 0, ReadBufferSize);

                // Write the remaining bytes.
                while (numBytesToRead > 0)
                {
                    _targetStream.Write(buffer, 0, ReadBufferSize);
                    numBytesToRead = _bufferStream.Read(buffer, 0, ReadBufferSize);
                }

                break;
            }

            return true;
        }

        private int CalculateProgress(long downloaded,long expectedSize)
        {
            if (expectedSize == -1)
                return 0;

            var progress = (double)downloaded / (double)expectedSize;

            return Convert.ToInt32(progress * 100);
        }

        public void Dispose()
        {
            if (_sourceStream != null)
                _sourceStream.Dispose();

            if (_targetStream != null)
                _targetStream.Dispose();
        }
    }
}
