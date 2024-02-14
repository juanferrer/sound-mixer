using System;
using System.IO;
using System.Windows.Controls;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using SoundMixer.Models;
using Unosquare.FFME.Common;
using NYoutubeDL;
using NYoutubeDL.Models;
using System.Linq;
using System.ComponentModel;
using Serilog;

namespace SoundMixer.UserControls
{
    /// <summary>
    /// Interaction logic for SoundControl.xaml
    /// </summary>
    public partial class SoundControl : UserControl
    {
        // Custom event
        public static readonly RoutedEvent SoloMuteClickEvent = EventManager.RegisterRoutedEvent("SoloMuteClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SoundControl));

        public event RoutedEventHandler SoloMuteClick
        {
            add { AddHandler(SoloMuteClickEvent, value); }
            remove { RemoveHandler(SoloMuteClickEvent, value); }
        }

        readonly private static Color missingSoundColor = Color.FromRgb(255, 0, 0);

        //readonly private string resourcesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources");

        readonly private Random rng;
        private CancellationTokenSource delayedPlayCancellationTokenSource;
        private CancellationTokenSource loadSoundCancellationTokenSource;
        private bool isPlayQueried;

        public SoundPropertyModel SoundPropertyModel
        {
            get { return (SoundPropertyModel)GetValue(SoundPropertyModelProperty); }
            set { SetValue(SoundPropertyModelProperty, value); }
        }

        public bool IsPlaying
        {
            get { return (bool)GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        public DirectSoundDeviceInfo OutputDevice
        {
            get { return (DirectSoundDeviceInfo)GetValue(OutputDeviceProperty); }
            set
            {
                SetValue(OutputDeviceProperty, value);
            }
        }

        public static readonly DependencyProperty SoundPropertyModelProperty = DependencyProperty.Register("SoundPropertyModel", typeof(SoundPropertyModel), typeof(SoundControl), null);
        public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register("IsPlaying", typeof(bool), typeof(SoundControl), null);
        public static readonly DependencyProperty OutputDeviceProperty = DependencyProperty.Register("OutputDevice", typeof(DirectSoundDeviceInfo), typeof(SoundControl), null);

        public SoundControl()
        {
            InitializeComponent();

            rng = new Random();
            //player.RendererOptions.UseLegacyAudioOut = true;
            Log.Information("Created SoundControl");
        }

        private void UpdatePlayer()
        {
            /*if (player.Volume != volumeSlider.Value)
            {
                double originalVolume = player.Volume;
                player.Volume = volumeSlider.Value;
                Log.Information("Volume for SoundControl {0} changed: {1}->{2}.", SoundPropertyModel.Name, originalVolume, player.Volume);
                Log.Information("UpdatePlayer() is actually needed.");
            }*/
        }

        public async Task Play()
        {
            if (!player.IsOpen)
            {
                isPlayQueried = true;
                Log.Information("Querying play of {0}.", SoundPropertyModel.Name);
                return;
            }
            UpdatePlayer();
            //player.Position = TimeSpan.Zero;
            await player.Seek(TimeSpan.Zero);
            await player.Play();
            IsPlaying = true;
            Log.Information("Playing {0}.", SoundPropertyModel.Name);
        }

        public Task DelayedPlay()
        {
            UpdatePlayer();

            if (SoundPropertyModel.UseRandomDelay)
            {
                // We need to set a new delay for this loop
                SoundPropertyModel.DelayTime = rng.Next(SoundPropertyModel.MinDelay, SoundPropertyModel.MaxDelay);
            }

            int delayTime = SoundPropertyModel.DelayTime;
            IsPlaying = true;

            // Create a new cancellation token source if the one we have is used (or we don't have one)
            if (delayedPlayCancellationTokenSource?.IsCancellationRequested ?? true)
            {
                delayedPlayCancellationTokenSource = new CancellationTokenSource();
            }

            Log.Information("Async playing {0}.", SoundPropertyModel.Name);

            return new Task(async () =>
            {
                await Task.Delay(delayTime);
                if (delayedPlayCancellationTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }

                _ = Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    _ = Play();
                }));
            }, delayedPlayCancellationTokenSource.Token);
        }

        public void Stop()
        {
            UpdatePlayer();

            // Cancel thread that keeps sound playing
            if (SoundPropertyModel.IsDelayed)
            {
                delayedPlayCancellationTokenSource?.Cancel();
                Log.Information("Cancelling delayed play of {0}.", SoundPropertyModel.Name);
            }

            // Stop sound if preparing to play or already playing
            if (isPlayQueried)
            {
                isPlayQueried = false;
                Log.Information("Cancelling queried play of {0}.", SoundPropertyModel.Name);
            }
            else if (IsPlaying)
            {
                _ = player.Stop();
                Log.Information("Stopping {0}.", SoundPropertyModel.Name);
            }
            IsPlaying = false;
        }

