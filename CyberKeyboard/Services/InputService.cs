using System;
using System.Runtime.InteropServices;
using CyberKeyboard.Native;

namespace CyberKeyboard.Services
{
    public class InputService
    {
        private readonly AudioService _audioService;

        public bool IsShiftActive { get; set; }
        public bool IsCapsLockActive { get; private set; }
        public bool IsAltGrActive { get; set; }
        public bool IsCtrlActive { get; set; }
        public bool IsAltActive { get; set; }
        public bool IsWinActive { get; set; }

        public event Action? ModifiersChanged;

        public InputService(AudioService audioService)
        {
            _audioService = audioService;
            UpdateCapsLockState();
        }

        public void UpdateCapsLockState()
        {
            short state = NativeMethods.GetKeyState(NativeMethods.VK_CAPITAL);
            IsCapsLockActive = (state & 0x0001) != 0;
            ModifiersChanged?.Invoke();
        }

        private char _lastChar = '\0';
        private long _lastCharTick = 0;
        private byte _lastVk = 0;
        private long _lastVkTick = 0;

        public void SendChar(char c)
        {
            long now = Environment.TickCount64;
            if (c == _lastChar && (now - _lastCharTick) < 100)
            {
                return; // Drop duplicate bounce
            }
            _lastChar = c;
            _lastCharTick = now;

            _audioService.PlayKeySound(false);

            // Handle modifier states if needed
            bool hadCtrl = IsCtrlActive;
            bool hadAlt = IsAltActive;
            bool hadWin = IsWinActive;

            // If Ctrl or Alt or Win is active, we might be triggering shortcuts like Ctrl+C
            if (hadCtrl || hadAlt || hadWin)
            {
                // Send key with active modifiers
                SendCharWithModifiers(c);
                return;
            }

            // Normal unicode injection
            NativeMethods.INPUT[] inputs = new NativeMethods.INPUT[2];

            inputs[0].type = NativeMethods.INPUT_KEYBOARD;
            inputs[0].u.ki.wVk = 0;
            inputs[0].u.ki.wScan = (ushort)c;
            inputs[0].u.ki.dwFlags = NativeMethods.KEYEVENTF_UNICODE;

            inputs[1].type = NativeMethods.INPUT_KEYBOARD;
            inputs[1].u.ki.wVk = 0;
            inputs[1].u.ki.wScan = (ushort)c;
            inputs[1].u.ki.dwFlags = NativeMethods.KEYEVENTF_UNICODE | NativeMethods.KEYEVENTF_KEYUP;

            NativeMethods.SendInput(2, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));

            // Shift and AltGr auto-unlatch after one character if not locked
            if (IsShiftActive)
            {
                IsShiftActive = false;
                ModifiersChanged?.Invoke();
            }
            if (IsAltGrActive)
            {
                IsAltGrActive = false;
                ModifiersChanged?.Invoke();
            }
        }

        private void SendCharWithModifiers(char c)
        {
            // For shortcuts, map char to virtual key code or send unicode
            byte vk = (byte)char.ToUpperInvariant(c);

            var inputs = new System.Collections.Generic.List<NativeMethods.INPUT>();

            if (IsCtrlActive) AddKey(inputs, NativeMethods.VK_CONTROL, false);
            if (IsAltActive) AddKey(inputs, NativeMethods.VK_MENU, false);
            if (IsWinActive) AddKey(inputs, NativeMethods.VK_LWIN, false);

            AddKey(inputs, vk, false);
            AddKey(inputs, vk, true);

            if (IsWinActive) AddKey(inputs, NativeMethods.VK_LWIN, true);
            if (IsAltActive) AddKey(inputs, NativeMethods.VK_MENU, true);
            if (IsCtrlActive) AddKey(inputs, NativeMethods.VK_CONTROL, true);

            NativeMethods.SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf(typeof(NativeMethods.INPUT)));

