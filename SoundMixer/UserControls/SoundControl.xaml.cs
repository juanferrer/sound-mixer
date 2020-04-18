using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

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
        private bool isLooping = true;

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set
            {
                SetValue(FilePathProperty, value);
                // We also need to load the file if it exists
                if (!File.Exists(FilePath))
                {
                    player.Source = null;
                    // Add a red border or something
                    // TODO
                }
            }
        }

        public double Volume
        {
            get { return (double)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SoundControl));
        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register("FilePath", typeof(string), typeof(SoundControl));
        public static readonly DependencyProperty VolumeProperty = DependencyProperty.Register("Volume", typeof(double), typeof(SoundControl));

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

        public void EnterEditMode()
        {
            editableTextBlock.EnterEditMode();
        }

        public void Play()
        {
            player.Play();
            isPlaying = true;
            isLooping = true;
            UpdatePlayButtonIcon();
        }

        public void Stop()
        {
            player.Stop();
            isPlaying = false;
            isLooping = false;
            UpdatePlayButtonIcon();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                isLooping = false;
                player.Stop();
                isPlaying = false;
            }
            else
            {
                // First, does it exist?
                if (!File.Exists(FilePath))
                {
                    // Add a red border
                    playButton.BorderBrush = new SolidColorBrush(missingSoundColor);
                    return;
                }

                isLooping = true;
                player.Play();
                isPlaying = true;
            }
            UpdatePlayButtonIcon();
        }

        private void SoundMediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            // The sound may have to loop
            if (isLooping)
            {
                player.Position = TimeSpan.Zero;
                player.Play();
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
