using System;
using System.Windows;

namespace DesktopQRTools
{
    public static class QRCodeScanFunctionality
    {
        public static void TriggerQRScan()
        {
            try
            {
                QRCodeScannerWindow.ShowNew();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while opening the QR Code Scanner: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
