using Microsoft.VisualStudio.TestTools.UnitTesting;
using DesktopQRTools;
using System.Windows.Media.Imaging;

namespace DesktopQRTools.Tests
{
    [TestClass]
    public class QRCodeGeneratorWindowTests
    {
        [TestMethod]
        public void GenerateQRCode_ValidInput_ReturnsImage()
        {
            // Arrange
            var window = new QRCodeGeneratorWindow();
            string testInput = "Test QR Code";

            // Act
            BitmapSource result = window.GenerateQRCode(testInput);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BitmapSource));
        }

        [TestMethod]
        public void GenerateQRCode_EmptyInput_ReturnsNull()
        {
            // Arrange
            var window = new QRCodeGeneratorWindow();
            string testInput = "";

            // Act
            BitmapSource result = window.GenerateQRCode(testInput);

            // Assert
            Assert.IsNull(result);
        }
    }
}
