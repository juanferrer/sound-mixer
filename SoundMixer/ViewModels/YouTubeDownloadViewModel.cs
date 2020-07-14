using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using SoundMixer.Miscellanea;

using Stylet;

namespace SoundMixer.ViewModels
{
    class YouTubeDownloadViewModel : Screen
    {
        readonly private IEventAggregator eventAggregator;

        private string tempPath;
        private string tempURL;

        private string videoURL;
        public string VideoURL
        {
            get { return this.videoURL; }
            set { this.SetAndNotify(ref this.videoURL, value); }
        }

        private string downloadPath;
        public string DownloadPath
        {
            get { return this.downloadPath; }
            set { this.SetAndNotify(ref this.downloadPath, value); }
        }

        public YouTubeDownloadViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public string DownloadFromYouTube(string url, string path)
        {
            string downloadPath;

            if (string.IsNullOrEmpty(path))
            {
                // Save to temp, we'll move it later
                downloadPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }
            else
            {
                // We already have a download path provided
                downloadPath = path;
            }

            string targetFormat = Path.GetExtension(downloadPath).Replace(".", "");
            string sourceFormat = "mp4";

            using (Process downloadProcess = new Process())
            {
                downloadProcess.StartInfo.UseShellExecute = false;
                downloadProcess.StartInfo.FileName = "youtube-dl.exe";
                downloadProcess.StartInfo.Arguments = $"-x --audio-format \"{targetFormat}\" -o \"{downloadPath.Replace(targetFormat, sourceFormat)}\" {url}";
                downloadProcess.StartInfo.CreateNoWindow = true;
                downloadProcess.Start();
            }

            // The downloaded file also got converted to mp3, so we need to fix the extension
            string convertedPath = Path.Combine(Path.GetDirectoryName(downloadPath), Path.GetFileNameWithoutExtension(downloadPath)) + "." + targetFormat;

            return convertedPath;
        }

        public void VideoURL_TextChanged(object sender, RoutedEventArgs e)
        {
            string text = (sender as TextBox).Text;
            BackgroundWorker bg = new BackgroundWorker();
            Process downloadProcess = new Process();
            bg.DoWork += (s, e) =>
            {
                string[] args = (string[])e.Argument;
                downloadProcess.StartInfo.UseShellExecute = false;
                downloadProcess.StartInfo.FileName = "youtube-dl.exe";
                downloadProcess.StartInfo.Arguments = $"-j {args[0]}";
                downloadProcess.StartInfo.CreateNoWindow = true;
                downloadProcess.Start();
                downloadProcess.WaitForExit();

                // Put the arguments in the result. It's not actually the result of the operation
                // but if they're not in the result, they're inaccessible in RunWorkerCompleted
                e.Result = args;
            };

            bg.RunWorkerCompleted += (s, e) =>
            {
                string[] args = (string[])e.Result;
                if (downloadProcess.ExitCode == 0)
                {
                    // Start downloading and keep track of the temp file, we'll move it later
                    tempPath = DownloadFromYouTube(args[0], args[1]);
                    tempURL = args[0];
                }
            };

            string[] parameters = new string[] { text, DownloadPath };
            bg.RunWorkerAsync(parameters);
        }

        public void DownloadButton_Clicked(object sender, RoutedEventArgs e)
        {
            // The file may take a time to download, so we do this in another thread
            BackgroundWorker bg = new BackgroundWorker();
            // We should have it downloaded already
            bg.DoWork += (s, e) =>
            {
                for (int i = 0; i < 5; ++i)
                {
                    try
                    {
                        if (VideoURL == tempURL)
                        {
                            // Great, we downloaded it earlier, just move it to the correct location now
                            File.Move(tempPath, DownloadPath);
                        }
                        else
                        {
                            // It must have changed and we missed it. Delete temp and download again
                            File.Delete(tempPath);
                            DownloadFromYouTube(VideoURL, DownloadPath);
                        }
                        break;
                    }
                    catch (Exception)
                    {
                        // Will wait a bit more
                        System.Threading.Thread.Sleep(1000);
                    }
                }
            };



            bg.RunWorkerCompleted += (s, e) =>
            {
                // Raise an event with info on the file that just got added
                this.eventAggregator.Publish(new AddedSoundFromYouTube(new AddedSoundFromYouTube.SoundFromYouTubeArgs(DownloadPath)));
            };

            // Before we start it, show a moving dialog on an overlay
            this.eventAggregator.Publish(new AddingSoundFromYouTube());
            bg.RunWorkerAsync();

            this.Close();
        }

        public void Close()
        {
            this.RequestClose(true);
        }

        public void CancelButton_Clicked(object sender, RoutedEventArgs e)
        {
            // Delete the temp file we created earlier if it's still there
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            this.Close();
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            // Delete the temp file we created earlier if it's still there
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}
