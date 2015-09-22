using System;

namespace SoundBoard.Updating.ViewModels
{
    public interface IViewModelInteraction
    {
        Func<bool?> ShowWindow { set; }

        Action CloseWindow { get; set; }

        Func<bool> ShutdownOnClose { get; }

        void OnWindowLoaded();
    }
}
