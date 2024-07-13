using System;
using System.IO;
using DesktopQRTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DesktopQRToolsTests
{
    [TestClass]
    public class MiscTests
    {
        private const string TestConfigFileName = "testconfig.ini";

        [TestMethod]
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
    }
}
