using System;
using System.Windows.Media.Imaging;
using DesktopQRTools;
using ZXing;
using ZXing.QrCode;
using ZXing.Common;

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
            Assert.That(qrCode, Is.Not.Null, "QR code should not be null");
            Assert.That(qrCode.PixelWidth, Is.EqualTo(300), "QR code width should be 300 pixels");
            Assert.That(qrCode.PixelHeight, Is.EqualTo(300), "QR code height should be 300 pixels");

            // Decode the generated QR code
            BarcodeReader<WriteableBitmap> reader = new BarcodeReader<WriteableBitmap>(null, null, null)
            {
                Options = new DecodingOptions
                {
                    PossibleFormats = new[] { BarcodeFormat.QR_CODE }
                }
            };
            Result result = reader.Decode(qrCode);

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
            string? scannedContent = ScanQRCode(qrCode);

            // Assert
            Assert.That(scannedContent, Is.EqualTo(testContent), "Scanned content should match the original content");
        }

        private string? ScanQRCode(WriteableBitmap qrCode)
        {
            BarcodeReader<WriteableBitmap> reader = new BarcodeReader<WriteableBitmap>(null, null, null)
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
