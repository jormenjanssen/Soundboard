using System.IO;

namespace SoundBoard.Updating.Internal.Streams
{
    class FileStreamCopier : StreamCopier
    {
        private readonly string File;

        public FileStreamCopier(Stream sourceStream,string file) : base(sourceStream,new FileStream(file, FileMode.Create))
        {
            File = file;
        }
    }
}
