using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Stylet;

namespace SoundMixer.UserControls
{
    /// <summary>
    /// Interaction logic for SoundListControl.xaml
    /// </summary>
    public partial class SoundListControl : UserControl
    {
        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public BindableCollection<string> FilePaths
        {
            get { return (BindableCollection<string>)GetValue(FilePathsProperty); }
            set { SetValue(FilePathsProperty, value); }
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

        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register("FilePath", typeof(string), typeof(SoundListControl));
        public static readonly DependencyProperty FilePathsProperty = DependencyProperty.Register("FilePaths", typeof(BindableCollection<string>), typeof(SoundListControl));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SoundListControl));
        public static readonly DependencyProperty DialogTypeProperty = DependencyProperty.Register("DialogType", typeof(string), typeof(SoundListControl));
        public static readonly DependencyProperty DialogFilterProperty = DependencyProperty.Register("DialogFilter", typeof(string), typeof(SoundListControl));

        public SoundListControl()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            FileDialog fileDialog = DialogType switch
            {
                "Open" => new OpenFileDialog(),
                "Save" => new SaveFileDialog(),
                _ => throw new ArgumentException("Only \"Open\" and \"Save\" dialogs can be created this way"),
            };
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

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            FilePaths.Add(FilePath);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var selectedItem in PathList.SelectedItems.Cast<string>())
            {
                FilePaths.Remove(selectedItem);
            }
        }
    }
}
