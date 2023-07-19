using System;
using System.Net;
using Serilog;
using SoundMixer.Miscellanea;

using Stylet;

namespace SoundMixer.ViewModels
{
    class StreamViewModel : Screen
    {
        readonly private IEventAggregator eventAggregator;

        private string videoURL;
        public string VideoURL
        {
            get { return this.videoURL; }
            set { this.SetAndNotify(ref this.videoURL, value); }
        }

        public StreamViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        /// <summary>
        /// This method will check a url to see that it does not return server or protocol errors<br/>
        /// From <a href="https://stackoverflow.com/a/7180033/7179042"></a>
        /// </summary>
        /// <param name="url">The path to check</param>
        /// <returns></returns>
        public bool IsUrlValid(string url)
        {
            try
            {
                var request = HttpWebRequest.Create(url) as HttpWebRequest;
                request.Timeout = 5000; // Set the timeout to 5 seconds to keep the user from waiting too long for the page to load
                request.Method = "HEAD"; // Get only the header information -- no need to download any content

                using var response = request.GetResponse() as HttpWebResponse;
                int statusCode = (int)response.StatusCode;
                if (statusCode >= 100 && statusCode < 400) //Good requests
                {
                    return true;
                }
                else if (statusCode >= 500 && statusCode <= 510) //Server Errors
                {
                    Log.Warning(string.Format("The remote server has thrown an internal error. Url is not valid: {0}", url));
                    return false;
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError) //400 errors
                {
                    return false;
                }
                else
                {
                    Log.Warning(String.Format("Unhandled status [{0}] returned for url: {1}", ex.Status, url), ex);
                }
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Could not test url {0}.", url), ex);
            }
            return false;
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            // Check it's a valid URL
            if (IsUrlValid(VideoURL))
            {
                this.eventAggregator.Publish(new AddedSoundFromStream(new AddedSoundFromStream.SoundFromStreamArgs(VideoURL)));
            }
        }

    }
}
