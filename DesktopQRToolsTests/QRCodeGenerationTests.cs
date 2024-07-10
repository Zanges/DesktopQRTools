using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DesktopQRTools;
using ZXing;
using ZXing.QrCode;
using ZXing.Common;
using ZXing.Windows.Compatibility;
using NUnit.Framework;

namespace DesktopQRToolsTests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class QRCodeGenerationTests
    {
        private QRCodeGeneratorWindow? _generatorWindow;
        private string _testConfigPath;

        [SetUp]
        public void Setup()
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            dispatcher.Invoke(() =>
            {
                _generatorWindow = new QRCodeGeneratorWindow();
            });

            // Create a test config file
            _testConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_config.ini");
            File.WriteAllText(_testConfigPath, 
                "AutoSaveQRCodeName=TestQRCode\n" +
                "SkipSaveDialog=true\n" +
                "AutoSaveDirectory=TestDir\n" +
                "AppendDate=true\n" +
                "AppendTime=true");
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up the test config file
            if (File.Exists(_testConfigPath))
            {
                File.Delete(_testConfigPath);
            }
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
        public void TestAutoSaveConfiguration()
        {
            // Arrange
            string testContent = "https://example.com";
            string expectedFileName = $"TestQRCode-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmmss}.png";
            string expectedFilePath = Path.Combine("TestDir", expectedFileName);

            // Act
            _generatorWindow.GenerateQRCode(testContent);
            
            // We need to mock the file saving process here
            // For now, we'll just check if the configuration is read correctly

            // Assert
            Assert.That(_generatorWindow.GetAutoSaveQRCodeName(), Is.EqualTo("TestQRCode"), "Auto-save QR code name should match the config");
            Assert.That(_generatorWindow.GetSkipSaveDialog(), Is.True, "Skip save dialog should be true");
            Assert.That(_generatorWindow.GetAutoSaveDirectory(), Is.EqualTo("TestDir"), "Auto-save directory should match the config");
            Assert.That(_generatorWindow.GetAppendDate(), Is.True, "Append date should be true");
            Assert.That(_generatorWindow.GetAppendTime(), Is.True, "Append time should be true");
        }
    }
}
