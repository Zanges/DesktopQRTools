using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DesktopQRTools;
using ZXing;
using ZXing.QrCode;
using ZXing.Common;
using ZXing.Windows.Compatibility;
using NUnit.Framework;
using Moq;

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
            // Create a test config file
            _testConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_config.ini");
            File.WriteAllText(_testConfigPath, 
                "AutoSaveQRCodeName=TestQRCode\n" +
                "SkipSaveDialog=true\n" +
                "AutoSaveDirectory=TestDir\n" +
                "AppendDate=true\n" +
                "AppendTime=true");

            var dispatcher = Dispatcher.CurrentDispatcher;
            dispatcher.Invoke(() =>
            {
                _generatorWindow = new QRCodeGeneratorWindow(_testConfigPath);
            });
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
            var mockFileSystem = new MockFileSystem();
            var testContent = "https://example.com";
            var expectedFileName = $"TestQRCode-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmmss}.png";
            var expectedFilePath = Path.Combine("TestDir", expectedFileName);

            // Create QRCodeGeneratorWindow with mock file system
            var qrCodeGeneratorWindow = new QRCodeGeneratorWindow(_testConfigPath, mockFileSystem);

            // Act
            qrCodeGeneratorWindow.GenerateQRCode(testContent);
            qrCodeGeneratorWindow.SaveQRCode();

            // Assert
            Assert.That(qrCodeGeneratorWindow.GetAutoSaveQRCodeName(), Is.EqualTo("TestQRCode"), "Auto-save QR code name should match the config");
            Assert.That(qrCodeGeneratorWindow.GetSkipSaveDialog(), Is.True, "Skip save dialog should be true");
            Assert.That(qrCodeGeneratorWindow.GetAutoSaveDirectory(), Is.EqualTo("TestDir"), "Auto-save directory should match the config");
            Assert.That(qrCodeGeneratorWindow.GetAppendDate(), Is.True, "Append date should be true");
            Assert.That(qrCodeGeneratorWindow.GetAppendTime(), Is.True, "Append time should be true");

            // Check if the file was "saved" in the mock file system
            var savedFiles = mockFileSystem.AllFiles.Where(f => f.StartsWith(Path.Combine("TestDir", "TestQRCode"))).ToList();
            Assert.That(savedFiles, Is.Not.Empty, "QR code file should be saved");
            Assert.That(savedFiles[0], Does.Match(@"TestDir\\TestQRCode-\d{8}-\d{6}\.png"), "Saved file should match expected format");
        }
    }
}
