namespace SoundBoard.Wpf.Client
{
    #region Namespaces

    using System;
    using System.Collections.Generic;
    using SoundBoard.Data;

    #endregion

    public class SoundBoardClient : ApiClientBase
    {
        #region Constructor

        public SoundBoardClient()
            : base("http://localhost:9000/api/")
        {
            //just for compatibility
        }

        public SoundBoardClient(string baseUrl)
            : base(baseUrl)
        {
        }

        #endregion

        #region Public methods

        public IEnumerable<SoundBoardItem> GetSoundBoardItems()
        {
            return Execute<IEnumerable<SoundBoardItem>>("Sound");
        }

        public SoundBoardItem AddToQueue(Guid id)
        {
            return Execute<SoundBoardItem>("Queue?id=" + Uri.EscapeDataString(id.ToString()));
        }

        #endregion
    }
}