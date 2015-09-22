using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoard.AutoUpdate.Internal.Streams
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
