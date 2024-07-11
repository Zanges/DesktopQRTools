using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace DesktopQRTools
{
    public class GlobalHotKey : IDisposable
    {
        private static int _currentId;
        private const int HOTKEY_MODIFIERS_ALT = 0x0001;
        private const int HOTKEY_MODIFIERS_CONTROL = 0x0002;
        private const int HOTKEY_MODIFIERS_SHIFT = 0x0004;
        private const int HOTKEY_MODIFIERS_WIN = 0x0008;

        private readonly int _id;
        private readonly IntPtr _hWnd;
        private readonly uint _modifiers;
        private readonly uint _key;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public GlobalHotKey(ModifierKeys modifiers, Key key, IntPtr hWnd)
        {
            _id = ++_currentId;
            _hWnd = hWnd;
            _modifiers = (uint)modifiers;
            _key = (uint)KeyInterop.VirtualKeyFromKey(key);

            if (!RegisterHotKey(_hWnd, _id, _modifiers, _key))
            {
                throw new InvalidOperationException("Couldn't register the hot key.");
            }
        }

        public void Dispose()
        {
            UnregisterHotKey(_hWnd, _id);
        }

        public static uint ModifiersToUInt(ModifierKeys modifiers)
        {
            uint result = 0;
            if (modifiers.HasFlag(ModifierKeys.Alt)) result |= HOTKEY_MODIFIERS_ALT;
            if (modifiers.HasFlag(ModifierKeys.Control)) result |= HOTKEY_MODIFIERS_CONTROL;
            if (modifiers.HasFlag(ModifierKeys.Shift)) result |= HOTKEY_MODIFIERS_SHIFT;
            if (modifiers.HasFlag(ModifierKeys.Windows)) result |= HOTKEY_MODIFIERS_WIN;
            return result;
        }
    }
}
