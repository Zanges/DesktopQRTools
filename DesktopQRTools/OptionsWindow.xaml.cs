using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace DesktopQRTools
{
    public partial class OptionsWindow : Window
    {
        private const string ConfigFileName = "config.ini";
        private Key _scanHotkey;
        private ModifierKeys _scanHotkeyModifiers;

        public enum ScannerMode
        {
            DrawBox,
            TargetingRectangle,
            AutomaticDetection
        }

        public OptionsWindow()
        {
            InitializeComponent();
            LoadOptions();
            UpdateControlsState();
        }

        private bool _isRecording = false;

        private void ScanHotkeyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            
            if (!_isRecording)
            {
                // Clear the textbox if Escape is pressed
                if (e.Key == Key.Escape)
                {
                    ScanHotkeyTextBox.Clear();
                    _scanHotkey = Key.None;
                    _scanHotkeyModifiers = ModifierKeys.None;
                    return;
                }

                // Ignore modifier keys when pressed alone
                if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.LeftAlt || e.Key == Key.RightAlt || e.Key == Key.LeftShift || e.Key == Key.RightShift)
                {
                    return;
                }

                _scanHotkey = e.Key;
                _scanHotkeyModifiers = Keyboard.Modifiers;

                string hotkeyText = GetHotkeyString();
                ScanHotkeyTextBox.Text = hotkeyText;
            }
        }

        private void RecordHotkeyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRecording)
            {
                _isRecording = true;
                RecordHotkeyButton.Content = "Stop";
                ScanHotkeyTextBox.Text = "Press hotkey...";
                ScanHotkeyTextBox.Focus();
            }
            else
            {
                _isRecording = false;
                RecordHotkeyButton.Content = "Record";
                if (_scanHotkey == Key.None)
                {
                    ScanHotkeyTextBox.Text = "";
                }
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (_isRecording)
            {
                e.Handled = true;

                if (e.Key == Key.Escape)
                {
                    _isRecording = false;
                    RecordHotkeyButton.Content = "Record";
                    ScanHotkeyTextBox.Text = "";
                    _scanHotkey = Key.None;
                    _scanHotkeyModifiers = ModifierKeys.None;
                    return;
                }

                if (e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl && e.Key != Key.LeftAlt && e.Key != Key.RightAlt && e.Key != Key.LeftShift && e.Key != Key.RightShift)
                {
                    _scanHotkey = e.Key;
                    _scanHotkeyModifiers = Keyboard.Modifiers;
                    string hotkeyText = GetHotkeyString();
                    ScanHotkeyTextBox.Text = hotkeyText;
                    _isRecording = false;
                    RecordHotkeyButton.Content = "Record";
                }
            }
        }

        private string GetHotkeyString()
        {
            string hotkeyText = "";
            if (_scanHotkeyModifiers.HasFlag(ModifierKeys.Control)) hotkeyText += "Ctrl + ";
            if (_scanHotkeyModifiers.HasFlag(ModifierKeys.Alt)) hotkeyText += "Alt + ";
            if (_scanHotkeyModifiers.HasFlag(ModifierKeys.Shift)) hotkeyText += "Shift + ";
            hotkeyText += _scanHotkey.ToString();
            return hotkeyText;
        }

        private void SkipSaveDialogCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            UpdateControlsState();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select Auto-save Directory"
            };

            if (dialog.ShowDialog() == true)
            {
                AutoSaveDirectoryTextBox.Text = dialog.FolderName;
            }
        }

        private void UpdateControlsState()
        {
            bool isEnabled = SkipSaveDialogCheckBox.IsChecked ?? false;
            AutoSaveQRCodeNameTextBox.IsEnabled = isEnabled;
            AutoSaveDirectoryTextBox.IsEnabled = isEnabled;
            AutoSaveDirectoryBrowseButton.IsEnabled = isEnabled;
            AppendDateCheckBox.IsEnabled = isEnabled;
            AppendTimeCheckBox.IsEnabled = isEnabled;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveOptions())
            {
                ShowStatusMessage("Options saved successfully!");
                Task.Delay(2000).ContinueWith(_ => Dispatcher.Invoke(this.Close));
            }
            else
            {
                ShowStatusMessage("Failed to save options.", isError: true);
            }
        }

        private void ShowStatusMessage(string message, bool isError = false)
        {
            StatusMessage.Text = message;
            StatusMessage.Foreground = isError ? Brushes.Red : Brushes.Green;
            StatusMessage.Visibility = Visibility.Visible;
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
                    writer.WriteLine($"ScannerMode={ScannerModeComboBox.SelectedIndex}");
                    writer.WriteLine($"ScanHotkey={_scanHotkey}");
                    writer.WriteLine($"ScanHotkeyModifiers={_scanHotkeyModifiers}");
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
                                case "ScannerMode":
                                    if (int.TryParse(parts[1], out int scannerMode))
                                        ScannerModeComboBox.SelectedIndex = scannerMode;
                                    break;
                                case "ScanHotkey":
                                    if (Enum.TryParse(parts[1], out Key scanHotkey))
                                        _scanHotkey = scanHotkey;
                                    break;
                                case "ScanHotkeyModifiers":
                                    if (Enum.TryParse(parts[1], out ModifierKeys scanHotkeyModifiers))
                                        _scanHotkeyModifiers = scanHotkeyModifiers;
                                    break;
                            }
                        }
                    }
                }

                // Update the ScanHotkeyTextBox
                ScanHotkeyTextBox.Text = GetHotkeyString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading options: {ex.Message}");
            }
            UpdateControlsState();
        }

        public ScannerMode GetScannerMode()
        {
            return (ScannerMode)ScannerModeComboBox.SelectedIndex;
        }

        public int GetOptionsCount()
        {
            return 5; // Return the current number of options
        }

        public string GetAutoSaveQRCodeName()
        {
            return AutoSaveQRCodeNameTextBox.Text;
        }

        public bool GetSkipSaveDialog()
        {
            return SkipSaveDialogCheckBox.IsChecked ?? false;
        }

        public string GetAutoSaveDirectory()
        {
            return AutoSaveDirectoryTextBox.Text;
        }

        public bool GetAppendDate()
        {
            return AppendDateCheckBox.IsChecked ?? false;
        }

        public bool GetAppendTime()
        {
            return AppendTimeCheckBox.IsChecked ?? false;
        }
    }
}
