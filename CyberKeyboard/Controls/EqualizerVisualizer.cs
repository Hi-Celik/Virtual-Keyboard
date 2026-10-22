using System;
using System.Windows;
using System.Windows.Media;
using CyberKeyboard.Graphics;

namespace CyberKeyboard.Controls
{
    public class EqualizerVisualizer : FrameworkElement
    {
        private const int BarCount = 18;
        private readonly double[] _barHeights = new double[BarCount];
        private readonly double[] _targetHeights = new double[BarCount];
        private readonly Random _rand = new();

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(EqualizerVisualizer),
                new PropertyMetadata(false));

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public EqualizerVisualizer()
        {
            IsHitTestVisible = false;
            Loaded += (s, e) => CompositionTarget.Rendering += OnRendering;
            Unloaded += (s, e) => CompositionTarget.Rendering -= OnRendering;
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            double maxH = ActualHeight > 0 ? ActualHeight : 30;

            bool is80s = ThemeManager.Instance.Is80sTheme;
            double lerpRate = is80s ? 0.16 : 0.35;

            for (int i = 0; i < BarCount; i++)
            {
                if (IsActive)
                {
                    // Generate pseudo-frequency movement with sine waves and random fluctuations
                    if (_rand.NextDouble() < (is80s ? 0.18 : 0.25))
                    {
                        double freq = 0.2 + (i / (double)BarCount) * 0.8;
                        _targetHeights[i] = (0.2 + _rand.NextDouble() * 0.8) * maxH * (1.0 - Math.Abs(i - BarCount / 2) * 0.04);
                    }
                    _barHeights[i] += (_targetHeights[i] - _barHeights[i]) * lerpRate;
                }
                else
                {
                    if (is80s)
                    {
                        // Ambient rhythmic synthwave wave (118 BPM beat rhythm)
                        double t = (DateTime.UtcNow - DateTime.Today).TotalSeconds * 2.8;
                        double wave = 3.0 + 6.5 * (0.5 + 0.5 * Math.Sin(t + i * 0.45));
                        _barHeights[i] += (wave - _barHeights[i]) * 0.12;
                    }
                    else
                    {
                        _barHeights[i] *= 0.85;
                    }
                }
            }

            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            double w = ActualWidth;
            double h = ActualHeight;
            if (w <= 0 || h <= 0) return;

            var theme = ThemeManager.Instance;
            bool isGlitch = theme.IsGlitchTheme;
            bool is80s = theme.Is80sTheme;
            Brush barBrush = theme.AccentBrush;
            Brush secondaryBrush = theme.SecondaryBrush;

            double spacing = 2.0;
            double totalSpacing = spacing * (BarCount - 1);
            double barWidth = Math.Max(2.0, (w - totalSpacing) / BarCount);
            double cornerRadius = isGlitch ? 0.5 : (is80s ? 2.0 : 1.5);

            for (int i = 0; i < BarCount; i++)
            {
                double bh = Math.Clamp(_barHeights[i], 2.0, h);
                double x = i * (barWidth + spacing);
                double y = h - bh;

                Brush activeBrush = (isGlitch || is80s) ? ((i % 2 == 0) ? barBrush : secondaryBrush) : barBrush;
                var rect = new Rect(x, y, barWidth, bh);
                dc.DrawRoundedRectangle(activeBrush, null, rect, cornerRadius, cornerRadius);
            }
        }
    }
}
