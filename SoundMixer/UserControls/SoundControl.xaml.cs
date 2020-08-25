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

        private static Color missingSoundColor = Color.FromRgb(255, 0, 0);

        private string resourcesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources");

        private Random rng;
        private CancellationTokenSource delayedPlayCancellationTokenSource;
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
        }

        private void UpdatePlayer()
        {
            player.Volume = volumeSlider.Value;
        }

        public void Play()
        {
            if (!player.IsOpen)
            {
                isPlayQueried = true;
                return;
            }
            UpdatePlayer();
            player.Position = TimeSpan.Zero;
            player.Play();
            IsPlaying = true;
        }

        public Task AsyncPlay()
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

            return Task.Factory.StartNew(async () =>
            {
                await Task.Delay(delayTime);
                if (delayedPlayCancellationTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }

                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    Play();
                }));
            }, delayedPlayCancellationTokenSource.Token);
        }

        public void Stop()
        {
            UpdatePlayer();
            if (SoundPropertyModel.IsDelayed)
            {
                delayedPlayCancellationTokenSource.Cancel();
            }
            player.Stop();
            IsPlaying = false;
        }

        public void PlayOrStop()
        {
            if (IsPlaying)
            {
                Stop();
            }
            else
            {
                // First, does it exist?
                if (!File.Exists(SoundPropertyModel.Sound.FilePath))
                {
                    // Add a red border
                    playButton.BorderBrush = new SolidColorBrush(missingSoundColor);
                    return;
                }

                if (SoundPropertyModel.IsDelayed)
                {
                    AsyncPlay();
                }
                else
                {
                    Play();
                }
            }
        }

        public void SetPlayerMute(bool state)
        {
            player.IsMuted = state;
        }

        public void SetOutputDevice(DirectSoundDeviceInfo outputDevice)
        {
            player.RendererOptions.DirectSoundDevice = outputDevice;
        }

        #region Event Handlers

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayOrStop();
        }

        private void SoundMediaElement_MediaEnded(object sender, EventArgs e)
        {
            // The sound may have to loop
            if (SoundPropertyModel.IsLoop)
            {
                if (SoundPropertyModel.IsDelayed)
                {
                    AsyncPlay();
                }
                else
                {
                    Play();
                }
            }
            else
            {
                player.Stop();
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

        private async void SoundControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!player.IsOpen || player.Source.AbsoluteUri != SoundPropertyModel.Sound.FilePath)
            {
                player.RendererOptions.DirectSoundDevice = OutputDevice;
                if (!SoundPropertyModel.IsMuted)
                {
                    await player.Open(new Uri(SoundPropertyModel.Sound.FilePath));
                    if (isPlayQueried)
                    {
                        isPlayQueried = false;
                        Play();
                    }
                }
            }
        }

        #endregion
    }
}
