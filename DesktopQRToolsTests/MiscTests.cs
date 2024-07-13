using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using DesktopQRTools;
using NUnit.Framework;

namespace DesktopQRToolsTests
{
    [TestFixture]
    public class MiscTests
    {
        private const string TestConfigFileName = "testconfig.ini";

        [Test]
        public void TestConfigFileAutoGeneration()
        {
            // Arrange
            string testConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TestConfigFileName);
            if (File.Exists(testConfigPath))
            {
                File.Delete(testConfigPath);
            }

            // Act
            App app = new App();
            app.EnsureConfigFileExists(TestConfigFileName);

            // Assert
            Assert.IsTrue(File.Exists(testConfigPath), "Config file should be created");

            string[] configLines = File.ReadAllLines(testConfigPath);
            Assert.AreEqual(8, configLines.Length, "Config file should have 8 lines");

            Assert.IsTrue(configLines[0].StartsWith("AutoSaveQRCodeName="), "AutoSaveQRCodeName should be set");
            Assert.IsTrue(configLines[1].StartsWith("SkipSaveDialog="), "SkipSaveDialog should be set");
            Assert.IsTrue(configLines[2].StartsWith("AutoSaveDirectory="), "AutoSaveDirectory should be set");
            Assert.IsTrue(configLines[3].StartsWith("AppendDate="), "AppendDate should be set");
            Assert.IsTrue(configLines[4].StartsWith("AppendTime="), "AppendTime should be set");
            Assert.IsTrue(configLines[5].StartsWith("ScannerMode="), "ScannerMode should be set");
            Assert.IsTrue(configLines[6].StartsWith("ScanHotkey="), "ScanHotkey should be set");
            Assert.IsTrue(configLines[7].StartsWith("ScanHotkeyModifiers="), "ScanHotkeyModifiers should be set");

            // Clean up
            File.Delete(testConfigPath);
        }

        [Test]
        [STAThread]
        public void TestQRCodeGeneratorWindowAutoSaveFileName()
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

            // Act
            string fileName = window.GetAutoSaveFileName();

            // Assert
            Assert.IsTrue(fileName.StartsWith("TestQR-"), "File name should start with TestQR-");
            Assert.IsTrue(fileName.EndsWith(".png"), "File name should end with .png");
            Assert.IsTrue(fileName.Contains(DateTime.Now.ToString("yyyyMMdd")), "File name should contain the current date");
            Assert.IsTrue(fileName.Contains(DateTime.Now.ToString("HHmmss")), "File name should contain the current time");
        }
    }
}
