using System.Windows;
using System.Linq;

namespace DesktopQRTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = new MainWindow();
            MainWindow.Show();

            MainWindow.Closed += (s, args) =>
            {
                // Close all windows when the main window is closed
                foreach (Window window in Windows.Cast<Window>().ToList())
                {
                    window.Close();
                }
                Shutdown();
            };
        }
            };
                {
                    window.Close();
                }
            };
        }
    }
}
