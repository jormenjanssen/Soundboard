using SoundBoard.Wpf.Utility;

namespace SoundBoard.Wpf.Client
{
    #region Namespaces

    using System;
    using System.Net;
    using Newtonsoft.Json;

    #endregion

    public abstract class ApiClientBase : IDisposable
    {
        #region Private fields

        private readonly string _baseUrl;
        private Lazy<WebClient> _lazyClient;

        #endregion

        #region Constructor

        protected ApiClientBase(string baseUrl)
        {
            _baseUrl = baseUrl.Trim('/');
            _lazyClient = new Lazy<WebClient>(() => new WebClient());
        }

        #endregion

        #region  Private helper functions

        protected WebClient Client()
        {
            if (_lazyClient == null)
            {
                throw new ObjectDisposedException("WebClient has been disposed");
            }
            //Add user info
            _lazyClient.Value.Headers.Add("username", SettingsHelper.GetSettings().Username);
            _lazyClient.Value.Headers.Add("version", typeof(ApiClientBase).Assembly.GetName().Version.ToString());
            return _lazyClient.Value;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_lazyClient != null)
            {
                if (disposing)
                {
                    if (_lazyClient.IsValueCreated)
                    {
                        _lazyClient.Value.Dispose();
                        _lazyClient = null;
                    }
                }

                // There are no unmanaged resources to release, but
                // if we add them, they need to be released here.
            }
        }

        protected T Execute<T>(string urlSegment)
        {
            using (var client = Client())
            {
                return JsonConvert.DeserializeObject<T>(client.DownloadString(_baseUrl + '/' + urlSegment.TrimStart('/')));
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        #endregion

        ~ApiClientBase()
        {
            Dispose(false);
        }
    }
}