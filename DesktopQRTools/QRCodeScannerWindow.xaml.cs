using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Shapes;
using ZXing;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;
using ZXing.Common;
using System.Windows.Interop;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace DesktopQRTools
{
    /// <summary>
    /// Interaction logic for QRCodeScannerWindow.xaml
    /// </summary>
    public partial class QRCodeScannerWindow : Window
    {
        private System.Windows.Point startPoint;
        private OptionsWindow.ScannerMode scannerMode;
        private Rectangle targetingRectangle;
        private const double MinRectangleSize = 50;
        private const double MaxRectangleSize = 400;
        private const double RectangleSizeStep = 10;

        public QRCodeScannerWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += QRCodeScannerWindow_MouseLeftButtonDown;
            this.MouseLeftButtonUp += QRCodeScannerWindow_MouseLeftButtonUp;
            this.MouseMove += QRCodeScannerWindow_MouseMove;
            this.KeyDown += QRCodeScannerWindow_KeyDown;
            this.MouseWheel += QRCodeScannerWindow_MouseWheel;

            // Make the window transparent but not click-through
            this.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            this.AllowsTransparency = true;
            this.WindowStyle = WindowStyle.None;
            this.Topmost = true;
            this.WindowState = WindowState.Maximized;

            // Get the scanner mode from options
            var optionsWindow = new OptionsWindow();
            scannerMode = optionsWindow.GetScannerMode();

            targetingRectangle = new Rectangle();

            if (scannerMode == OptionsWindow.ScannerMode.TargetingRectangle)
            {
                InitializeTargetingRectangle();
            }
            else if (scannerMode == OptionsWindow.ScannerMode.AutomaticDetection)
            {
                AutomaticScan();
            }

            UpdateInstructionsText();
        }

        public static void ShowNew()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var newWindow = new QRCodeScannerWindow();
                    newWindow.Closed += (sender, args) => newWindow = null;
                    newWindow.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while opening the QR Code Scanner: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private void InitializeTargetingRectangle()
        {
            targetingRectangle = new Rectangle
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                Width = 200,
                Height = 200
            };
            Canvas.SetLeft(targetingRectangle, 0);
            Canvas.SetTop(targetingRectangle, 0);
            ((Canvas)this.Content).Children.Add(targetingRectangle);
        }

        private void QRCodeScannerWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (scannerMode == OptionsWindow.ScannerMode.TargetingRectangle)
            {
                double delta = e.Delta > 0 ? RectangleSizeStep : -RectangleSizeStep;
                ResizeTargetingRectangle(delta);
            }
        }

        private void ResizeTargetingRectangle(double delta)
        {
            double newSize = targetingRectangle.Width + delta;
            newSize = Math.Max(MinRectangleSize, Math.Min(MaxRectangleSize, newSize));

            double centerX = Canvas.GetLeft(targetingRectangle) + targetingRectangle.Width / 2;
            double centerY = Canvas.GetTop(targetingRectangle) + targetingRectangle.Height / 2;

            targetingRectangle.Width = newSize;
            targetingRectangle.Height = newSize;

            Canvas.SetLeft(targetingRectangle, centerX - newSize / 2);
            Canvas.SetTop(targetingRectangle, centerY - newSize / 2);
        }

        private void UpdateInstructionsText()
        {
            switch (scannerMode)
            {
                case OptionsWindow.ScannerMode.DrawBox:
                    InstructionsTextBlock.Text = "Click and drag to select the area containing the QR code";
                    break;
                case OptionsWindow.ScannerMode.TargetingRectangle:
                    InstructionsTextBlock.Text = "Move the targeting rectangle over the QR code. Use the scroll wheel to resize.";
                    break;
                case OptionsWindow.ScannerMode.AutomaticDetection:
                    InstructionsTextBlock.Text = "Scanning for QR codes automatically...";
                    break;
            }
        }

        private void AutomaticScan()
        {
            var screenBmp = CaptureScreen();
            var result = ScanQRCode(ConvertToGrayscale(screenBmp));
            if (result != null)
            {
                DisplayResult(result);
            }
            else
            {
                ShowNoQRCodeFoundMessage();
            }
        }

        private void ShowNoQRCodeFoundMessage()
        {
            MessageBox.Show("No QR code found on the screen.", "Scan Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            Application.Current.Dispatcher.InvokeAsync(() => this.Close());
        }

        private void QRCodeScannerWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void QRCodeScannerWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
            if (scannerMode == OptionsWindow.ScannerMode.DrawBox)
            {
                SelectionRectangle.Visibility = Visibility.Visible;
                Canvas.SetLeft(SelectionRectangle, startPoint.X);
                Canvas.SetTop(SelectionRectangle, startPoint.Y);
                SelectionRectangle.Width = 0;
                SelectionRectangle.Height = 0;
            }
        }

        private void QRCodeScannerWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (scannerMode == OptionsWindow.ScannerMode.TargetingRectangle)
            {
                var currentPoint = e.GetPosition(null);
                double left = currentPoint.X - targetingRectangle.Width / 2;
                double top = currentPoint.Y - targetingRectangle.Height / 2;

                // Ensure the rectangle stays within the window bounds
                left = Math.Max(0, Math.Min(left, this.ActualWidth - targetingRectangle.Width));
                top = Math.Max(0, Math.Min(top, this.ActualHeight - targetingRectangle.Height));

                Canvas.SetLeft(targetingRectangle, left);
                Canvas.SetTop(targetingRectangle, top);
            }
            else if (scannerMode == OptionsWindow.ScannerMode.DrawBox && e.LeftButton == MouseButtonState.Pressed)
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
            if (scannerMode == OptionsWindow.ScannerMode.DrawBox)
            {
                CaptureAndScanQRCode(startPoint, endPoint);
            }
            else if (scannerMode == OptionsWindow.ScannerMode.TargetingRectangle)
            {
                double x = Canvas.GetLeft(targetingRectangle);
                double y = Canvas.GetTop(targetingRectangle);
                CaptureAndScanQRCode(new Point(x, y), new Point(x + targetingRectangle.Width, y + targetingRectangle.Height));
            }
        }

        private void ResetUI()
        {
            SelectionRectangle.Visibility = Visibility.Collapsed;
            InstructionsTextBlock.Visibility = Visibility.Visible;
        }

        public string? CaptureAndScanQRCode(System.Windows.Point startPoint, System.Windows.Point endPoint)
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
                WriteableBitmap writableBmp = new WriteableBitmap(screenBmp);

                // Show the window again
                this.Visibility = Visibility.Visible;

                WriteableBitmap bitmapToScan;
                if (scannerMode == OptionsWindow.ScannerMode.AutomaticDetection)
                {
                    bitmapToScan = new WriteableBitmap(screenBmp);
                }
                else
                {
                    // Crop the captured screen to the selected area
                    var croppedBmp = new CroppedBitmap(screenBmp, new Int32Rect(x, y, width, height));
                    bitmapToScan = ConvertToGrayscale(croppedBmp);
                }

                var result = ScanQRCode(bitmapToScan);
                if (result != null)
                {
                    DisplayResult(result);
                }
                else
                {
                    ShowNoQRCodeFoundMessage();
                }
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CaptureAndScanQRCode: {ex.Message}");
                MessageBox.Show($"An error occurred while scanning: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public string? CaptureAndScanQRCode(System.Windows.Point startPoint, System.Windows.Point endPoint, WriteableBitmap testBitmap)
        {
            try
            {
                int x = (int)Math.Min(startPoint.X, endPoint.X);
                int y = (int)Math.Min(startPoint.Y, endPoint.Y);
                int width = (int)Math.Abs(endPoint.X - startPoint.X);
                int height = (int)Math.Abs(endPoint.Y - startPoint.Y);

                // Crop the test bitmap to the selected area
                var croppedBmp = new CroppedBitmap(testBitmap, new Int32Rect(x, y, width, height));

                // Convert to grayscale for better recognition
                var grayscaleBmp = ConvertToGrayscale(croppedBmp);

                return ScanQRCode(grayscaleBmp);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CaptureAndScanQRCode: {ex.Message}");
                MessageBox.Show($"An error occurred while scanning: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private string? ScanQRCode(WriteableBitmap bitmap)
        {
            BarcodeReader reader = new BarcodeReader
            {
                Options = new DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new[] { BarcodeFormat.QR_CODE }
                }
            };

            Result result = reader.Decode(bitmap);

            return result?.Text;
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

        private void DisplayResult(string content)
        {
            QRCodeResultWindow resultWindow = new QRCodeResultWindow(content);
            resultWindow.Show();
            this.Close();
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
                    BitmapSizeOptions.FromEmptyOptions())!;
            }
        }
    }
}
