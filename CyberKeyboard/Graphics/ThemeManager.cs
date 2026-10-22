using System;
using System.ComponentModel;
using System.Windows.Media;

namespace CyberKeyboard.Graphics
{
    public enum CyberTheme
    {
        CyberCyan,
        SynthwavePink,
        MatrixGreen,
        SolarGold,
        Retro,
        RetroFuturist,
        RetroFuturist80s
    }

    public class ThemeManager : INotifyPropertyChanged
    {
        private static ThemeManager? _instance;
        public static ThemeManager Instance => _instance ??= new ThemeManager();

        private CyberTheme _currentTheme = CyberTheme.CyberCyan;
        public CyberTheme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    ApplyTheme(value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTheme)));
                }
            }
        }

        public Color AccentColor { get; private set; }
        public Color SecondaryColor { get; private set; }
        public Color BackgroundColor { get; private set; }
        public Color KeyBackgroundColor { get; private set; }
        public Color KeyBorderColor { get; private set; }
        public Color GlowColor { get; private set; }

        public SolidColorBrush AccentBrush { get; private set; } = new(Colors.Cyan);
        public SolidColorBrush SecondaryBrush { get; private set; } = new(Colors.DodgerBlue);
        public SolidColorBrush BackgroundBrush { get; private set; } = new(Color.FromArgb(240, 8, 12, 22));
        public SolidColorBrush KeyBackgroundBrush { get; private set; } = new(Color.FromArgb(190, 14, 21, 38));
        public SolidColorBrush KeyBorderBrush { get; private set; } = new(Color.FromArgb(90, 0, 240, 255));
        public SolidColorBrush GlowBrush { get; private set; } = new(Colors.Cyan);

        public bool IsGlitchTheme => CurrentTheme == CyberTheme.RetroFuturist;
        public bool Is80sTheme => CurrentTheme == CyberTheme.RetroFuturist80s;

        public event Action? ThemeChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public ThemeManager()
        {
            ApplyTheme(CyberTheme.CyberCyan);
        }

        public void ApplyTheme(CyberTheme theme)
        {
            switch (theme)
            {
                case CyberTheme.CyberCyan:
                    AccentColor = Color.FromRgb(0, 240, 255);
                    SecondaryColor = Color.FromRgb(0, 120, 255);
                    BackgroundColor = Color.FromArgb(245, 7, 11, 20);
                    KeyBackgroundColor = Color.FromArgb(200, 12, 19, 35);
                    KeyBorderColor = Color.FromArgb(100, 0, 240, 255);
                    GlowColor = Color.FromRgb(0, 240, 255);
                    break;

                case CyberTheme.SynthwavePink:
                    AccentColor = Color.FromRgb(255, 0, 128);
                    SecondaryColor = Color.FromRgb(155, 0, 232);
                    BackgroundColor = Color.FromArgb(245, 18, 5, 26);
                    KeyBackgroundColor = Color.FromArgb(200, 28, 10, 40);
                    KeyBorderColor = Color.FromArgb(100, 255, 0, 128);
                    GlowColor = Color.FromRgb(255, 0, 128);
                    break;

                case CyberTheme.MatrixGreen:
                    AccentColor = Color.FromRgb(0, 255, 102);
                    SecondaryColor = Color.FromRgb(0, 170, 68);
                    BackgroundColor = Color.FromArgb(245, 4, 18, 10);
                    KeyBackgroundColor = Color.FromArgb(200, 8, 28, 16);
                    KeyBorderColor = Color.FromArgb(100, 0, 255, 102);
                    GlowColor = Color.FromRgb(0, 255, 102);
                    break;

                case CyberTheme.SolarGold:
                    AccentColor = Color.FromRgb(255, 183, 0);
                    SecondaryColor = Color.FromRgb(255, 85, 0);
                    BackgroundColor = Color.FromArgb(245, 20, 14, 5);
                    KeyBackgroundColor = Color.FromArgb(200, 32, 22, 8);
                    KeyBorderColor = Color.FromArgb(100, 255, 183, 0);
                    GlowColor = Color.FromRgb(255, 183, 0);
                    break;

                case CyberTheme.Retro:
                    // 80s Retro Arcade / Warm Tangerine & Vintage Gold
                    AccentColor = Color.FromRgb(255, 101, 47);    // Retro Tangerine / CRT Amber
                    SecondaryColor = Color.FromRgb(255, 215, 0);  // Vintage Arcade Yellow
                    BackgroundColor = Color.FromArgb(245, 18, 15, 24);
                    KeyBackgroundColor = Color.FromArgb(200, 34, 30, 44);
                    KeyBorderColor = Color.FromArgb(120, 255, 101, 47);
                    GlowColor = Color.FromRgb(255, 101, 47);
                    break;

                case CyberTheme.RetroFuturist:
                    // Retro-Futurist Glitch Art Aesthetic: Raw Electric Cyan vs Jarring Neon Magenta (#00F0FF vs #FF0080)
                    AccentColor = Color.FromRgb(0, 240, 255);          // Raw Electric Cyan
                    SecondaryColor = Color.FromRgb(255, 0, 128);       // Jarring Neon Magenta
                    BackgroundColor = Color.FromArgb(248, 5, 4, 10);    // Ultra-Dark Void (#05040A)
                    KeyBackgroundColor = Color.FromArgb(220, 12, 7, 22); // Deep Obsidian Violet
                    KeyBorderColor = Color.FromArgb(140, 255, 0, 128);  // Magenta Glitch Edge
                    GlowColor = Color.FromRgb(0, 240, 255);             // Cyan Phosphor Glow
                    break;

                case CyberTheme.RetroFuturist80s:
                    // 80s Retro-Futurist / Neon Arcade Aesthetic (Matching reference palette)
                    AccentColor = Color.FromRgb(0, 240, 255);          // High-Voltage Neon Cyan (#00F0FF)
                    SecondaryColor = Color.FromRgb(255, 0, 85);        // Intense Arcade Hot Pink (#FF0055)
                    BackgroundColor = Color.FromArgb(252, 4, 4, 8);     // Pitch Dark Synth Void (#040408)
                    KeyBackgroundColor = Color.FromArgb(240, 10, 10, 18); // Deep Matte Black (#0A0A12)
                    KeyBorderColor = Color.FromArgb(230, 0, 240, 255); // Solid Neon Cyan Rim
                    GlowColor = Color.FromRgb(255, 0, 85);             // Hot Pink 3D Underglow
                    break;
            }

            AccentBrush = new SolidColorBrush(AccentColor);
            SecondaryBrush = new SolidColorBrush(SecondaryColor);
            BackgroundBrush = new SolidColorBrush(BackgroundColor);
            KeyBackgroundBrush = new SolidColorBrush(KeyBackgroundColor);
            KeyBorderBrush = new SolidColorBrush(KeyBorderColor);
            GlowBrush = new SolidColorBrush(GlowColor);

            AccentBrush.Freeze();
            SecondaryBrush.Freeze();
            BackgroundBrush.Freeze();
            KeyBackgroundBrush.Freeze();
            KeyBorderBrush.Freeze();
            GlowBrush.Freeze();

            ThemeChanged?.Invoke();
        }

        public void CycleTheme()
        {
            CurrentTheme = CurrentTheme switch
            {
                CyberTheme.CyberCyan => CyberTheme.SynthwavePink,
                CyberTheme.SynthwavePink => CyberTheme.MatrixGreen,
                CyberTheme.MatrixGreen => CyberTheme.SolarGold,
                CyberTheme.SolarGold => CyberTheme.Retro,
                CyberTheme.Retro => CyberTheme.RetroFuturist,
                CyberTheme.RetroFuturist => CyberTheme.RetroFuturist80s,
                CyberTheme.RetroFuturist80s => CyberTheme.CyberCyan,
                _ => CyberTheme.CyberCyan
            };
        }

        public string GetThemeDisplayName() => CurrentTheme switch
        {
            CyberTheme.CyberCyan => "CYAN",
            CyberTheme.SynthwavePink => "SYNTH",
            CyberTheme.MatrixGreen => "MATRIX",
            CyberTheme.SolarGold => "SOLAR",
            CyberTheme.Retro => "RETRO",
            CyberTheme.RetroFuturist => "R-FUTURIST",
            CyberTheme.RetroFuturist80s => "80s CHILL",
            _ => "CYBER"
        };
    }
}