            // Unlatch single-use modifiers
            IsCtrlActive = false;
            IsAltActive = false;
            IsWinActive = false;
            IsShiftActive = false;
            ModifiersChanged?.Invoke();
        }

        public void SendVirtualKey(byte vk, bool isSpecial = true)
        {
            long now = Environment.TickCount64;
            if (vk == _lastVk && (now - _lastVkTick) < 100)
            {
                return; // Drop duplicate bounce
            }
            _lastVk = vk;
            _lastVkTick = now;

            _audioService.PlayKeySound(isSpecial);

            var inputs = new System.Collections.Generic.List<NativeMethods.INPUT>();

            if (IsShiftActive && vk != NativeMethods.VK_SHIFT) AddKey(inputs, NativeMethods.VK_SHIFT, false);
            if (IsCtrlActive && vk != NativeMethods.VK_CONTROL) AddKey(inputs, NativeMethods.VK_CONTROL, false);
            if (IsAltActive && vk != NativeMethods.VK_MENU) AddKey(inputs, NativeMethods.VK_MENU, false);
            if (IsWinActive && vk != NativeMethods.VK_LWIN) AddKey(inputs, NativeMethods.VK_LWIN, false);

            AddKey(inputs, vk, false);
            AddKey(inputs, vk, true);

            if (IsWinActive && vk != NativeMethods.VK_LWIN) AddKey(inputs, NativeMethods.VK_LWIN, true);
            if (IsAltActive && vk != NativeMethods.VK_MENU) AddKey(inputs, NativeMethods.VK_MENU, true);
            if (IsCtrlActive && vk != NativeMethods.VK_CONTROL) AddKey(inputs, NativeMethods.VK_CONTROL, true);
            if (IsShiftActive && vk != NativeMethods.VK_SHIFT) AddKey(inputs, NativeMethods.VK_SHIFT, true);

            NativeMethods.SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf(typeof(NativeMethods.INPUT)));

            if (IsShiftActive && vk != NativeMethods.VK_SHIFT)
            {
                IsShiftActive = false;
                ModifiersChanged?.Invoke();
            }
            if (IsCtrlActive && vk != NativeMethods.VK_CONTROL)
            {
                IsCtrlActive = false;
                ModifiersChanged?.Invoke();
            }
        }

        public void ToggleCapsLock()
        {
            _audioService.PlayKeySound(true);
            SendRawVk(NativeMethods.VK_CAPITAL);
            UpdateCapsLockState();
        }

        public void ToggleShift()
        {
            _audioService.PlayKeySound(true);
            IsShiftActive = !IsShiftActive;
            ModifiersChanged?.Invoke();
        }

        public void ToggleAltGr()
        {
            _audioService.PlayKeySound(true);
            IsAltGrActive = !IsAltGrActive;
            ModifiersChanged?.Invoke();
        }

        public void ToggleCtrl()
        {
            _audioService.PlayKeySound(true);
            IsCtrlActive = !IsCtrlActive;
            ModifiersChanged?.Invoke();
        }

        public void ToggleAlt()
        {
            _audioService.PlayKeySound(true);
            IsAltActive = !IsAltActive;
            ModifiersChanged?.Invoke();
        }

        public void ToggleWin()
        {
            _audioService.PlayKeySound(true);
            IsWinActive = !IsWinActive;
            ModifiersChanged?.Invoke();
        }

        public void SendShortcut(string action)
        {
            _audioService.PlayKeySound(true);
            switch (action.ToLowerInvariant())
            {
                case "selectall":
                    SendVkCombo(NativeMethods.VK_CONTROL, (byte)'A');
                    break;
                case "copy":
                    SendVkCombo(NativeMethods.VK_CONTROL, (byte)'C');
                    break;
                case "paste":
                    SendVkCombo(NativeMethods.VK_CONTROL, (byte)'V');
                    break;
                case "cut":
                    SendVkCombo(NativeMethods.VK_CONTROL, (byte)'X');
                    break;
                case "undo":
                    SendVkCombo(NativeMethods.VK_CONTROL, (byte)'Z');
                    break;
            }
        }

        private void SendVkCombo(byte modVk, byte keyVk)
        {
            var inputs = new NativeMethods.INPUT[4];
            inputs[0].type = NativeMethods.INPUT_KEYBOARD;
            inputs[0].u.ki.wVk = modVk;

            inputs[1].type = NativeMethods.INPUT_KEYBOARD;
            inputs[1].u.ki.wVk = keyVk;

            inputs[2].type = NativeMethods.INPUT_KEYBOARD;
            inputs[2].u.ki.wVk = keyVk;
            inputs[2].u.ki.dwFlags = NativeMethods.KEYEVENTF_KEYUP;

            inputs[3].type = NativeMethods.INPUT_KEYBOARD;
            inputs[3].u.ki.wVk = modVk;
            inputs[3].u.ki.dwFlags = NativeMethods.KEYEVENTF_KEYUP;

            NativeMethods.SendInput(4, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }

        private void SendRawVk(byte vk)
        {
            var inputs = new NativeMethods.INPUT[2];
            inputs[0].type = NativeMethods.INPUT_KEYBOARD;
            inputs[0].u.ki.wVk = vk;

            inputs[1].type = NativeMethods.INPUT_KEYBOARD;
            inputs[1].u.ki.wVk = vk;
            inputs[1].u.ki.dwFlags = NativeMethods.KEYEVENTF_KEYUP;

            NativeMethods.SendInput(2, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }

        private static void AddKey(System.Collections.Generic.List<NativeMethods.INPUT> list, byte vk, bool isUp)
        {
            var input = new NativeMethods.INPUT
            {
                type = NativeMethods.INPUT_KEYBOARD
            };
            input.u.ki.wVk = vk;
            if (isUp)
            {
                input.u.ki.dwFlags = NativeMethods.KEYEVENTF_KEYUP;
            }
            list.Add(input);
        }
    }
}
