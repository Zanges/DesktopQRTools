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
    public class QRCodeGenerationTests
    {
        private QRCodeGeneratorWindow? _generatorWindow;

        [SetUp]
        public void Setup()
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            dispatcher.Invoke(() =>
            {
                _generatorWindow = new QRCodeGeneratorWindow();
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
    }
}
