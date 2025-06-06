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
        private string? _testConfigPath;

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

        [Test]
        public void TestAutoSaveFileNameGeneration()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var configPath = "C:\\TestConfig\\config.ini";
            mockFileSystem.AddFile(configPath, new MockFileData(
                "AutoSaveQRCodeName=TestQR\n" +
                "SkipSaveDialog=true\n" +
                "AutoSaveDirectory=C:\\TestSaveDir\n" +
                "AppendDate=true\n" +
                "AppendTime=true\n"
            ));

            var window = new QRCodeGeneratorWindow(configPath, mockFileSystem);
            window.LoadConfiguration(configPath); // Explicitly load the configuration

            // Act
            string fileName = window.GetAutoSaveFileName();

            // Assert
            Console.WriteLine($"Generated file name: {fileName}");
            Console.WriteLine($"Auto-save QR code name: {window.GetAutoSaveQRCodeName()}");
            Console.WriteLine($"Skip save dialog: {window.GetSkipSaveDialog()}");
            Console.WriteLine($"Auto-save directory: {window.GetAutoSaveDirectory()}");
            Console.WriteLine($"Append date: {window.GetAppendDate()}");
            Console.WriteLine($"Append time: {window.GetAppendTime()}");

            Assert.That(fileName, Does.StartWith("TestQR-"), "File name should start with TestQR-");
            Assert.That(fileName, Does.EndWith(".png"), "File name should end with .png");
            Assert.That(fileName, Does.Contain(DateTime.Now.ToString("yyyyMMdd")), "File name should contain the current date");
            Assert.That(fileName, Does.Contain(DateTime.Now.ToString("HHmmss")), "File name should contain the current time");
            Assert.That(fileName, Does.Match(@"TestQR-\d{8}-\d{6}\.png"), "Generated file name should match expected format");

            Assert.That(window.GetAutoSaveQRCodeName(), Is.EqualTo("TestQR"), "Auto-save QR code name should match the config");
            Assert.That(window.GetSkipSaveDialog(), Is.True, "Skip save dialog should be true");
            Assert.That(window.GetAutoSaveDirectory(), Is.EqualTo("C:\\TestSaveDir"), "Auto-save directory should match the config");
            Assert.That(window.GetAppendDate(), Is.True, "Append date should be true");
            Assert.That(window.GetAppendTime(), Is.True, "Append time should be true");
        }

        [Test]
        public void TestAutoSaveFileNameGenerationWithDifferentSettings()
        {
            // Arrange
            var mockFileSystem = new MockFileSystem();
            var configPath = "C:\\TestConfig\\config2.ini";
            mockFileSystem.AddFile(configPath, new MockFileData(
                "AutoSaveQRCodeName=DifferentQR\n" +
                "SkipSaveDialog=true\n" +
                "AutoSaveDirectory=C:\\AnotherSaveDir\n" +
                "AppendDate=false\n" +
                "AppendTime=false\n"
            ));

            var window = new QRCodeGeneratorWindow(configPath, mockFileSystem);
            
            // Ensure configuration is loaded
            window.LoadConfiguration(configPath);

            // Verify configuration is loaded correctly
            Assert.That(window.GetAutoSaveQRCodeName(), Is.EqualTo("DifferentQR"), "Auto-save QR code name should match the config");
            Assert.That(window.GetSkipSaveDialog(), Is.True, "Skip save dialog should be true");
            Assert.That(window.GetAutoSaveDirectory(), Is.EqualTo("C:\\AnotherSaveDir"), "Auto-save directory should match the config");
            Assert.That(window.GetAppendDate(), Is.False, "Append date should be false");
            Assert.That(window.GetAppendTime(), Is.False, "Append time should be false");

            // Act
            string fileName = window.GetAutoSaveFileName();

            // Assert
            Assert.That(fileName, Is.EqualTo("DifferentQR.png"), "File name should be exactly 'DifferentQR.png'");
            Assert.That(fileName, Does.Not.Contain(DateTime.Now.ToString("yyyyMMdd")), "File name should not contain the current date");
            Assert.That(fileName, Does.Not.Contain(DateTime.Now.ToString("HHmmss")), "File name should not contain the current time");
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
    }
}
