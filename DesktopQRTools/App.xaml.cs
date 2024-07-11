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
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var modifiersString = config.AppSettings.Settings["HotkeyModifiers"]?.Value ?? "Control,Alt";
                var keyString = config.AppSettings.Settings["HotkeyKey"]?.Value ?? "S";

                ModifierKeys modifiers = (ModifierKeys)Enum.Parse(typeof(ModifierKeys), modifiersString.Replace(",", " "), true);
                Key key = (Key)Enum.Parse(typeof(Key), keyString);

                _globalHotKey = new GlobalHotKey(modifiers, key, new WindowInteropHelper(MainWindow).Handle);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to register global hotkey: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
