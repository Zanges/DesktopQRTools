using System;
using System.Windows.Input;
using DesktopQRTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DesktopQRToolsTests
{
    [TestClass]
    public class GlobalHotKeyAndScanFunctionalityTests
    {
        [TestMethod]
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

        [TestMethod]
        public void TestQRCodeScanFunctionalityTrigger()
        {
            // This test verifies that the TriggerQRScan method calls QRCodeScannerWindow.ShowNew()
            // We can't directly test the static method, so we'll use a wrapper class for testing
            
            bool showNewCalled = false;
            
            // Create a test wrapper that overrides the ShowNew method
            class TestWrapper : QRCodeScannerWindow
            {
                public static new void ShowNew()
                {
                    showNewCalled = true;
                }
            }

            // Call the TriggerQRScan method
            QRCodeScanFunctionality.TriggerQRScan();

            // Assert that ShowNew was called
            Assert.IsTrue(showNewCalled, "QRCodeScannerWindow.ShowNew() should have been called");
        }
    }
}
