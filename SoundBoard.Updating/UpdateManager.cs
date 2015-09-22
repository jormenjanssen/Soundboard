using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using log4net;
using SoundBoard.Updating.Helpers;
using SoundBoard.Updating.Internal;
using SoundBoard.Updating.Internal.Streams;
using SoundBoard.Updating.Userinterface;

namespace SoundBoard.Updating
{
    public class UpdateManager : IDisposable
    {
        internal const string AutoUpdateUrl = "http://stream3:9000/Webportal/";

        #region Private Fields

        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly CancellationTokenSource _updateCancellationTokenSource;

        #endregion

        #region Constructor

        public UpdateManager(int maximumTimeout)
        {
            _updateCancellationTokenSource = new CancellationTokenSource();
            MaximumSearchTimeOut = maximumTimeout;
        }

        #endregion

        #region Public Properties

        public int MaximumSearchTimeOut { get; set; }

        #endregion

        #region Private Methods

        private IProductUpdate CheckForProductUpdate(Uri updateSource, int timeout)
        {
            var productUpdate = new ProductUpdate();

            try
            {
                var webRequest = WebRequest.Create(updateSource);

                // Skip proxy checking because it's slow.
                webRequest.Proxy = null;

                webRequest.Timeout = timeout * 1000;

                using (var webResponse = webRequest.GetResponse())
                using (var responseStream = webResponse.GetResponseStream())
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(responseStream);

                    var productElement = xmlDoc.DocumentElement;

                    var minimalVersion = productElement.Attributes.GetAttributeByKeyOrNull<Version>("minimal-version");
                    var latestVersion = productElement.Attributes.GetAttributeByKeyOrNull<Version>("latest-version");
                    var url = productElement.Attributes.GetAttributeByKeyOrNull<Uri>("url");
                    var command = productElement.Attributes.GetAttributeByKeyOrNull<string>("command");

                    // Validate the xml file.
                    if (latestVersion == null || url == null)
                        throw new XmlException("Invallid update schema");

                    productUpdate.MinimalVersion = minimalVersion;
                    productUpdate.Url = url;
                    productUpdate.Command = command;
                    productUpdate.NewVersion = latestVersion;

                }

            }

            catch (Exception ex)
            {
                _log.Error("Failed to retrieve update", ex);
            }

            return productUpdate;
        }

        private IProductUpdate DownloadProductUpdateFile(IProductUpdate productUpdate)
        {
            var myProductUpdate = productUpdate as ProductUpdate;

            if (myProductUpdate == null)
                throw new InvalidCastException("Invallid ProductUpdate class");

            try
            {
                var webRequest = WebRequest.Create(myProductUpdate.Url);
                webRequest.Timeout = MaximumSearchTimeOut * 1000;

                using (var webResponse = webRequest.GetResponse())
                using (var responseStream = webResponse.GetResponseStream())
                {
                    string extension = string.Empty;

                    var fileName = myProductUpdate.Url.Segments.LastOrDefault();
                    if (fileName != null && fileName.Split('.').Length > 0)
                        extension = fileName.Split('.').Last();

                    var tempDirectory = System.IO.Path.GetTempPath();
                    var productFile = string.Format("{0}.{1}", AssemblyHelper.GetCurrentAssemblyName(), extension);

                    var productFilePath = Path.Combine(tempDirectory, productFile);

                    // Make sure any old installation files are removed before trying to write a new one.
                    if (File.Exists(productFilePath))
                        File.Delete(productFilePath);

                    using (var streamCopier = new FileStreamCopier(responseStream, productFilePath))
                    {
                        var downloadCompleted = streamCopier.CopyStream(myProductUpdate, _updateCancellationTokenSource.Token,webResponse.ContentLength);                                  
                        myProductUpdate.DownloadCompleted = downloadCompleted;
                        
                    }

                    return myProductUpdate;

                }

            }

            catch (Exception ex)
            {
                _log.Error("Failed to retrieve update", ex);
            }

            return null;

        }

        #endregion

        #region Public Mehods

        /// <summary>
        /// Check for product updates async (Task based).
        /// </summary>
        /// <param name="showUpdateUserInterface">Show the check for updates interface.</param>
        /// <param name="timeout">The maximum timeout before the check for update mechanism is cancelled.</param>
        /// <returns>An IProductUpdate interface which contains info about the update.</returns>
        public Task<IProductUpdate> CheckForUpdatesAsync()
        {
            var productUpdateTask = new Task<IProductUpdate>(() =>
            {
                var updateSource = AutoUpdateHelper.GetProductUpdateSourceUri();
                var productUpdate = CheckForProductUpdate(updateSource, MaximumSearchTimeOut);

                return productUpdate;
            });

            productUpdateTask.Start();

            return productUpdateTask;
        }

        public Task<ProductUpdateResult> UpdateProduct(IProductUpdate productUpdate)
        {
            var downloadTask = new Task<IProductUpdate>(() =>
            {
                var productUpdateResult = DownloadProductUpdateFile(productUpdate);

                if (productUpdateResult == null)
                    throw new ProductUpdateFailedException(new Exception("Failed to retrieve download."));

                return productUpdate;

            });

            var fileExecutionTask = downloadTask.ContinueWith((pu) =>
            {
                if (pu.Exception != null)
                    return ProductUpdateResult.DownloadFailed;


                var productUpdateResult = pu.Result as ProductUpdate;

                if (productUpdateResult == null)
                    return ProductUpdateResult.DownloadFailed;

                productUpdateResult.Execute();
   

                return ProductUpdateResult.Updating;
            });

            downloadTask.Start();

            return fileExecutionTask;
        }

        public static Task ApplyUpdateIfAvailableAsync(UpdateManager updateManager)
        {
            var updateTask = updateManager.CheckForUpdatesAsync();
            return updateTask;
        }

        public static void ApplyUpdateIfAvailable(bool showUpdateUserinterface = true, int maximumSearchTimeout = 5)
        {
            using (var updateManager = new UpdateManager(maximumSearchTimeout))

            if (showUpdateUserinterface)
            {
                try
                {
                    var userInterfaceFactory = new UpdateUserInterfaceFactory(updateManager);
                    var updateInterfaceWindow = userInterfaceFactory.CreateUpdateWindow();
                    updateInterfaceWindow.ShowDialog();
                }

                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                
            }
            else
            {
                ApplyUpdateIfAvailableAsync(updateManager).Wait();
            }
        }
   
        public void Dispose()
        {
            _updateCancellationTokenSource.Dispose();
        }

        #endregion
    }
}
