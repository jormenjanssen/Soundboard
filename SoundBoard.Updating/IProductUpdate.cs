using System;

namespace SoundBoard.Updating
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
