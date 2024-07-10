using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using ZXing;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;
using ZXing.Common;
using System.Windows.Interop;
using System.Diagnostics;

namespace DesktopQRTools
{
    /// <summary>
    /// Interaction logic for QRCodeScannerWindow.xaml
    /// </summary>
    public partial class QRCodeScannerWindow : Window
    {
        private System.Windows.Point startPoint;

        public QRCodeScannerWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += QRCodeScannerWindow_MouseLeftButtonDown;
            this.MouseLeftButtonUp += QRCodeScannerWindow_MouseLeftButtonUp;
            this.MouseMove += QRCodeScannerWindow_MouseMove;

            // Make the window transparent but not click-through
            this.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            this.AllowsTransparency = true;
            this.WindowStyle = WindowStyle.None;
            this.Topmost = true;
            this.WindowState = WindowState.Maximized;
        }

        private void QRCodeScannerWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
            SelectionRectangle.Visibility = Visibility.Visible;
            Canvas.SetLeft(SelectionRectangle, startPoint.X);
            Canvas.SetTop(SelectionRectangle, startPoint.Y);
            SelectionRectangle.Width = 0;
            SelectionRectangle.Height = 0;
        }

        private void QRCodeScannerWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point currentPoint = e.GetPosition(null);
                double x = Math.Min(startPoint.X, currentPoint.X);
                double y = Math.Min(startPoint.Y, currentPoint.Y);
                double width = Math.Abs(currentPoint.X - startPoint.X);
                double height = Math.Abs(currentPoint.Y - startPoint.Y);

                Canvas.SetLeft(SelectionRectangle, x);
                Canvas.SetTop(SelectionRectangle, y);
                SelectionRectangle.Width = width;
                SelectionRectangle.Height = height;
            }
        }

        private void QRCodeScannerWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point endPoint = e.GetPosition(null);
            CaptureAndScanQRCode(startPoint, endPoint);
            this.Close();
        }

        private void CaptureAndScanQRCode(System.Windows.Point startPoint, System.Windows.Point endPoint)
        {
            try
            {
                int x = (int)Math.Min(startPoint.X, endPoint.X);
                int y = (int)Math.Min(startPoint.Y, endPoint.Y);
                int width = (int)Math.Abs(endPoint.X - startPoint.X);
                int height = (int)Math.Abs(endPoint.Y - startPoint.Y);

                // Hide this window before capturing the screen
                this.Visibility = Visibility.Hidden;
                System.Threading.Thread.Sleep(100); // Give time for the window to hide

                // Capture the screen
                var screenBmp = CaptureScreen();

                // Show the window again
                this.Visibility = Visibility.Visible;

                // Crop the captured screen to the selected area
                var croppedBmp = new CroppedBitmap(screenBmp, new Int32Rect(x, y, width, height));

                // Convert to grayscale for better recognition
                var grayscaleBmp = ConvertToGrayscale(croppedBmp);

                BarcodeReader reader = new BarcodeReader
                {
                    Options = new DecodingOptions
                    {
                        TryHarder = true,
                        PossibleFormats = new[] { BarcodeFormat.QR_CODE }
                    }
                };

                Result result = reader.Decode(grayscaleBmp);

                if (result != null)
                {
                    MessageBox.Show($"QR Code content: {result.Text}", "QR Code Scanned", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No QR code found in the selected area.", "Scan Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CaptureAndScanQRCode: {ex.Message}");
                MessageBox.Show($"An error occurred while scanning: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private WriteableBitmap ConvertToGrayscale(BitmapSource source)
        {
            var stride = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;
            var pixels = new byte[stride * source.PixelHeight];
            source.CopyPixels(pixels, stride, 0);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                var gray = (byte)(0.299 * pixels[i + 2] + 0.587 * pixels[i + 1] + 0.114 * pixels[i]);
                pixels[i] = gray;
                pixels[i + 1] = gray;
                pixels[i + 2] = gray;
            }

            var grayscaleBitmap = new WriteableBitmap(source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY, PixelFormats.Bgr32, null);
            grayscaleBitmap.WritePixels(new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight), pixels, stride, 0);

            return grayscaleBitmap;
        }

        private BitmapSource CaptureScreen()
        {
            using (var screenBmp = new System.Drawing.Bitmap(
                (int)SystemParameters.PrimaryScreenWidth,
                (int)SystemParameters.PrimaryScreenHeight,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var g = System.Drawing.Graphics.FromImage(screenBmp))
                {
                    g.CopyFromScreen(0, 0, 0, 0, screenBmp.Size);
                }

                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    screenBmp.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
        }
    }
}
