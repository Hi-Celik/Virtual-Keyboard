using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CyberKeyboard.Graphics
{
    public class CyberParticle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Vx { get; set; }
        public double Vy { get; set; }
        public double Radius { get; set; }
    }

    public class Shockwave
    {
        public Point Center { get; set; }
        public double CurrentRadius { get; set; }
        public double MaxRadius { get; set; } = 140.0;
        public double Life { get; set; } = 1.0;
        public double Speed { get; set; } = 4.5;
    }

    public class SparkParticle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Vx { get; set; }
        public double Vy { get; set; }
        public double Life { get; set; } = 1.0;
        public double Size { get; set; } = 2.0;
    }

    public struct GlitchTear
    {
        public double Y;
        public double Height;
        public double X;
        public double Width;
        public int ColorType; // 0 = Cyan, 1 = Magenta, 2 = Stark White
        public double Life;
    }

    public class NeonBeacon
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Phase { get; set; }
        public double Speed { get; set; }
        public double BaseSize { get; set; }
        public int BeaconType { get; set; } // 0 = Diamond Star, 1 = Crosshair Reticle, 2 = Strobe Pulsar
        public bool IsMagenta { get; set; }
    }

    public class CyberBackgroundCanvas : FrameworkElement
    {
        private readonly List<CyberParticle> _particles = new();
        private readonly List<Shockwave> _shockwaves = new();
        private readonly List<SparkParticle> _sparks = new();
        private readonly List<GlitchTear> _glitchTears = new();
        private readonly List<NeonBeacon> _beacons = new();
        private readonly Random _rand = new();

        private double _gridOffset = 0;
        private double _glitchTimer = 0.1;
        private double _sweepLaserY = -45;
        private DateTime _lastFrameTime = DateTime.UtcNow;

        // Cached, pre-frozen drawing resources (Zero GC allocations per frame!)
        private Pen? _gridPen;
        private Pen? _scanlinePen;
        private Brush? _nodeBrush;
        private Brush? _sparkBrush;
        private Brush _glitchCyanBrush = new SolidColorBrush(Color.FromArgb(170, 0, 240, 255));
        private Brush _glitchMagentaBrush = new SolidColorBrush(Color.FromArgb(170, 255, 0, 128));
        private Brush _glitchWhiteBrush = new SolidColorBrush(Color.FromArgb(210, 255, 255, 255));
        private Pen[] _linePens = new Pen[8];
        private Pen[] _shockwavePens = new Pen[8];

        // 80s Theme Specific High-Visibility Drawing Resources (0 GC)
        private Brush _dotCyanBrush = new SolidColorBrush(Color.FromArgb(85, 0, 240, 255));
        private Brush _dotPinkBrush = new SolidColorBrush(Color.FromArgb(145, 255, 0, 85));
        private Pen _laserBeamPen = new Pen(new SolidColorBrush(Color.FromArgb(235, 0, 240, 255)), 1.5);
        private Pen _laserTrailingPen = new Pen(new SolidColorBrush(Color.FromArgb(170, 255, 0, 85)), 1.0);
        private LinearGradientBrush? _laserWakeBrush;
        private Pen[] _beaconCyanPens = new Pen[8];
        private Pen[] _beaconPinkPens = new Pen[8];
        private Brush[] _beaconCyanBrushes = new Brush[8];
        private Brush[] _beaconPinkBrushes = new Brush[8];
        private Pen[] _shockwaveCyanPens = new Pen[8];
        private Pen[] _shockwavePinkPens = new Pen[8];
        private Brush _sparkCyanBrush = new SolidColorBrush(Color.FromArgb(250, 0, 240, 255));
        private Brush _sparkPinkBrush = new SolidColorBrush(Color.FromArgb(250, 255, 0, 85));

        public CyberBackgroundCanvas()
        {
            IsHitTestVisible = false;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            SizeChanged += OnSizeChanged;
            ThemeManager.Instance.ThemeChanged += UpdateCachedThemeResources;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateCachedThemeResources();
            InitParticles();
            InitBeacons();
            CompositionTarget.Rendering += OnRendering;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= OnRendering;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitParticles();
            InitBeacons();
        }

        private void UpdateCachedThemeResources()
        {
            var theme = ThemeManager.Instance;
            Color accent = theme.AccentColor;
            Color secondary = theme.SecondaryColor;

            // Grid pen
            var gb = new SolidColorBrush(Color.FromArgb(12, accent.R, accent.G, accent.B));
            gb.Freeze();
            _gridPen = new Pen(gb, 1.0);
            _gridPen.Freeze();

            // CRT Scanline pen for Glitch theme
            if (theme.IsGlitchTheme)
            {
                var slb = new SolidColorBrush(Color.FromArgb(22, 0, 240, 255));
                slb.Freeze();
                _scanlinePen = new Pen(slb, 0.85);
                _scanlinePen.Freeze();
            }
            else
            {
                _scanlinePen = null;
            }

            // Node brush
            var nb = new SolidColorBrush(Color.FromArgb(70, secondary.R, secondary.G, secondary.B));
            nb.Freeze();
            _nodeBrush = nb;

            // Spark brush
            var sb = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255));
            sb.Freeze();
            _sparkBrush = sb;

            // Pre-allocate 8 quantized line pens by alpha
            for (int i = 0; i < 8; i++)
            {
                byte alpha = (byte)(8 + i * 5); // 8 to 43
                Color lineCol = ((theme.IsGlitchTheme || theme.Is80sTheme) && i % 2 == 1) ? secondary : accent;
                var lb = new SolidColorBrush(Color.FromArgb(alpha, lineCol.R, lineCol.G, lineCol.B));
                lb.Freeze();
                _linePens[i] = new Pen(lb, 1.0);
                _linePens[i].Freeze();
            }

            // Pre-allocate 8 quantized shockwave pens by life
            for (int i = 0; i < 8; i++)
            {
                byte alpha = (byte)(20 + i * 20); // 20 to 160
                Color swCol = ((theme.IsGlitchTheme || theme.Is80sTheme) && i % 2 == 1) ? secondary : accent;
                var swb = new SolidColorBrush(Color.FromArgb(alpha, swCol.R, swCol.G, swCol.B));
                swb.Freeze();
                _shockwavePens[i] = new Pen(swb, 1.5 + i * 0.25);
                _shockwavePens[i].Freeze();
            }

            // 80s Chill / Neon Arcade high-visibility drawing resources
            _dotCyanBrush.Freeze();
            _dotPinkBrush.Freeze();
            _laserBeamPen.Freeze();
            _laserTrailingPen.Freeze();
            _sparkCyanBrush.Freeze();
            _sparkPinkBrush.Freeze();

            var lwb = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(0, 0, 240, 255), 0.0),
                    new GradientStop(Color.FromArgb(40, 255, 0, 85), 0.6),
                    new GradientStop(Color.FromArgb(85, 0, 240, 255), 1.0)
                }
            };
            lwb.Freeze();
            _laserWakeBrush = lwb;

            for (int i = 0; i < 8; i++)
            {
                byte alpha = (byte)(45 + i * 28); // 45 to 241
                var bcb = new SolidColorBrush(Color.FromArgb(alpha, 0, 240, 255));
                bcb.Freeze();
                _beaconCyanBrushes[i] = bcb;
                _beaconCyanPens[i] = new Pen(bcb, 1.2);
                _beaconCyanPens[i].Freeze();

                var bpb = new SolidColorBrush(Color.FromArgb(alpha, 255, 0, 85));
                bpb.Freeze();
                _beaconPinkBrushes[i] = bpb;
                _beaconPinkPens[i] = new Pen(bpb, 1.2);
                _beaconPinkPens[i].Freeze();

                byte swAlpha = (byte)(50 + i * 27); // 50 to 239
                var swcb = new SolidColorBrush(Color.FromArgb(swAlpha, 0, 240, 255));
                swcb.Freeze();
                _shockwaveCyanPens[i] = new Pen(swcb, 2.0 + i * 0.25);
                _shockwaveCyanPens[i].Freeze();

                var swpb = new SolidColorBrush(Color.FromArgb(swAlpha, 255, 0, 85));
                swpb.Freeze();
                _shockwavePinkPens[i] = new Pen(swpb, 2.5 + i * 0.25);
                _shockwavePinkPens[i].Freeze();
            }
        }

        private void InitParticles()
        {
            _particles.Clear();
            double w = ActualWidth > 0 ? ActualWidth : 1200;
            double h = ActualHeight > 0 ? ActualHeight : 600;

            for (int i = 0; i < 30; i++)
            {
                _particles.Add(new CyberParticle
                {
                    X = _rand.NextDouble() * w,
                    Y = _rand.NextDouble() * h,
                    Vx = (_rand.NextDouble() - 0.5) * 0.6,
                    Vy = (_rand.NextDouble() - 0.5) * 0.6,
                    Radius = 1.8 + _rand.NextDouble() * 1.5
                });
            }
        }

        private void InitBeacons()
        {
            _beacons.Clear();
            double w = ActualWidth > 0 ? ActualWidth : 1200;
            double h = ActualHeight > 0 ? ActualHeight : 600;

            // Spread 24 prominent synth beacon stars across the screen
            for (int i = 0; i < 24; i++)
            {
                _beacons.Add(new NeonBeacon
                {
                    X = 35 + _rand.NextDouble() * (w - 70),
                    Y = 25 + _rand.NextDouble() * (h - 50),
                    Phase = _rand.NextDouble() * Math.PI * 2.0,
                    Speed = 1.8 + _rand.NextDouble() * 3.4,
                    BaseSize = 4.5 + _rand.NextDouble() * 4.5,
                    BeaconType = _rand.Next(0, 3), // 0=Diamond Star, 1=Crosshair Reticle, 2=Strobe Pulsar
                    IsMagenta = (_rand.Next(0, 2) == 1)
                });
            }
        }

        public void TriggerShockwave(Point pt)
        {
            // Dairesel sok dalgasi efektleri hizli yazim performansi ve 0 gecikme icin kaldirildi
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            var now = DateTime.UtcNow;
            double dt = (now - _lastFrameTime).TotalSeconds;
            _lastFrameTime = now;
            if (dt > 0.05) dt = 0.016;

            double w = ActualWidth;
            double h = ActualHeight;
            if (w <= 0 || h <= 0) return;

            bool is80s = ThemeManager.Instance.Is80sTheme;

            if (is80s)
            {
                // Smooth sweeping laser scan beam down the keyboard
                _sweepLaserY += 72.0 * dt;
                if (_sweepLaserY > h + 50) _sweepLaserY = -45;

                // Update pulsating beacon phases
                for (int i = 0; i < _beacons.Count; i++)
                {
                    _beacons[i].Phase += _beacons[i].Speed * dt;
                }
            }

            // Animate grid offset (slow, calm cruising for 80s theme)
            double gridSpeed = is80s ? 4.0 : 12.0;
            _gridOffset = (_gridOffset + gridSpeed * dt) % 50.0;

            // Update particles (gentle slow drifting stars for 80s theme)
            double pSpeedMultiplier = is80s ? 0.35 : 1.0;
            for (int i = 0; i < _particles.Count; i++)
            {
                var p = _particles[i];
                p.X += p.Vx * pSpeedMultiplier;
                p.Y += p.Vy * pSpeedMultiplier;

                if (p.X < 0) { p.X = 0; p.Vx = -p.Vx; }
                else if (p.X > w) { p.X = w; p.Vx = -p.Vx; }
                if (p.Y < 0) { p.Y = 0; p.Vy = -p.Vy; }
                else if (p.Y > h) { p.Y = h; p.Vy = -p.Vy; }
            }

            // Update Glitch Art Tearing & Static Noise
            if (ThemeManager.Instance.IsGlitchTheme)
            {
                _glitchTimer -= dt;
                if (_glitchTimer <= 0)
                {
                    _glitchTimer = 0.05 + _rand.NextDouble() * 0.12;
                    int tearCount = _rand.Next(1, 4);
                    for (int t = 0; t < tearCount; t++)
                    {
                        if (_glitchTears.Count < 12)
                        {
                            _glitchTears.Add(new GlitchTear
                            {
                                Y = _rand.NextDouble() * h,
                                Height = 1.5 + _rand.NextDouble() * 6.5,
                                X = _rand.NextDouble() * (w * 0.25),
                                Width = (0.3 + _rand.NextDouble() * 0.7) * w,
                                ColorType = _rand.Next(0, 3),
                                Life = 0.04 + _rand.NextDouble() * 0.08
                            });
                        }
                    }
                }

                for (int i = _glitchTears.Count - 1; i >= 0; i--)
                {
                    var tear = _glitchTears[i];
                    tear.Life -= dt;
                    if (tear.Life <= 0)
                    {
                        _glitchTears.RemoveAt(i);
                    }
                    else
                    {
                        _glitchTears[i] = tear;
                    }
                }
            }
            else if (_glitchTears.Count > 0)
            {
                _glitchTears.Clear();
            }

            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            double w = ActualWidth;
            double h = ActualHeight;
            if (w <= 0 || h <= 0) return;

            if (ThemeManager.Instance.Is80sTheme)
            {
                // ==========================================
                // 80s CHILL / NEON SYNTH ARCADE VISUAL ENGINE
                // Prominent pulsating animations & visible laser grid
                // ==========================================

                // 1. High-Visibility Crisp Dot-Matrix Grid
                double step = 32.0;
                int col = 0;
                for (double x = 16.0; x < w; x += step, col++)
                {
                    int row = 0;
                    for (double y = 16.0; y < h; y += step, row++)
                    {
                        bool isMajor = (col % 4 == 0 && row % 4 == 0);
                        if (isMajor)
                        {
                            dc.DrawRectangle(_dotPinkBrush, null, new Rect(x - 1.25, y - 1.25, 2.5, 2.5));
                        }
                        else
                        {
                            dc.DrawRectangle(_dotCyanBrush, null, new Rect(x - 0.75, y - 0.75, 1.5, 1.5));
                        }
                    }
                }

                // 2. Prominent Sweeping Neon Laser Beam & Glowing Wake
                if (_laserWakeBrush != null)
                {
                    dc.DrawRectangle(_laserWakeBrush, null, new Rect(0, _sweepLaserY - 45, w, 45));
                }
                dc.DrawLine(_laserBeamPen, new Point(0, _sweepLaserY), new Point(w, _sweepLaserY));
                dc.DrawLine(_laserTrailingPen, new Point(0, _sweepLaserY + 2.5), new Point(w, _sweepLaserY + 2.5));

                // 3. Prominent Pulsating & Blinking Arcade Beacon Stars
                for (int i = 0; i < _beacons.Count; i++)
                {
                    var b = _beacons[i];
                    double pulse;
                    if (b.BeaconType == 2)
                    {
                        // Strobe Pulsar: sharp, crisp rhythmic arcade flash!
                        double s = Math.Sin(b.Phase);
                        pulse = (s > 0.35) ? Math.Pow((s - 0.35) / 0.65, 2.2) : 0.05;
                    }
                    else
                    {
                        // Smooth breathing / pulsating diamond star / reticle
                        pulse = 0.25 + 0.75 * (0.5 + 0.5 * Math.Sin(b.Phase));
                    }

                    int qIdx = Math.Clamp((int)(pulse * 7.99), 0, 7);
                    Pen pPen = b.IsMagenta ? _beaconPinkPens[qIdx] : _beaconCyanPens[qIdx];
                    Brush pBrush = b.IsMagenta ? _beaconPinkBrushes[qIdx] : _beaconCyanBrushes[qIdx];
                    double curSize = b.BaseSize * (0.6 + 0.65 * pulse);

                    if (b.BeaconType == 0)
                    {
                        // 4-Point Diamond Flare Star: horizontal & vertical flare rays + glowing center
                        dc.DrawLine(pPen, new Point(b.X - curSize * 1.5, b.Y), new Point(b.X + curSize * 1.5, b.Y));
                        dc.DrawLine(pPen, new Point(b.X, b.Y - curSize * 1.5), new Point(b.X, b.Y + curSize * 1.5));
                        dc.DrawRectangle(pBrush, null, new Rect(b.X - 1.5, b.Y - 1.5, 3.0, 3.0));
                    }
                    else if (b.BeaconType == 1)
                    {
                        // Crosshair Target Reticle: '+' with square center
                        dc.DrawLine(pPen, new Point(b.X - curSize, b.Y), new Point(b.X + curSize, b.Y));
                        dc.DrawLine(pPen, new Point(b.X, b.Y - curSize), new Point(b.X, b.Y + curSize));
                        dc.DrawRectangle(null, pPen, new Rect(b.X - curSize * 0.4, b.Y - curSize * 0.4, curSize * 0.8, curSize * 0.8));
                    }
                    else
                    {
                        // Strobe Pulsar: halo ring + blazing core
                        dc.DrawEllipse(null, pPen, new Point(b.X, b.Y), curSize * 1.4, curSize * 1.4);
                        dc.DrawRectangle(pBrush, null, new Rect(b.X - 2.0, b.Y - 2.0, 4.0, 4.0));
                    }
                }
                return;
            }

            // ==========================================
            // STANDARD & GLITCH THEMES (100% UNCHANGED)
            // ==========================================
            if (_gridPen == null) return;

            // 1. Draw Tech Grid Lines
            double originalStep = 50.0;
            for (double x = 0; x < w; x += originalStep)
            {
                dc.DrawLine(_gridPen, new Point(x, 0), new Point(x, h));
            }
            for (double y = _gridOffset; y < h; y += originalStep)
            {
                dc.DrawLine(_gridPen, new Point(0, y), new Point(w, y));
            }

            // 2. Draw Constellation Network Connections
            double maxDistSq = 130 * 130;
            for (int i = 0; i < _particles.Count; i++)
            {
                var p1 = _particles[i];
                for (int j = i + 1; j < _particles.Count; j++)
                {
                    var p2 = _particles[j];
                    double dx = p1.X - p2.X;
                    double dy = p1.Y - p2.Y;
                    double distSq = dx * dx + dy * dy;

                    if (distSq < maxDistSq)
                    {
                        double factor = 1.0 - (distSq / maxDistSq);
                        int penIndex = (int)(factor * 7.99);
                        if (penIndex >= 0 && penIndex < 8 && _linePens[penIndex] != null)
                        {
                            dc.DrawLine(_linePens[penIndex], new Point(p1.X, p1.Y), new Point(p2.X, p2.Y));
                        }
                    }
                }
            }

            // 3. Draw Nodes
            if (_nodeBrush != null)
            {
                for (int i = 0; i < _particles.Count; i++)
                {
                    var p = _particles[i];
                    dc.DrawEllipse(_nodeBrush, null, new Point(p.X, p.Y), p.Radius, p.Radius);
                }
            }

            // 4. Glitch Art CRT Scanlines & Screen Tearing (Retro-Futurist only)
            if (ThemeManager.Instance.IsGlitchTheme)
            {
                if (_scanlinePen != null)
                {
                    for (double y = 0; y < h; y += 4.0)
                    {
                        dc.DrawLine(_scanlinePen, new Point(0, y), new Point(w, y));
                    }
                }

                // Screen Tearing Glitch Slices
                for (int i = 0; i < _glitchTears.Count; i++)
                {
                    var tear = _glitchTears[i];
                    Brush tb = tear.ColorType switch
                    {
                        0 => _glitchCyanBrush,
                        1 => _glitchMagentaBrush,
                        _ => _glitchWhiteBrush
                    };
                    dc.DrawRectangle(tb, null, new Rect(tear.X, tear.Y, tear.Width, tear.Height));
                }

                // Machine Static Noise Specks
                if (_rand.NextDouble() < 0.60)
                {
                    int noiseCount = _rand.Next(3, 8);
                    for (int n = 0; n < noiseCount; n++)
                    {
                        double nx = _rand.NextDouble() * w;
                        double ny = _rand.NextDouble() * h;
                        double ns = 2.0 + _rand.NextDouble() * 3.5;
                        Brush nb = (_rand.NextDouble() < 0.5) ? _glitchCyanBrush : _glitchMagentaBrush;
                        dc.DrawRectangle(nb, null, new Rect(nx, ny, ns, ns));
                    }
                }
            }
        }
    }
}
