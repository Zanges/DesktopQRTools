using System;
using System.Windows.Media.Imaging;
using DesktopQRTools;
using ZXing;
using ZXing.QrCode;

namespace DesktopQRToolsTests
{
    [TestFixture]
    public class QRCodeTests
    {
        private QRCodeGeneratorWindow _generatorWindow;

        [SetUp]
        public void Setup()
        {
            _generatorWindow = new QRCodeGeneratorWindow();
        }

        [Test]
        public void TestQRCodeGeneration()
        {
            // Arrange
            string testContent = "https://example.com";

            // Act
            WriteableBitmap qrCode = _generatorWindow.GenerateQRCode(testContent);

            // Assert
            Assert.IsNotNull(qrCode, "QR code should not be null");
            Assert.AreEqual(300, qrCode.PixelWidth, "QR code width should be 300 pixels");
            Assert.AreEqual(300, qrCode.PixelHeight, "QR code height should be 300 pixels");

            // Decode the generated QR code
            BarcodeReader reader = new BarcodeReader
            {
                Options = new DecodingOptions
                {
                    PossibleFormats = new[] { BarcodeFormat.QR_CODE }
                }
            };
            Result result = reader.Decode(qrCode);

            Assert.IsNotNull(result, "Decoded result should not be null");
            Assert.AreEqual(testContent, result.Text, "Decoded content should match the original content");
        }

        [Test]
        public void TestQRCodeScanning()
        {
            // Arrange
            string testContent = "Test QR Code Content";
            WriteableBitmap qrCode = _generatorWindow.GenerateQRCode(testContent);

            // Act
            string scannedContent = ScanQRCode(qrCode);

            // Assert
            Assert.AreEqual(testContent, scannedContent, "Scanned content should match the original content");
        }

        private string ScanQRCode(WriteableBitmap qrCode)
        {
            BarcodeReader reader = new BarcodeReader
            {
                Options = new DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new[] { BarcodeFormat.QR_CODE }
                }
            };

            Result result = reader.Decode(qrCode);
            return result?.Text;
        }
    }
}
