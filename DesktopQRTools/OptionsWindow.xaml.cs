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
                    writer.WriteLine($"ExampleOption={ExampleOptionCheckBox.IsChecked}");
                    // Add more options here as needed
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
                            if (parts[0] == "ExampleOption" && bool.TryParse(parts[1], out bool isChecked))
                            {
                                ExampleOptionCheckBox.IsChecked = isChecked;
                            }
                            // Add more options here as needed
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading options: {ex.Message}");
            }
        }
    }
}
