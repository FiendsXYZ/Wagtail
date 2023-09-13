using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;

namespace wagtail
{
    public partial class ConfigWindow : Window
    {
        // Define a constant for the log file path
        private const string LogFilePath = "log.txt";

        public ConfigWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Load the config file

            // Write a log message to log.txt to see if this is being called
            Log("ConfigWindow constructor called.");

            var (apiKey, voiceId) = ConfigHelper.LoadConfig();

            if (apiKey != null && voiceId != null)
            {
                // Debug message to check loaded values
                Log($"Loaded API Key: {apiKey}");
                Log($"Loaded Voice ID: {voiceId}");

                // Initialize with values from config.json
                ApiKey.Text = apiKey;
                VoiceId.Text = voiceId;
                ApiKey.Foreground = !string.IsNullOrEmpty(apiKey) ? Brushes.White : Brushes.Gray;
                VoiceId.Foreground = !string.IsNullOrEmpty(voiceId) ? Brushes.White : Brushes.Gray;
            }
            else
            {
                // Initialize with environment variables or placeholder text.
                ApiKey.Text = Environment.GetEnvironmentVariable("XI_API_KEY") ?? "Enter API Key";
                VoiceId.Text = Environment.GetEnvironmentVariable("VOICE_ID") ?? "Enter Voice ID";
                ApiKey.Foreground = string.IsNullOrEmpty(
                    Environment.GetEnvironmentVariable("XI_API_KEY")
                )
                    ? Brushes.Gray
                    : Brushes.White;
                VoiceId.Foreground = string.IsNullOrEmpty(
                    Environment.GetEnvironmentVariable("VOICE_ID")
                )
                    ? Brushes.Gray
                    : Brushes.White;
            }
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Update the environment variables.
                Environment.SetEnvironmentVariable("XI_API_KEY", ApiKey.Text);
                Environment.SetEnvironmentVariable("VOICE_ID", VoiceId.Text);

                // This is where the code might be failing

                Log("Calling SaveConfig method.");
                try
                {
                    ConfigHelper.SaveConfig(ApiKey.Text, VoiceId.Text);
                    Log("SaveConfig method succeeded.");
                }
                catch (Exception ex)
                {
                    Log($"An error occurred: {ex.Message}");
                }


                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                // Log the error message
                Log($"An error occurred: {ex.Message}");

                // Show an error message
                MessageBox.Show(
                    $"An error occurred: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
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

        // Helper method to log messages to log.txt
        private void Log(string message)
        {
            try
            {
                File.AppendAllText(LogFilePath, $"{DateTime.Now}: {message}\n");
            }
            catch (Exception ex)
            {
                // Handle any errors that occur while logging (e.g., disk full, permissions issues)
                MessageBox.Show(
                    $"An error occurred while logging: {ex.Message}",
                    "Logging Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        //////////////////
    }
}
