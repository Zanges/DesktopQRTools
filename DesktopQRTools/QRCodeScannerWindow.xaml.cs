using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Windows.Compatibility;
using System.Windows.Interop;

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
        }

        private void QRCodeScannerWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(this);
            SelectionRectangle.Visibility = Visibility.Visible;
        }

        private void QRCodeScannerWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point currentPoint = e.GetPosition(this);
                double x = Math.Min(startPoint.X, currentPoint.X);
                double y = Math.Min(startPoint.Y, currentPoint.Y);
                double width = Math.Abs(currentPoint.X - startPoint.X);
                double height = Math.Abs(currentPoint.Y - startPoint.Y);

                SelectionRectangle.Margin = new Thickness(x, y, 0, 0);
                SelectionRectangle.Width = width;
                SelectionRectangle.Height = height;
            }
        }

        private void QRCodeScannerWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point endPoint = e.GetPosition(this);
            CaptureAndScanQRCode(startPoint, endPoint);
            this.Close();
        }

        private void CaptureAndScanQRCode(System.Windows.Point startPoint, System.Windows.Point endPoint)
        {
            int x = (int)Math.Min(startPoint.X, endPoint.X);
            int y = (int)Math.Min(startPoint.Y, endPoint.Y);
            int width = (int)Math.Abs(endPoint.X - startPoint.X);
            int height = (int)Math.Abs(endPoint.Y - startPoint.Y);

            // Capture the entire screen
            var screenBmp = CaptureScreen();

            // Crop the captured screen to the selected area
            var croppedBmp = new CroppedBitmap(screenBmp, new Int32Rect(x, y, width, height));

            BarcodeReader reader = new BarcodeReader();
            Result result = reader.Decode(croppedBmp);

            if (result != null)
            {
                MessageBox.Show($"QR Code content: {result.Text}", "QR Code Scanned", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("No QR code found in the selected area.", "Scan Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
