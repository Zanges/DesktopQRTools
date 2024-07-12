using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;
using System.Linq;
using DesktopQRTools;
using NUnit.Framework;

namespace DesktopQRToolsTests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class OptionsWindowTests
    {
        private OptionsWindow? _optionsWindow;

        [SetUp]
        public void Setup()
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            dispatcher.Invoke(() =>
            {
                _optionsWindow = new OptionsWindow();
            });
        }

        [Test]
        public void TestOptionsWindowInitialization()
        {
            Assert.That(_optionsWindow, Is.Not.Null, "Options window should be initialized");
            Assert.That(_optionsWindow!.Title, Is.EqualTo("Options"), "Window title should be 'Options'");
            Assert.That(_optionsWindow.GetOptionsCount(), Is.EqualTo(5), "Options window should have 5 options");
        }

        [Test]
        public void TestOptionsCount()
        {
            Assert.That(_optionsWindow, Is.Not.Null, "Options window should be initialized");
            Assert.That(_optionsWindow!.GetOptionsCount(), Is.EqualTo(5), "Options window should have 5 options");
        }

        [Test]
        public void TestAutoSaveQRCodeName()
        {
            Assert.That(_optionsWindow, Is.Not.Null, "Options window should be initialized");
            _optionsWindow!.Dispatcher.Invoke(() =>
            {
                var autoSaveQRCodeNameTextBox = _optionsWindow.FindName("AutoSaveQRCodeNameTextBox") as System.Windows.Controls.TextBox;
                Assert.That(autoSaveQRCodeNameTextBox, Is.Not.Null, "AutoSaveQRCodeNameTextBox should exist");
                autoSaveQRCodeNameTextBox!.Text = "TestQRCode";
            });
            Assert.That(_optionsWindow.GetAutoSaveQRCodeName(), Is.EqualTo("TestQRCode"), "Auto save QR code name should be set correctly");
        }

        [Test]
        public void TestSkipSaveDialog()
        {
            Assert.That(_optionsWindow, Is.Not.Null, "Options window should be initialized");
            _optionsWindow!.Dispatcher.Invoke(() =>
            {
                var skipSaveDialogCheckBox = _optionsWindow.FindName("SkipSaveDialogCheckBox") as System.Windows.Controls.CheckBox;
                Assert.That(skipSaveDialogCheckBox, Is.Not.Null, "SkipSaveDialogCheckBox should exist");
                skipSaveDialogCheckBox!.IsChecked = true;
            });
            Assert.That(_optionsWindow.GetSkipSaveDialog(), Is.True, "Skip save dialog should be set to true");
        }

        [Test]
        public void TestAutoSaveDirectory()
        {
            Assert.That(_optionsWindow, Is.Not.Null, "Options window should be initialized");
            _optionsWindow!.Dispatcher.Invoke(() =>
            {
                var autoSaveDirectoryTextBox = _optionsWindow.FindName("AutoSaveDirectoryTextBox") as System.Windows.Controls.TextBox;
                Assert.That(autoSaveDirectoryTextBox, Is.Not.Null, "AutoSaveDirectoryTextBox should exist");
                autoSaveDirectoryTextBox!.Text = @"C:\TestAutoSave";
            });
            Assert.That(_optionsWindow.GetAutoSaveDirectory(), Is.EqualTo(@"C:\TestAutoSave"), "Auto save directory should be set correctly");
        }

        [Test]
        public void TestAppendDate()
        {
            Assert.That(_optionsWindow, Is.Not.Null, "Options window should be initialized");
            _optionsWindow!.Dispatcher.Invoke(() =>
            {
                var appendDateCheckBox = _optionsWindow.FindName("AppendDateCheckBox") as System.Windows.Controls.CheckBox;
                Assert.That(appendDateCheckBox, Is.Not.Null, "AppendDateCheckBox should exist");
                appendDateCheckBox!.IsChecked = true;
            });
            Assert.That(_optionsWindow.GetAppendDate(), Is.True, "Append date should be set to true");
        }

        [Test]
        public void TestAppendTime()
        {
            Assert.That(_optionsWindow, Is.Not.Null, "Options window should be initialized");
            _optionsWindow!.Dispatcher.Invoke(() =>
            {
                var appendTimeCheckBox = _optionsWindow.FindName("AppendTimeCheckBox") as System.Windows.Controls.CheckBox;
                Assert.That(appendTimeCheckBox, Is.Not.Null, "AppendTimeCheckBox should exist");
                appendTimeCheckBox!.IsChecked = true;
            });
            Assert.That(_optionsWindow.GetAppendTime(), Is.True, "Append time should be set to true");
        }

        [Test]
        public void TestScannerMode()
        {
            Assert.That(_optionsWindow, Is.Not.Null, "Options window should be initialized");
            _optionsWindow!.Dispatcher.Invoke(() =>
            {
                var scannerModeComboBox = _optionsWindow.FindName("ScannerModeComboBox") as System.Windows.Controls.ComboBox;
                Assert.That(scannerModeComboBox, Is.Not.Null, "ScannerModeComboBox should exist");
                scannerModeComboBox!.SelectedIndex = 1; // Selecting TargetingRectangle
            });
            Assert.That(_optionsWindow.GetScannerMode(), Is.EqualTo(OptionsWindow.ScannerMode.TargetingRectangle), "Scanner mode should be set to TargetingRectangle");
        }

        [Test]
        public void TestSaveButtonClick()
        {
            Assert.That(_optionsWindow, Is.Not.Null, "Options window should be initialized");

            bool messageBoxShown = false;
            string? messageBoxText = null;

            _optionsWindow!.Dispatcher.Invoke(() =>
            {
                // Set some test values
                var autoSaveQRCodeNameTextBox = _optionsWindow.FindName("AutoSaveQRCodeNameTextBox") as System.Windows.Controls.TextBox;
                var skipSaveDialogCheckBox = _optionsWindow.FindName("SkipSaveDialogCheckBox") as System.Windows.Controls.CheckBox;
                var autoSaveDirectoryTextBox = _optionsWindow.FindName("AutoSaveDirectoryTextBox") as System.Windows.Controls.TextBox;
                var appendDateCheckBox = _optionsWindow.FindName("AppendDateCheckBox") as System.Windows.Controls.CheckBox;
                var appendTimeCheckBox = _optionsWindow.FindName("AppendTimeCheckBox") as System.Windows.Controls.CheckBox;
                var scannerModeComboBox = _optionsWindow.FindName("ScannerModeComboBox") as System.Windows.Controls.ComboBox;

                autoSaveQRCodeNameTextBox!.Text = "TestSaveQRCode";
                skipSaveDialogCheckBox!.IsChecked = true;
                autoSaveDirectoryTextBox!.Text = @"C:\TestSaveDirectory";
                appendDateCheckBox!.IsChecked = true;
                appendTimeCheckBox!.IsChecked = false;
                scannerModeComboBox!.SelectedIndex = 1; // TargetingRectangle

                // Override the default MessageBox.Show method
                MessageBoxManager.Register(() => new MessageBoxMock(result =>
                {
                    messageBoxShown = true;
                    messageBoxText = result.Item1;
                    return MessageBoxResult.OK;
                }));

                // Simulate clicking the Save button
                var saveButton = _optionsWindow.FindName("SaveButton") as System.Windows.Controls.Button;
                if (saveButton == null)
                {
                    // If we can't find it by name, try to find it by content
                    saveButton = _optionsWindow.FindChildren<System.Windows.Controls.Button>()
                        .FirstOrDefault(b => b.Content.ToString() == "Save");
                }
                Assert.That(saveButton, Is.Not.Null, "Save button should exist");
                saveButton!.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));

                // Restore the default MessageBox.Show method
                MessageBoxManager.Unregister();

                // Test button fit
                Assert.That(saveButton.ActualWidth, Is.LessThanOrEqualTo(_optionsWindow.ActualWidth),
                    "Save button width should not exceed window width");
                Assert.That(saveButton.ActualHeight, Is.LessThanOrEqualTo(_optionsWindow.ActualHeight),
                    "Save button height should not exceed window height");
            });

            Assert.That(messageBoxShown, Is.True, "Message box should be shown");
            Assert.That(messageBoxText, Is.EqualTo("Options saved successfully!"), "Message box should display correct text");

            // Verify that the options were actually saved
            Assert.That(_optionsWindow!.GetAutoSaveQRCodeName(), Is.EqualTo("TestSaveQRCode"), "Auto save QR code name should be saved");
            Assert.That(_optionsWindow.GetSkipSaveDialog(), Is.True, "Skip save dialog should be saved");
            Assert.That(_optionsWindow.GetAutoSaveDirectory(), Is.EqualTo(@"C:\TestSaveDirectory"), "Auto save directory should be saved");
            Assert.That(_optionsWindow.GetAppendDate(), Is.True, "Append date should be saved");
            Assert.That(_optionsWindow.GetAppendTime(), Is.False, "Append time should be saved");
            Assert.That(_optionsWindow.GetScannerMode(), Is.EqualTo(OptionsWindow.ScannerMode.TargetingRectangle), "Scanner mode should be saved");
        }
    }

    // Mock class for MessageBox
    public class MessageBoxMock : IMessageBox
    {
        private readonly Func<(string, string, MessageBoxButton, MessageBoxImage), MessageBoxResult> _showFunc;

        public MessageBoxMock(Func<(string, string, MessageBoxButton, MessageBoxImage), MessageBoxResult> showFunc)
        {
            _showFunc = showFunc;
        }

        public MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return _showFunc((messageBoxText, caption, button, icon));
        }
    }

    // Interface for MessageBox
    public interface IMessageBox
    {
        MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon);
    }

    // MessageBox manager
    public static class MessageBoxManager
    {
        private static IMessageBox? _messageBox;

        public static void Register(Func<IMessageBox> factory)
        {
            _messageBox = factory();
        }

        public static void Unregister()
        {
            _messageBox = null;
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            if (_messageBox != null)
            {
                return _messageBox.Show(messageBoxText, caption, button, icon);
            }
            return MessageBox.Show(messageBoxText, caption, button, icon);
        }
    }

    public static class WindowExtensions
    {
        public static IEnumerable<T> FindChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T)
                {
                    yield return (T)child;
                }

                foreach (T childOfChild in FindChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }
    }
}