        public async Task PlayOrStop()
        {
            if (IsPlaying)
            {
                Stop();
            }
            else
            {
                // First, does it exist?
                if (!SoundPropertyModel.Sound.IsURL && !File.Exists(SoundPropertyModel.Sound.FilePath))
                {
                    // Add a red border
                    playButton.BorderBrush = new SolidColorBrush(missingSoundColor);
                    return;
                }

                if (SoundPropertyModel.IsDelayed)
                {
                    DelayedPlay().RunSynchronously();
                }
                else
                {
                    await Play();
                }
            }
        }

        public void SetPlayerMute(bool state)
        {
            player.IsMuted = state;
            Log.Information("{0} {1}.", state ? "Muted" : "Unmuted", SoundPropertyModel.Name);
        }

        public void SetOutputDevice(DirectSoundDeviceInfo outputDevice)
        {
            player.RendererOptions.DirectSoundDevice = outputDevice;
            Log.Information("Set output device for {0} to {1}.", SoundPropertyModel.Name, outputDevice.Name);
        }

        #region Event Handlers

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            await PlayOrStop();
        }

        private async void SoundMediaElement_MediaEnded(object sender, EventArgs e)
        {
            // The sound may have to loop
            if (SoundPropertyModel.IsLoop)
            {
                if (SoundPropertyModel.IsDelayed)
                {
                    DelayedPlay().RunSynchronously();
                }
                else
                {
                    await Play();
                }
            }
            else
            {
                _ = player.Stop();
                IsPlaying = false;
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.Volume = volumeSlider.Value;
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        private async void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            SoundPropertyModel.IsMuted = (sender as System.Windows.Controls.Primitives.ToggleButton).IsChecked == true;

            // If it's not muted anymore, we need to load the sound asynchronously
            if (!SoundPropertyModel.IsMuted && !player.IsOpen)
            {
                await player.Open(new Uri(SoundPropertyModel.Sound.FilePath));
            }

            var newEventArgs = new RoutedEventArgs(SoloMuteClickEvent);
            RaiseEvent(newEventArgs);
        }

        private void SoloButton_Click(object sender, RoutedEventArgs e)
        {
            // Sets this sound as Solo and raises the event for RootViewModel to control what happens to the other sound controls
            SoundPropertyModel.IsSolo = (sender as System.Windows.Controls.Primitives.ToggleButton).IsChecked == true;

            var newEventArgs = new RoutedEventArgs(SoloMuteClickEvent);
            RaiseEvent(newEventArgs);
        }

        private void SoundControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!player.IsOpen || player.Source.AbsoluteUri != SoundPropertyModel.Sound.FilePath)
            {
                player.RendererOptions.DirectSoundDevice = OutputDevice;
                if (!SoundPropertyModel.IsMuted)
                {
                    if (loadSoundCancellationTokenSource?.IsCancellationRequested ?? true)
                    {
                        loadSoundCancellationTokenSource = new CancellationTokenSource();
                    }

                    var sound = SoundPropertyModel.Sound;

                    // Load sound and query a play
                    _ = Task.Run(async () =>
                    {
                        if (sound.IsURL)
                        {
                            // URLs should be streamed, which requires an audioURL. We use YoutubDL for that
                            var youtubeDl = new YoutubeDL
                            {
                                VideoUrl = sound.FilePath,
                                RetrieveAllInfo = true
                            };

                            await youtubeDl.PrepareDownloadAsync();
                            var audioURL = (youtubeDl.Info as VideoDownloadInfo).RequestedFormats.First(v => v.Acodec != "none").Url;

                            await player.Open(new Uri(Uri.UnescapeDataString(audioURL)));
                        }
                        else
                        {
                            await player.Open(new Uri(sound.FilePath));
                        }

                        // Sometimes the player status is not updated instantly, give it a moment
                        int sleepCount = 0;
                        while (!player.IsOpen && sleepCount < 5)
                        {
                            sleepCount++; // But not infinitely
                            Thread.Sleep(500);
                        }

                        // The task was cancelled while loading
                        if (loadSoundCancellationTokenSource.IsCancellationRequested)
                        {
                            return;
                        }

                        if (isPlayQueried)
                        {
                            isPlayQueried = false;
                            _ = Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                            {
                                _ = Play();
                            }));
                        }
                    }, loadSoundCancellationTokenSource.Token);
                }
            }
        }

        private void SoundControl_Unloaded(object sender, EventArgs e)
        {
            loadSoundCancellationTokenSource?.Cancel();
            Stop();
        }

        #endregion
    }
}
