using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Linq;

namespace DesktopQRTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private GlobalHotKey _globalHotKey;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = new MainWindow();
            MainWindow.Show();

            SetupGlobalHotKey();

            MainWindow.Closed += (s, args) =>
            {
                // Dispose of the global hotkey
                _globalHotKey?.Dispose();

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

        private void SetupGlobalHotKey()
        {
            // TODO: Replace with actual configured hotkey
            ModifierKeys modifiers = ModifierKeys.Control | ModifierKeys.Alt;
            Key key = Key.S;

            try
            {
                _globalHotKey = new GlobalHotKey(modifiers, key, new WindowInteropHelper(MainWindow).Handle);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to register global hotkey: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
