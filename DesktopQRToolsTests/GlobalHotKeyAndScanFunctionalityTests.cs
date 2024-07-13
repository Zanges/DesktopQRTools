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
            public static bool ShowNewCalled { get; set; }

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

            // Replace the actual QRCodeScannerWindow.ShowNew with our test wrapper
            var originalShowNew = typeof(QRCodeScannerWindow).GetMethod("ShowNew", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var wrapperShowNew = typeof(TestWrapper).GetMethod("ShowNew", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            try
            {
                System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(originalShowNew.MethodHandle);
                System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(wrapperShowNew.MethodHandle);

                // Replace the original method with our wrapper
                MethodHelper.SwapMethods(originalShowNew, wrapperShowNew);

                // Call the TriggerQRScan method
                QRCodeScanFunctionality.TriggerQRScan();

                // Assert that ShowNew was called
                Assert.IsTrue(TestWrapper.ShowNewCalled, "QRCodeScannerWindow.ShowNew() should have been called");
            }
            finally
            {
                // Restore the original method
                MethodHelper.SwapMethods(wrapperShowNew, originalShowNew);
            }
        }
    }

    public static class MethodHelper
    {
        public static void SwapMethods(System.Reflection.MethodInfo method1, System.Reflection.MethodInfo method2)
        {
            unsafe
            {
                ulong* methodDescriptor1 = (ulong*)method1.MethodHandle.Value.ToPointer();
                ulong* methodDescriptor2 = (ulong*)method2.MethodHandle.Value.ToPointer();

                ulong temp = *methodDescriptor1;
                *methodDescriptor1 = *methodDescriptor2;
                *methodDescriptor2 = temp;
            }
        }
    }
}
