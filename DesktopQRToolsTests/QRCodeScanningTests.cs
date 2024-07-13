using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DesktopQRTools;

namespace DesktopQRToolsTests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class QRCodeScanningTests
    {
        private QRCodeGeneratorWindow _generatorWindow = null!;
        private QRCodeScannerWindow _scannerWindow = null!;

        [SetUp]
        public void Setup()
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            dispatcher.Invoke(() =>
            {
                _generatorWindow = new QRCodeGeneratorWindow(configPath: null);
                _scannerWindow = new QRCodeScannerWindow();
            });
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
                if (scannedContent != null)
                {
                    var resultWindow = new QRCodeResultWindow(scannedContent);
                    isOpenLinkButtonVisible = resultWindow.IsLinkButtonVisible();
                }
            });

            // Assert
            Assert.That(scannedContent, Is.EqualTo(testUrl), "Scanned content should match the original URL");
            Assert.That(isOpenLinkButtonVisible, Is.True, "Open Link button should be visible for a URL");
        }
    }
}
