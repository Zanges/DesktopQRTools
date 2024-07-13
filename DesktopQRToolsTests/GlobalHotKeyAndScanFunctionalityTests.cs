using System;
using System.Windows.Input;
using DesktopQRTools;
using NUnit.Framework;

namespace DesktopQRToolsTests
{
    [TestFixture]
    public class GlobalHotKeyAndScanFunctionalityTests
    {
        [Test]
        public void TestGlobalHotKeyModifiersToUInt()
        {
            // Test various combinations of modifier keys
            Assert.AreEqual(0x0001U, GlobalHotKey.ModifiersToUInt(ModifierKeys.Alt));
            Assert.AreEqual(0x0002U, GlobalHotKey.ModifiersToUInt(ModifierKeys.Control));
            Assert.AreEqual(0x0004U, GlobalHotKey.ModifiersToUInt(ModifierKeys.Shift));
            Assert.AreEqual(0x0008U, GlobalHotKey.ModifiersToUInt(ModifierKeys.Windows));
            Assert.AreEqual(0x0003U, GlobalHotKey.ModifiersToUInt(ModifierKeys.Alt | ModifierKeys.Control));
            Assert.AreEqual(0x000FU, GlobalHotKey.ModifiersToUInt(ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Windows));
        }

        // Test wrapper class
        private class TestWrapper : QRCodeScannerWindow
        {
            public static bool ShowNewCalled { get; private set; }

            public static new void ShowNew()
            {
                ShowNewCalled = true;
            }
        }

        [Test]
        public void TestQRCodeScanFunctionalityTrigger()
        {
            // Reset the flag
            TestWrapper.ShowNewCalled = false;

            // Call the TriggerQRScan method
            QRCodeScanFunctionality.TriggerQRScan();

            // Assert that ShowNew was called
            Assert.IsTrue(TestWrapper.ShowNewCalled, "QRCodeScannerWindow.ShowNew() should have been called");
        }
    }
}
