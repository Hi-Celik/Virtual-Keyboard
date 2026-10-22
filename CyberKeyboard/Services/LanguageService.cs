using System;
using System.Text;
using System.Windows.Threading;
using CyberKeyboard.Native;

namespace CyberKeyboard.Services
{
    public enum SupportedLanguage
    {
        Turkish,
        English,
        FrenchAzerty,
        FrenchQwerty,
        German
    }

    public class LanguageService
    {
        private readonly DispatcherTimer _pollTimer;
        private IntPtr _lastHwnd = IntPtr.Zero;
        private IntPtr _lastHkl = IntPtr.Zero;
        private SupportedLanguage _currentLanguage = SupportedLanguage.Turkish;
        private string _activeAppName = string.Empty;
        private IntPtr _ownHwnd = IntPtr.Zero;

        public event Action<SupportedLanguage>? LanguageChanged;
        public event Action<string>? ActiveApplicationChanged;

        public SupportedLanguage CurrentLanguage => _currentLanguage;
        public string ActiveAppName => _activeAppName;

        public LanguageService()
        {
            _pollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _pollTimer.Tick += OnPollTimerTick;
        }

        public void SetOwnHandle(IntPtr ownHwnd)
        {
            _ownHwnd = ownHwnd;
        }

        public void Start()
        {
            _pollTimer.Start();
        }

        public void Stop()
        {
            _pollTimer.Stop();
        }

        private void OnPollTimerTick(object? sender, EventArgs e)
        {
            IntPtr fgHwnd = NativeMethods.GetForegroundWindow();
            if (fgHwnd == IntPtr.Zero || fgHwnd == _ownHwnd)
                return;

            // Check if active application window changed
            if (fgHwnd != _lastHwnd)
            {
                _lastHwnd = fgHwnd;
                var sb = new StringBuilder(256);
                NativeMethods.GetWindowText(fgHwnd, sb, sb.Capacity);
                string title = sb.ToString();
                if (string.IsNullOrWhiteSpace(title))
                {
                    _activeAppName = "Active Target Application";
                }
                else
                {
                    if (title.Length > 30)
                        title = title.Substring(0, 27) + "...";
                    _activeAppName = title;
                }
                ActiveApplicationChanged?.Invoke(_activeAppName);
            }

            // Get keyboard layout for target thread
            uint threadId = NativeMethods.GetWindowThreadProcessId(fgHwnd, out _);
            IntPtr hkl = NativeMethods.GetKeyboardLayout(threadId);

            if (hkl != _lastHkl)
            {
                _lastHkl = hkl;
                ushort langId = (ushort)((long)hkl & 0xFFFF);
                var detected = MapLangIdToSupported(langId);

                if (detected != _currentLanguage)
                {
                    _currentLanguage = detected;
                    LanguageChanged?.Invoke(_currentLanguage);
                }
            }
        }

        public void SetLanguageManual(SupportedLanguage language)
        {
            _currentLanguage = language;
            LanguageChanged?.Invoke(_currentLanguage);

            try
            {
                string klid = language switch
                {
                    SupportedLanguage.Turkish => "0000041F",
                    SupportedLanguage.English => "00000409",
                    SupportedLanguage.FrenchAzerty => "0000040C",
                    SupportedLanguage.FrenchQwerty => "00000C0C",
                    SupportedLanguage.German => "00000407",
                    _ => "0000041F"
                };

                IntPtr hkl = NativeMethods.LoadKeyboardLayout(klid, NativeMethods.KLF_ACTIVATE);
                IntPtr fgHwnd = NativeMethods.GetForegroundWindow();
                if (fgHwnd != IntPtr.Zero && fgHwnd != _ownHwnd && hkl != IntPtr.Zero)
                {
                    NativeMethods.PostMessage(fgHwnd, NativeMethods.WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, hkl);
                }
            }
            catch
            {
                // Layout switch fallback
            }
        }

        private static SupportedLanguage MapLangIdToSupported(ushort langId)
        {
            // Specific overrides
            if (langId == 0x0C0C || langId == 0x1009)
            {
                return SupportedLanguage.FrenchQwerty; // Canadian French QWERTY
            }

            // Primary language IDs (lower 10 bits of langId)
            ushort primaryLang = (ushort)(langId & 0x03FF);

            switch (primaryLang)
            {
                case 0x1F: // Turkish
                    return SupportedLanguage.Turkish;
                case 0x0C: // French
                    return SupportedLanguage.FrenchAzerty;
                case 0x07: // German
                    return SupportedLanguage.German;
                case 0x09: // English
                default:
                    return SupportedLanguage.English;
            }
        }

        public static string GetLanguageCode(SupportedLanguage lang) => lang switch
        {
            SupportedLanguage.Turkish => "TR-Q",
            SupportedLanguage.English => "EN-US",
            SupportedLanguage.FrenchAzerty => "FR-AZ",
            SupportedLanguage.FrenchQwerty => "FR-Q",
            SupportedLanguage.German => "DE-QZ",
            _ => "TR-Q"
        };

        public static string GetLanguageDisplayName(SupportedLanguage lang) => lang switch
        {
            SupportedLanguage.Turkish => "TÜRKÇE (TR-Q)",
            SupportedLanguage.English => "ENGLISH (US)",
            SupportedLanguage.FrenchAzerty => "FRANÇAIS (AZERTY)",
            SupportedLanguage.FrenchQwerty => "FRANÇAIS (QWERTY)",
            SupportedLanguage.German => "DEUTSCH (QWERTZ)",
            _ => "TÜRKÇE"
        };
    }
}
