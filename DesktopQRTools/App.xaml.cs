using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;

namespace DesktopQRTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private GlobalHotKey _globalHotKey;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = new MainWindow();
            MainWindow.Show();

            await SetupGlobalHotKeyAsync();

            MainWindow.Closed += async (s, args) =>
            {
                // Dispose of the global hotkey
                await DisposeGlobalHotKeyAsync();

                // Close all windows when the main window is closed
                foreach (Window window in Windows.Cast<Window>().ToList())
                {
                    window.Close();
                }
                Shutdown();
            };

            // Register for messages
            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
        }

        public async Task SetupGlobalHotKeyAsync()
        {
            await DisposeGlobalHotKeyAsync();

            try
            {
                var (modifiers, key) = LoadHotkeyFromConfig();
                _globalHotKey = new GlobalHotKey(modifiers, key, new WindowInteropHelper(MainWindow).Handle);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to register global hotkey: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private (ModifierKeys modifiers, Key key) LoadHotkeyFromConfig()
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string configFilePath = Path.Combine(appDirectory, "config.ini");

            if (File.Exists(configFilePath))
            {
                string[] lines = File.ReadAllLines(configFilePath);
                ModifierKeys modifiers = ModifierKeys.None;
                Key key = Key.None;

                foreach (string line in lines)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        switch (parts[0])
                        {
                            case "ScanHotkey":
                                if (Enum.TryParse(parts[1], out Key parsedKey))
                                    key = parsedKey;
                                break;
                            case "ScanHotkeyModifiers":
                                if (Enum.TryParse(parts[1], out ModifierKeys parsedModifiers))
                                    modifiers = parsedModifiers;
                                break;
                        }
                    }
                }

                if (key != Key.None)
                {
                    return (modifiers, key);
                }
            }

            // If config file doesn't exist or hotkey is not set, return default values
            return (ModifierKeys.Control | ModifierKeys.Alt, Key.S);
        }

        private async Task DisposeGlobalHotKeyAsync()
        {
            if (_globalHotKey != null)
            {
                _globalHotKey.Dispose();
                _globalHotKey = null;
                await Task.Delay(100); // Give some time for the hotkey to be unregistered
            }
        }

        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message == 786) // WM_HOTKEY
            {
                // Trigger the stand functionality
                ((MainWindow)MainWindow).TriggerStandFunctionality();
                handled = true;
            }
        }
    }
}
