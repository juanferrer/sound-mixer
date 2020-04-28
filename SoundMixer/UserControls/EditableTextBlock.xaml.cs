using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SoundMixer.UserControls
{
    /// <summary>
    /// Interaction logic for EditableTextBlock.xaml
    /// </summary>
    public partial class EditableTextBlock : UserControl
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(EditableTextBlock));

        public EditableTextBlock()
        {
            InitializeComponent();
        }

        public void EnterEditMode()
        {
            textBox.Visibility = Visibility.Visible;
            textBox.Focus();
            textBox.SelectAll();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            textBox.Visibility = Visibility.Hidden;
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            var key = e.Key;

            // Enter also needs to accept
            if (key == Key.Enter || key == Key.Return)
            {
                textBox.Visibility = Visibility.Hidden;
            }
        }
    }
}
