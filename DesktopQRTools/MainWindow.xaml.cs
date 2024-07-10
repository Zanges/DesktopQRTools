using System.Windows;

namespace DesktopQRTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event handler for the Create QR Code button click.
        /// Opens a new window for QR code creation.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void CreateQRCodeButton_Click(object sender, RoutedEventArgs e)
        {
            QRCodeGeneratorWindow qrCodeGeneratorWindow = new QRCodeGeneratorWindow();
            qrCodeGeneratorWindow.Show();
        }

        /// <summary>
        /// Event handler for the Scan QR Code button click.
        /// Initiates the process to scan a QR code on the screen.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void ScanQRCodeButton_Click(object sender, RoutedEventArgs e)
        {
            QRCodeScannerWindow scannerWindow = new QRCodeScannerWindow();
            scannerWindow.Show();
        }
    }
}
