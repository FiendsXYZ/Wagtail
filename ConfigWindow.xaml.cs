using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace wagtail
{
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Initialize with environment variables or placeholder text.
            ApiKey.Text = Environment.GetEnvironmentVariable("XI_API_KEY") ?? "Enter API Key";
            ApiKey.Foreground = string.IsNullOrEmpty(
                Environment.GetEnvironmentVariable("XI_API_KEY")
            )
                ? Brushes.Gray
                : Brushes.White;

            VoiceId.Text = Environment.GetEnvironmentVariable("VOICE_ID") ?? "Enter Voice ID";
            VoiceId.Foreground = string.IsNullOrEmpty(
                Environment.GetEnvironmentVariable("VOICE_ID")
            )
                ? Brushes.Gray
                : Brushes.White;
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            // Update the environment variables.
            Environment.SetEnvironmentVariable("XI_API_KEY", ApiKey.Text);
            Environment.SetEnvironmentVariable("VOICE_ID", VoiceId.Text);

            DialogResult = true;
            Close();
        }

        ///////////////
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                if (textBox.Text == "Enter API Key" || textBox.Text == "Enter Voice ID")
                {
                    textBox.Text = "";
                    textBox.Foreground = new SolidColorBrush(Colors.White);
                }
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = textBox.Name == "ApiKey" ? "Enter API Key" : "Enter Voice ID";
                    textBox.Foreground = new SolidColorBrush(Colors.White);
                }
            }
        }
        //////////////////
    }
}
