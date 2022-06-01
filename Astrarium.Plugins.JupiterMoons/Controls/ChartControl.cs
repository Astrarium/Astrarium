using Astrarium.Algorithms;
using Astrarium.Plugins.JupiterMoons.ImportExport;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Astrarium.Plugins.JupiterMoons.Controls
{
    public class ChartControl : Control, IBitmapWriter
    {
        public static readonly DependencyProperty IsLockedProperty =
            DependencyProperty.Register(nameof(IsLocked), typeof(bool), typeof(ChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty ShowIoProperty =
            DependencyProperty.Register(nameof(ShowIo), typeof(bool), typeof(ChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty ShowEuropaProperty =
            DependencyProperty.Register(nameof(ShowEuropa), typeof(bool), typeof(ChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty ShowGanymedeProperty =
            DependencyProperty.Register(nameof(ShowGanymede), typeof(bool), typeof(ChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty ShowCallistoProperty =
            DependencyProperty.Register(nameof(ShowCallisto), typeof(bool), typeof(ChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty PositionsProperty =
            DependencyProperty.Register(nameof(Positions), typeof(ICollection<CrdsRectangular[,]>), typeof(ChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty CurrentPositionsProperty =
            DependencyProperty.Register(nameof(CurrentPositions), typeof(CrdsRectangular[,]), typeof(ChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = false, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty DaysOffsetProperty =
            DependencyProperty.Register(nameof(DaysOffset), typeof(double), typeof(ChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty VerticalScaleProperty =
            DependencyProperty.Register(nameof(VerticalScale), typeof(int), typeof(ChartControl), new FrameworkPropertyMetadata(null) { DefaultValue = 1, BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty HorizontalScaleProperty =
            DependencyProperty.Register(nameof(HorizontalScale), typeof(int), typeof(ChartControl), new FrameworkPropertyMetadata(null) { DefaultValue = 3, BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(ChartOrientation), typeof(ChartControl), new FrameworkPropertyMetadata(null) { DefaultValue = ChartOrientation.Direct, BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty HeaderProperty =
           DependencyProperty.Register(nameof(Header), typeof(string), typeof(ChartControl), new FrameworkPropertyMetadata(null) { DefaultValue = null, BindsTwoWayByDefault = true, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty CurrentPositionProperty =
            DependencyProperty.Register(nameof(CurrentPosition), typeof(int), typeof(ChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty DarkModeProperty =
            DependencyProperty.Register(nameof(DarkMode), typeof(bool), typeof(ChartControl), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, DefaultValue = false });

        private Typeface font = new Typeface(new FontFamily("#Noto Sans"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

        private string[] names = new[] { Text.Get("JupiterMoons.Io"), Text.Get("JupiterMoons.Europa"), Text.Get("JupiterMoons.Ganymede"), Text.Get("JupiterMoons.Callisto") };

        private WriteToBitmapOptions renderOptions = new WriteToBitmapOptions();

        private Brush LabelsBrush => new SolidColorBrush((Color)FindResource("ColorForeground"));

        private Brush LinesBrush => new SolidColorBrush((Color)FindResource("ColorControlLightBackground"));

        private Brush CurrentDateBrush => new SolidColorBrush((Color)FindResource("ColorForeground"));

        private readonly Brush[] jupiterBrush = new Brush[] { Brushes.Wheat, Brushes.DarkRed };
        private Brush JupiterBrush => jupiterBrush[DarkMode ? 1 : 0];

        private readonly Brush[][] curvesBrush = new Brush[][]
        {
            new [] { Brushes.Orange, Brushes.Cyan, Brushes.Red, Brushes.LightGreen },
            new [] { Brushes.OrangeRed, Brushes.DarkRed, Brushes.Red, Brushes.Brown }
        };
        private Brush[] CurvesBrush => curvesBrush[DarkMode ? 1 : 0];

        public bool IsLocked
        {
            get => (bool)GetValue(IsLockedProperty);
            set => SetValue(IsLockedProperty, value);
        }

        public bool ShowIo
        {
            get => (bool)GetValue(ShowIoProperty);
            set => SetValue(ShowIoProperty, value);
        }

        public bool ShowEuropa
        {
            get => (bool)GetValue(ShowEuropaProperty);
            set => SetValue(ShowEuropaProperty, value);
        }

        public bool ShowGanymede
        {
            get => (bool)GetValue(ShowGanymedeProperty);
            set => SetValue(ShowGanymedeProperty, value);
        }

        public bool ShowCallisto
        {
            get => (bool)GetValue(ShowCallistoProperty);
            set => SetValue(ShowCallistoProperty, value);
        }

        public int CurrentPosition
        {
            get => (int)GetValue(CurrentPositionProperty);
            set => SetValue(CurrentPositionProperty, value);
        }

        public ICollection<CrdsRectangular[,]> Positions
        {
            get => (ICollection<CrdsRectangular[,]>)GetValue(PositionsProperty);
            set => SetValue(PositionsProperty, value);
        }

        public double DaysOffset
        {
            get => (double)GetValue(DaysOffsetProperty);
            set => SetValue(DaysOffsetProperty, value);
        }

        public int HorizontalScale
        {
            get => (int)GetValue(HorizontalScaleProperty);
            set => SetValue(HorizontalScaleProperty, value);
        }

        public int VerticalScale
        {
            get => (int)GetValue(VerticalScaleProperty);
            set => SetValue(VerticalScaleProperty, value);
        }

        public CrdsRectangular[,] CurrentPositions
        {
            get => (CrdsRectangular[,])GetValue(CurrentPositionsProperty);
            set => SetValue(CurrentPositionsProperty, value);
        }

        public ChartOrientation Orientation
        {
            get => (ChartOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        /// <summary>
        /// If set, dark mode is used
        /// </summary>
        public bool DarkMode
        {
            get => (bool)GetValue(DarkModeProperty);
            set => SetValue(DarkModeProperty, value);
        }

        private void DoRender(DrawingContext ctx, WriteToBitmapOptions options)
        {
            int headerSize =  options.Header ? Math.Max(40, (int)(((options.Size.Height) / ((Positions?.Count ?? 0) / 24) * 5.0) * (options.VerticalScale / 5.0))) : 0;
            int legendSize = options.Legend ? 40 : 0;

            var bounds = new Rect(0, 0, options.Size.Width, options.Size.Height);
            ctx.PushClip(new RectangleGeometry(bounds));
            ctx.DrawRectangle(options.Background, null, bounds);
            double halfWidth = bounds.Width / 2;

            Brush brushLabels = options.IsWriteToFile ? Brushes.Black : LabelsBrush;
            Brush brushLines = options.IsWriteToFile ? Brushes.Gray : LinesBrush;
            Brush jupiterBrush = options.IsWriteToFile ? Brushes.DimGray : JupiterBrush;

            int daysCount = (Positions?.Count ?? 0) / 24;
            double dayHeight = ((bounds.Height - headerSize - legendSize) / daysCount * 5.0) * (options.VerticalScale / 5.0);
            double jupRadius = (bounds.Width / 110.0) * (options.HorizontalScale / 5.0);
            double daysOffset = DaysOffset;

            if (options.IsWriteToFile)
            {
                if (options.Header)
                {
                    double y = headerSize / 2 - daysOffset * dayHeight + (daysOffset / daysCount) * (bounds.Height - headerSize - legendSize);
                    DrawText(ctx, Header, new Point(halfWidth, y), headerSize * 0.5, brushLabels);
                }
                if (options.Legend)
                {
                    double y = headerSize - daysOffset * dayHeight + (daysOffset / daysCount) * (bounds.Height - headerSize - legendSize) + daysCount * dayHeight + legendSize * 0.65;
                    for (int m=0; m<4; m++)
                    {
                        double x = bounds.Width / 8 + m * bounds.Width / 4;
                        DrawText(ctx, names[m], new Point(bounds.Width / 8 + m * bounds.Width / 4, y), 10, brushLabels);
                        ctx.DrawLine(new Pen(curvesBrush[0][m], 2), new Point(x - 20, y - 10), new Point(x + 20, y - 10));
                    }
                }
            }

            if (!options.IsWriteToFile)
            {
                ctx.DrawLine(new Pen(BorderBrush, BorderThickness.Left), new Point(0, 0), new Point(0, bounds.Height));
                ctx.DrawLine(new Pen(BorderBrush, BorderThickness.Right), new Point(bounds.Width, 0), new Point(bounds.Width, bounds.Height));
                ctx.DrawLine(new Pen(BorderBrush, BorderThickness.Top), new Point(0, 0), new Point(bounds.Width, 0));
                ctx.DrawLine(new Pen(BorderBrush, BorderThickness.Bottom), new Point(0, bounds.Height), new Point(bounds.Width, bounds.Height));
            }

            if (Positions != null && Positions.Any())
            {                
                // Jupiter bounds
                {
                    double y1 = headerSize - daysOffset * dayHeight + (daysOffset / daysCount) * (bounds.Height - headerSize - legendSize);
                    double y2 = y1 + daysCount * dayHeight;

                    var pen = new Pen(jupiterBrush, 0.25) { DashStyle = new DashStyle(new double[] { 5, 5 }, 5) };
                    ctx.DrawLine(pen, new Point(halfWidth - jupRadius, y1), new Point(halfWidth - jupRadius, y2));
                    ctx.DrawLine(pen, new Point(halfWidth + jupRadius, y1), new Point(halfWidth + jupRadius, y2));
                }

                // Grid
                int dayLabelSize = 10;
                int prevDayLabel = -10;
                for (int d = 0; d <= daysCount; d++)
                {
                    double y = headerSize - daysOffset * dayHeight + (daysOffset / daysCount) * (bounds.Height - headerSize - legendSize) + d * dayHeight;
                    ctx.DrawLine(new Pen(brushLines, 0.5), new Point(20, y), new Point(bounds.Size.Width, y));
                    int day = (d + 1) % (daysCount + 1);
                    if (day > 1)
                    {
                        bool needLabel = true;
                        if (dayLabelSize * 1.5 / dayHeight >= 1 && (double)(day - prevDayLabel) * dayHeight < dayLabelSize * 1.5)
                        {
                            needLabel = false;
                        }

                        if (needLabel)
                        {
                            prevDayLabel = day;
                            DrawText(ctx, day.ToString(), new Point(10, y), dayLabelSize, brushLabels);
                        }
                    }
                }

                // Curves
                for (int m = 0; m < 4; m++)
                {
                    if (m == 0 && !ShowIo) continue;
                    if (m == 1 && !ShowEuropa) continue;
                    if (m == 2 && !ShowGanymede) continue;
                    if (m == 3 && !ShowCallisto) continue;

                    var points = new List<Point>();
                    var curve = new PathFigure();
                    int h = 0;

                    bool mirrored = Orientation != ChartOrientation.Direct;

                    foreach (var pos in Positions)
                    {
                        double x = halfWidth + pos[m, 0].X * jupRadius * (mirrored ? -1 : 1);
                        double y = headerSize - daysOffset * dayHeight + (daysOffset / daysCount) * (bounds.Height - headerSize - legendSize) + (h / 24.0) * dayHeight;
                        points.Add(new Point(x, y));

                        if (!options.IsWriteToFile && pos == CurrentPositions)
                        {
                            ctx.DrawLine(new Pen(CurrentDateBrush, options.IsWriteToFile ? 2 : 1), new Point(20, y), new Point(bounds.Width, y));
                        }
                        h++;
                    }

                    curve.StartPoint = points.First();
                    curve.Segments.Add(new PolyLineSegment(points, true));
                    curve.IsFilled = false;
                    curve.IsClosed = false;

                    var path = new PathGeometry();
                    path.Figures.Add(curve);

                    ctx.DrawGeometry(null, new Pen(options.IsWriteToFile ? curvesBrush[0][m] : CurvesBrush[m], 1), path);
                }
            }
        }

        protected override void OnRender(DrawingContext ctx)
        {
            renderOptions.Size = new Size(ActualWidth, ActualHeight);
            renderOptions.HorizontalScale = HorizontalScale;
            renderOptions.VerticalScale = VerticalScale;
            DoRender(ctx, renderOptions);            
        }

        public System.Drawing.Bitmap WriteToBitmap(WriteToBitmapOptions options)
        {
            int daysCount = Positions.Count / 24;
            double dayHeight = (ActualHeight / daysCount * 5.0) * (VerticalScale / 5.0);

            int width = (int)ActualWidth;
            int height = (int)(dayHeight * daysCount);

            var renderBitmap = new RenderTargetBitmap(width, height, 96.0, 96.0, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                options.Size = new Size(width, height);
                options.Background = Brushes.White;
                options.HorizontalScale = HorizontalScale;
                options.VerticalScale = 1;

                DoRender(dc, options);
            }
            renderBitmap.Render(dv);
            return ToWinFormsBitmap(renderBitmap);
        }

        private System.Drawing.Bitmap ToWinFormsBitmap(BitmapSource bitmapsource)
        {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(
                bitmapsource.PixelWidth,
                bitmapsource.PixelHeight,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            var data = bmp.LockBits(
                new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            bitmapsource.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);

            bmp.UnlockBits(data);

            return bmp;
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsLocked = !IsLocked;
                InvalidateVisual();
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (!IsLocked)
            {
                CurrentPositions = null;
                CurrentPosition = -1;
                InvalidateVisual();
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (Positions != null && Positions.Any() && !IsLocked)
            {
                int daysCount = Positions.Count / 24;
                double dayHeight = (ActualHeight / daysCount * 5.0) * (VerticalScale / 5.0);
                double daysOffset = DaysOffset;

                var p = e.GetPosition(this);

                int h = (int)(Math.Floor(p.Y - (-daysOffset * dayHeight + (daysOffset / daysCount) * ActualHeight)) / dayHeight * 24);
                CurrentPositions = Positions.ElementAt(h);
                CurrentPosition = h;

                InvalidateVisual();
            }
        }

        private void DrawText(DrawingContext ctx, string text, Point point, double size, Brush brush )
        {
            FormattedText formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, font, size, brush);
            ctx.DrawText(formattedText, new Point(point.X - formattedText.Width / 2, point.Y - formattedText.Height / 2));
        }
    }
}
