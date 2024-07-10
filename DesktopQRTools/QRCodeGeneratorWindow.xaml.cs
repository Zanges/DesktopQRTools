using System.Windows;

namespace DesktopQRTools
{
    /// <summary>
    /// Interaction logic for QRCodeGeneratorWindow.xaml
    /// </summary>
    public partial class QRCodeGeneratorWindow : Window
    {
        public QRCodeGeneratorWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event handler for the Generate QR Code button click.
        /// Generates a QR code based on the content in the text box.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            string content = ContentTextBox.Text;
            if (string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("Please enter some content for the QR code.", "Empty Content", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: Implement QR code generation
            MessageBox.Show($"QR Code will be generated with the following content:\n\n{content}", "QR Code Generation", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
