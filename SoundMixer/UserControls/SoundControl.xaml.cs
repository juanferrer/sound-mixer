using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using SoundMixer.Models;

namespace SoundMixer.UserControls
{
    /// <summary>
    /// Interaction logic for SoundControl.xaml
    /// </summary>
    public partial class SoundControl : UserControl
    {
        private static Color missingSoundColor = Color.FromRgb(255, 0, 0);

        private string resourcesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources");

        private Random rng;
        private CancellationTokenSource delayedPlayCancellationTokenSource;

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

        public static readonly DependencyProperty SoundPropertyModelProperty = DependencyProperty.Register("SoundPropertyModel", typeof(SoundPropertyModel), typeof(SoundControl));
        public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register("IsPlaying", typeof(bool), typeof(SoundControl));

        public SoundControl()
        {
            InitializeComponent();

            rng = new Random();
        }

        private void UpdatePlayer()
        {
            player.Volume = volumeSlider.Value;
        }

        public void Play()
        {
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

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayOrStop();
        }

        private void SoundMediaElement_MediaEnded(object sender, RoutedEventArgs e)
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
    }
}
