using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoard.AutoUpdate
{
    public enum ProductUpdateResult
    {
        DownloadFailed,
        ParseFailed,
        ReadyToUpdate,
        Updating
    }
}
