using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DesktopQRTools;
using ZXing;
using ZXing.QrCode;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace DesktopQRToolsTests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class QRCodeTests
    {
        private QRCodeGeneratorWindow? _generatorWindow;
        private QRCodeScannerWindow? _scannerWindow;

        [SetUp]
        public void Setup()
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            dispatcher.Invoke(() =>
            {
                _generatorWindow = new QRCodeGeneratorWindow();
                _scannerWindow = new QRCodeScannerWindow();
            });
        }

        [Test]
        public void TestQRCodeGeneration()
        {
            // Arrange
            string testContent = "https://example.com";

            // Act
            WriteableBitmap qrCode = _generatorWindow.GenerateQRCode(testContent);

            // Assert
            Assert.That(qrCode, Is.Not.Null, "QR code should not be null");
            Assert.That(qrCode.PixelWidth, Is.EqualTo(300), "QR code width should be 300 pixels");
            Assert.That(qrCode.PixelHeight, Is.EqualTo(300), "QR code height should be 300 pixels");

            // Decode the generated QR code
            var reader = new BarcodeReaderGeneric
            {
                Options = new DecodingOptions
                {
                    PossibleFormats = new[] { BarcodeFormat.QR_CODE }
                }
            };

            BitmapSource bitmapSource = qrCode;
            var width = bitmapSource.PixelWidth;
            var height = bitmapSource.PixelHeight;
            var stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
            var pixels = new byte[height * stride];
            bitmapSource.CopyPixels(pixels, stride, 0);

            var luminanceSource = new RGBLuminanceSource(pixels, width, height, RGBLuminanceSource.BitmapFormat.BGR32);
            var binarizer = new HybridBinarizer(luminanceSource);
            var binaryBitmap = new BinaryBitmap(binarizer);

            Result result = reader.Decode(luminanceSource);

            Assert.That(result, Is.Not.Null, "Decoded result should not be null");
            Assert.That(result.Text, Is.EqualTo(testContent), "Decoded content should match the original content");
        }

        [Test]
        public void TestQRCodeScanning()
        {
            // Arrange
            string testContent = "Test QR Code Content";
            WriteableBitmap qrCode = _generatorWindow.GenerateQRCode(testContent);

            // Act
            string? scannedContent = null;
            _scannerWindow.Dispatcher.Invoke(() =>
            {
                var startPoint = new Point(0, 0);
                var endPoint = new Point(qrCode.PixelWidth, qrCode.PixelHeight);
                scannedContent = _scannerWindow.CaptureAndScanQRCode(startPoint, endPoint, qrCode);
            });

            // Assert
            Assert.That(scannedContent, Is.EqualTo(testContent), "Scanned content should match the original content");
        }

        [Test]
        public void TestURLRecognitionAndOpenButton()
        {
            // Arrange
            string testUrl = "https://example.com";
            WriteableBitmap qrCode = _generatorWindow.GenerateQRCode(testUrl);

            // Act
            string? scannedContent = null;
            bool isOpenLinkButtonVisible = false;
            _scannerWindow.Dispatcher.Invoke(() =>
            {
                var startPoint = new Point(0, 0);
                var endPoint = new Point(qrCode.PixelWidth, qrCode.PixelHeight);
                scannedContent = _scannerWindow.CaptureAndScanQRCode(startPoint, endPoint, qrCode);

                // Create and show the QRCodeResultWindow
                var resultWindow = new QRCodeResultWindow(scannedContent);
                isOpenLinkButtonVisible = resultWindow.OpenLinkButton.Visibility == Visibility.Visible;
            });

            // Assert
            Assert.That(scannedContent, Is.EqualTo(testUrl), "Scanned content should match the original URL");
            Assert.That(isOpenLinkButtonVisible, Is.True, "Open Link button should be visible for a URL");
        }
    }
}
