using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace SoundMixer.UserControls
{
    /// <summary>
    /// Interaction logic for BrowseControl.xaml
    /// </summary>
    public partial class BrowseControl : UserControl
    {
        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Should be either "Open" or "Save"
        public string DialogType
        {
            get { return (string)GetValue(DialogTypeProperty); }
            set { SetValue(DialogTypeProperty, value); }
        }

        public string DialogFilter
        {
            get { return (string)GetValue(DialogFilterProperty); }
            set { SetValue(DialogFilterProperty, value); }
        }

        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register("FilePath", typeof(string), typeof(BrowseControl));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(BrowseControl));
        public static readonly DependencyProperty DialogTypeProperty = DependencyProperty.Register("DialogType", typeof(string), typeof(BrowseControl));
        public static readonly DependencyProperty DialogFilterProperty = DependencyProperty.Register("DialogFilter", typeof(string), typeof(BrowseControl));

        public BrowseControl()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            FileDialog fileDialog;
            switch (DialogType)
            {
                case "Open":
                    fileDialog = new OpenFileDialog();
                    break;
                case "Save":
                    fileDialog = new SaveFileDialog();
                    break;
                default:
                    throw new ArgumentException("Only \"Open\" and \"Save\" dialogs can be created this way");

            }

            if (!string.IsNullOrEmpty(DialogFilter))
            {
                fileDialog.Filter = DialogFilter;
            }

            if (fileDialog.ShowDialog() == true)
            {
                FilePath = fileDialog.FileName;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            FilePath = (sender as TextBox).Text;
        }
    }
}
