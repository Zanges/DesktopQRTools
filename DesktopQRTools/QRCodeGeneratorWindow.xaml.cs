using System;
using System.Windows;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;

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

            try
            {
                var qrCodeBitmap = GenerateQRCode(content);
                QRCodeImage.Source = ConvertToBitmapSource(qrCodeBitmap);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while generating the QR code: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Generates a QR code from the given text.
        /// </summary>
        /// <param name="text">The text to encode in the QR code.</param>
        /// <returns>A bitmap image of the generated QR code.</returns>
        private System.Drawing.Bitmap GenerateQRCode(string text)
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = 300,
                    Width = 300,
                    Margin = 1
                }
            };

            var pixelData = writer.Write(text);
            var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            using (var ms = new System.IO.MemoryStream(pixelData.Pixels))
            {
                bitmap.SetResolution(96, 96);
                var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
            }

            return bitmap;
        }

        /// <summary>
        /// Converts a System.Drawing.Bitmap to a BitmapSource for WPF display.
        /// </summary>
        /// <param name="bitmap">The System.Drawing.Bitmap to convert.</param>
        /// <returns>A BitmapSource for WPF display.</returns>
        private BitmapSource ConvertToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                System.Windows.Media.PixelFormats.Bgr32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }
    }
}
