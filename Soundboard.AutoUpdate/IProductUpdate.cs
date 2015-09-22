using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoard.AutoUpdate
{
    public interface IProductUpdate
    {
        bool IsManadatory { get; }

        bool HasUpdate { get; }

        Version OldVersion { get; }

        Version NewVersion { get; }

        int Progress { get; }

        long BytesPerSec {get;}

     

    }
}
