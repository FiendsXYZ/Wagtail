using DotNetEnv;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace wagtail
{
    public class FileItem
    {
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
    }

    public partial class MainWindow : Window
    {
        private Point startPoint;

        public MainWindow()
        {
            LoadEnvironmentVariables();
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void FileListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }

        private void FileListView_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (
                e.LeftButton == MouseButtonState.Pressed
                && (
                    Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance
                )
            )
            {
                ListView listView = sender as ListView;
                ListViewItem listViewItem = FindAncestor<ListViewItem>(
                    (DependencyObject)e.OriginalSource
                );

                if (listViewItem != null)
                {
                    FileItem fileItem = (FileItem)
                        listView.ItemContainerGenerator.ItemFromContainer(listViewItem);

                    if (fileItem != null && fileItem.FilePath != null)
                    {
                        string[] fileToMove = { fileItem.FilePath };
                        DataObject dragData = new DataObject(DataFormats.FileDrop, fileToMove);
                        DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Copy);
                    }
                }
            }
        }

        private static T FindAncestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);
            return null;
        }

        private async void OnDownloadClick(object sender, RoutedEventArgs e)
        {
            // Check if environment variables are set
            string? apiKey = Environment.GetEnvironmentVariable("XI_API_KEY");
            string? voiceId = Environment.GetEnvironmentVariable("VOICE_ID");

            // If either is not set, show a message box and return
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(voiceId))
            {
                MessageBox.Show(
                    "Please set the API Key and Voice ID in the configuration.",
                    "Configuration Missing",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            string sanitizedInput = SanitizeFileName(InputTextBox.Text); // Use InputTextBox.Text 
            string filePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                $"{sanitizedInput}.mp3"
            );

            var requestData = new
            {
                text = InputTextBox.Text,
                model_id = "eleven_monolingual_v1",
                voice_settings = new { stability = 0.5, similarity_boost = 0.5 }
            };

            string requestJson = JsonSerializer.Serialize(requestData);

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "audio/mpeg");
                httpClient.DefaultRequestHeaders.Add("xi-api-key", apiKey);

                var response = await httpClient.PostAsync(
                    "https://api.elevenlabs.io/v1/text-to-speech/" + voiceId,
                    new StringContent(requestJson, Encoding.UTF8, "application/json")
                );

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Failed to download file.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return;
                }

                byte[] mp3Data = await response.Content.ReadAsByteArrayAsync();

                File.WriteAllBytes(filePath, mp3Data);
            }

            FileListView.Items.Add(
                new FileItem { FileName = $"{sanitizedInput}.mp3", FilePath = filePath }
            );
        }

        private string SanitizeFileName(string fileName)
        {
            string invalidChars =
                new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            return Regex.Replace(fileName, "[" + Regex.Escape(invalidChars) + "]", "");
        }

        private void FileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedFile = (FileItem)FileListView.SelectedItem;
            if (selectedFile != null && selectedFile.FilePath != null)
            {
                Process.Start(
                    new ProcessStartInfo(selectedFile.FilePath) { UseShellExecute = true }
                );
            }
        }

        private void OpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            var selectedFile = (FileItem)FileListView.SelectedItem;
            if (selectedFile != null && selectedFile.FilePath != null)
            {
                string argument = "/select, \"" + selectedFile.FilePath + "\"";
                Process.Start("explorer.exe", argument);
            }
        }

        private void StopPlayback()
        {
            MediaPlayer.Stop();
        }

        private bool isPlaying = false;

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                isPlaying = false;
                PlayButton.Content = "Play";
            });
        }

        private void PlayFile_Click(object sender, RoutedEventArgs e)
        {
            var selectedFile = (FileItem)FileListView.SelectedItem;
            if (selectedFile != null && selectedFile.FilePath != null)
            {
                try
                {
                    if (isPlaying)
                    {
                        StopPlayback();
                    }
                    else
                    {
                        MediaPlayer.Source = new Uri(selectedFile.FilePath);
                        MediaPlayer.Play();
                    }
                    isPlaying = !isPlaying; // Toggle the isPlaying flag
                    PlayButton.Content = isPlaying ? "Stop" : "Play"; // Update button content
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"An error occurred: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }

        //////////////////////////////////////////
        private void OnConfigClick(object sender, RoutedEventArgs e)
        {
            ConfigWindow configWindow = new ConfigWindow();
            if (configWindow.ShowDialog() == true)
            {
                Environment.SetEnvironmentVariable("XI_API_KEY", configWindow.ApiKey.Text);
                Environment.SetEnvironmentVariable("VOICE_ID", configWindow.VoiceId.Text);
            }
        }

        //////////////////////////////////////////
        private void LoadEnvironmentVariables()
        {
            DotNetEnv.Env.Load();
        }

        //////////////////////////////////////////
        private void InputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (InputTextBox.Text == "Please enter TTS prompt...")
            {
                InputTextBox.Text = "";
                InputTextBox.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void InputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                InputTextBox.Foreground = new SolidColorBrush(Colors.White);
                InputTextBox.Text = "Please enter TTS prompt...";
            }
        }

        //////////////////////////////////////////
    }
}
