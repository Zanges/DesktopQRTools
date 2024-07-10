using Microsoft.VisualStudio.TestTools.UnitTesting;
using DesktopQRTools;
using System.Windows;

namespace DesktopQRTools.Tests
{
    [TestClass]
    public class QRCodeResultWindowTests
    {
        [TestMethod]
        public void Constructor_ValidInput_SetsContentCorrectly()
        {
            // Arrange
            string testContent = "Test QR Code Content";

            // Act
            var window = new QRCodeResultWindow(testContent);

            // Assert
            Assert.AreEqual(testContent, window.ContentTextBlock.Text);
        }

        [TestMethod]
        public void Constructor_URLInput_ShowsOpenLinkButton()
        {
            // Arrange
            string testUrl = "https://www.example.com";

            // Act
            var window = new QRCodeResultWindow(testUrl);

            // Assert
            Assert.AreEqual(Visibility.Visible, window.OpenLinkButton.Visibility);
        }

        [TestMethod]
        public void Constructor_NonURLInput_HidesOpenLinkButton()
        {
            // Arrange
            string testContent = "Not a URL";

            // Act
            var window = new QRCodeResultWindow(testContent);

            // Assert
            Assert.AreEqual(Visibility.Collapsed, window.OpenLinkButton.Visibility);
        }
    }
}
