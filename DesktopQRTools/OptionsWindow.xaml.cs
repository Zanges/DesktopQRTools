using System;
using System.IO;
using System.Windows;

namespace DesktopQRTools
{
    public partial class OptionsWindow : Window
    {
        private const string ConfigFileName = "config.ini";

        public OptionsWindow()
        {
            InitializeComponent();
            LoadOptions();
            UpdateControlsState();
        }

        private void SkipSaveDialogCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            UpdateControlsState();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    AutoSaveDirectoryTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void UpdateControlsState()
        {
            bool isEnabled = SkipSaveDialogCheckBox.IsChecked ?? false;
            AutoSaveQRCodeNameTextBox.IsEnabled = isEnabled;
            AutoSaveDirectoryTextBox.IsEnabled = isEnabled;
            AppendDateCheckBox.IsEnabled = isEnabled;
            AppendTimeCheckBox.IsEnabled = isEnabled;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveOptions())
            {
                MessageBox.Show("Options saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Failed to save options.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool SaveOptions()
        {
            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string configFilePath = Path.Combine(appDirectory, ConfigFileName);

                using (StreamWriter writer = new StreamWriter(configFilePath))
                {
                    writer.WriteLine($"AutoSaveQRCodeName={AutoSaveQRCodeNameTextBox.Text}");
                    writer.WriteLine($"SkipSaveDialog={SkipSaveDialogCheckBox.IsChecked}");
                    writer.WriteLine($"AutoSaveDirectory={AutoSaveDirectoryTextBox.Text}");
                    writer.WriteLine($"AppendDate={AppendDateCheckBox.IsChecked}");
                    writer.WriteLine($"AppendTime={AppendTimeCheckBox.IsChecked}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving options: {ex.Message}");
                return false;
            }
        }

        private void LoadOptions()
        {
            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string configFilePath = Path.Combine(appDirectory, ConfigFileName);

                if (File.Exists(configFilePath))
                {
                    string[] lines = File.ReadAllLines(configFilePath);
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            switch (parts[0])
                            {
                                case "AutoSaveQRCodeName":
                                    AutoSaveQRCodeNameTextBox.Text = parts[1];
                                    break;
                                case "SkipSaveDialog":
                                    if (bool.TryParse(parts[1], out bool skipDialog))
                                        SkipSaveDialogCheckBox.IsChecked = skipDialog;
                                    break;
                                case "AutoSaveDirectory":
                                    AutoSaveDirectoryTextBox.Text = parts[1];
                                    break;
                                case "AppendDate":
                                    if (bool.TryParse(parts[1], out bool appendDate))
                                        AppendDateCheckBox.IsChecked = appendDate;
                                    break;
                                case "AppendTime":
                                    if (bool.TryParse(parts[1], out bool appendTime))
                                        AppendTimeCheckBox.IsChecked = appendTime;
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading options: {ex.Message}");
            }
            UpdateControlsState();
        }
    }
}
