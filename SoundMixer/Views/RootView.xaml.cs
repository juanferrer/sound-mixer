using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SoundMixer.Views
{
    /// <summary>
    /// Interaction logic for RootView.xaml
    /// </summary>
    public partial class RootView : Window
    {
        public RootView()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox)
            {
                mainGrid.Focus();
            }
        }
    }
}
