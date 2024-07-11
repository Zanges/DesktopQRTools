using System;
using System.Windows;
using System.Windows.Threading;
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
        }

        [Test]
        public void TestSaveButtonClick()
        {
            Assert.That(_optionsWindow, Is.Not.Null, "Options window should be initialized");

            bool messageBoxShown = false;
            string? messageBoxText = null;

            _optionsWindow!.Dispatcher.Invoke(() =>
            {
                // Override the default MessageBox.Show method
                MessageBoxManager.Register(() => new MessageBoxMock(result =>
                {
                    messageBoxShown = true;
                    messageBoxText = result.Item1;
                    return MessageBoxResult.OK;
                }));

                // Simulate clicking the Save button
                var saveButton = _optionsWindow.FindName("SaveButton") as System.Windows.Controls.Button;
                Assert.That(saveButton, Is.Not.Null, "Save button should exist");
                saveButton!.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));

                // Restore the default MessageBox.Show method
                MessageBoxManager.Unregister();
            });

            Assert.That(messageBoxShown, Is.True, "Message box should be shown");
            Assert.That(messageBoxText, Is.EqualTo("Options saved successfully!"), "Message box should display correct text");
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
}
