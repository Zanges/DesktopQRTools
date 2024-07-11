using System;
using System.Windows;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;

namespace DesktopQRTools
{
    public partial class QRCodeResultWindow : Window
    {
        public QRCodeResultWindow(string content)
        {
            InitializeComponent();
            ContentTextBlock.Text = content;

            if (Uri.TryCreate(content, UriKind.Absolute, out Uri? uriResult) &&
                (uriResult?.Scheme == Uri.UriSchemeHttp || uriResult?.Scheme == Uri.UriSchemeHttps))
            {
                OpenLinkButton.Visibility = Visibility.Visible;
            }
        }

        private void OpenLinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (Uri.TryCreate(ContentTextBlock.Text, UriKind.Absolute, out Uri? uri) && uri != null)
            {
                Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
            }
        }

        private void SaveContentButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = "txt",
                AddExtension = true
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, ContentTextBlock.Text);
                MessageBox.Show("Content saved successfully.", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public bool IsLinkButtonVisible()
        {
            return OpenLinkButton.Visibility == Visibility.Visible;
        }
    }
}