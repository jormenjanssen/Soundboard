using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundBoard.AutoUpdate.ViewModels
{
    public interface IViewModelInteraction
    {
        Func<bool?> ShowWindow { set; }

        Action CloseWindow { get; set; }

        Func<bool> ShutdownOnClose { get; }

        void OnWindowLoaded();
    }
}
