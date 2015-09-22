using System;
using System.Diagnostics;
using System.Linq;
using SoundBoard.AutoUpdate;
using SoundBoard.Updating.Helpers;
using SoundBoard.Updating.Internal.Streams;

namespace SoundBoard.Updating.Internal
{
    internal class ProductUpdate : IProductUpdate, IProgress<StreamProgressResult>
    {
        #region Private Fields

        private bool _isVallidProductUpdate;
        private bool _hasUpdate;
        private bool _isMandatory;

        private Version _newVersion;

        #endregion

        #region Public Properties

        public bool IsManadatory
        {
            get { return _isMandatory; }
        }

        public bool HasUpdate
        {
            get { return _hasUpdate; }
            set { _hasUpdate = true; }
        }

        public Version OldVersion
        {
            get { return AssemblyHelper.GetCurrentVersion(); }
        }

        public Version MinimalVersion { get; set; }

        public Version NewVersion
        {
            get
            {
                if (_newVersion == null)
                    return OldVersion;

                return _newVersion;
            }
            set
            {
                _isVallidProductUpdate = true;

                if (value > OldVersion)
                {
                    _newVersion = value;
                    _hasUpdate = true;

                    if (MinimalVersion > OldVersion)
                        _isMandatory = true;

                }

            }
        }

        public string Command { get; set; }

        public Uri Url { get; set; }

        public bool IsVallidProductUpdate
        {
            get { return _isVallidProductUpdate; }
        }

        public int Progress { get; set; }

        public long BytesPerSec { get; set; }

        public bool DownloadCompleted { get; set; }

        #endregion

        #region Public Methods

        public void Report(StreamProgressResult value)
        {
            Progress = value.Progress;
            BytesPerSec = value.BytesPerSec;

        }

        public void Execute()
        {
            if (string.IsNullOrWhiteSpace(Command))
            {
                var process = Process.Start(Command);
                process.Dispose();
                Environment.Exit(0);
            }

            var processStartInfo = new System.Diagnostics.ProcessStartInfo();

            if (Command.Contains("%MsiFile%"))
            {
                var tempDirectory = System.IO.Path.GetTempPath();
                var productFile = string.Format("{0}.{1}", AssemblyHelper.GetCurrentAssemblyName(), "msi");

                var productFilePath = System.IO.Path.Combine(tempDirectory, productFile);

                Command = Command.Replace("%MsiFile%", string.Format("\"{0}\"", productFilePath));
            }

            if (Command.Contains(' '))
            {
                processStartInfo.FileName = Command.Split(' ')[0];
                processStartInfo.Arguments = Command.Split(' ')
                                                    .Skip(1)
                                                    .Aggregate((a, b) =>
                                                    {
                                                        return a + " " + b;
                                                    });
            }
            else
            {
                processStartInfo.FileName = Command.Split(' ')[0];
            }

            Process.Start(processStartInfo);
            Environment.Exit(0);
        }


        #endregion
    }
}
