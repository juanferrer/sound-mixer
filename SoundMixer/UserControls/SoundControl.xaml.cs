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
        private string playIcon = "play.png";
        private string pauseIcon = "pause.png";
        private string stopIcon = "stop.png";
        private bool isPlaying = false;

        private Task delayedPlay;
        private CancellationTokenSource delayedPlayCancellationTokenSource;

        public SoundPropertyModel SoundPropertyModel
        {
            get { return (SoundPropertyModel)GetValue(SoundPropertyModelProperty); }
            set { SetValue(SoundPropertyModelProperty, value); }
        }

        public static readonly DependencyProperty SoundPropertyModelProperty = DependencyProperty.Register("SoundPropertyModel", typeof(SoundPropertyModel), typeof(SoundControl));

        public SoundControl()
        {
            InitializeComponent();

            // Set the default icon for the PlayButton
            UpdatePlayButtonIcon();
        }

        private void UpdatePlayButtonIcon()
        {
            playButtonImage.Source = new BitmapImage(new Uri(Path.Combine(resourcesPath, isPlaying ? stopIcon : playIcon)));
        }

        private void UpdatePlayer()
        {
            player.Volume = volumeSlider.Value;
        }

        public void EnterEditMode()
        {
            editableTextBlock.EnterEditMode();
        }

        public void Play()
        {
            UpdatePlayer();
            player.Position = TimeSpan.Zero;
            player.Play();
            isPlaying = true;
            UpdatePlayButtonIcon();
        }

        public Task AsyncPlay()
        {
            UpdatePlayer();
            int delayTime = SoundPropertyModel.DelayTime;
            isPlaying = true;
            UpdatePlayButtonIcon();

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
            isPlaying = false;
            UpdatePlayButtonIcon();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
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
                isPlaying = false;
                UpdatePlayButtonIcon();
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.Volume = volumeSlider.Value;
        }

        private void EditableTextBlock_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }
    }
}
