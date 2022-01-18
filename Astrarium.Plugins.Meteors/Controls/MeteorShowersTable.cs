using Astrarium.Algorithms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Astrarium.Plugins.Meteors.Controls
{
    public class MeteorShowersTable : DataGrid
    {
        public static readonly DependencyProperty MoonPhaseDataProperty =
            DependencyProperty.Register(nameof(MoonPhaseData), typeof(ICollection<float>), typeof(MeteorShowersTable), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty YearProperty =
            DependencyProperty.Register(nameof(Year), typeof(int), typeof(MeteorShowersTable), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty JulianDayProperty =
            DependencyProperty.Register(nameof(JulianDay), typeof(double), typeof(MeteorShowersTable), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty RowDoubleClickProperty =
            DependencyProperty.Register(nameof(RowDoubleClick), typeof(ICommand), typeof(MeteorShowersTable), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public static readonly DependencyProperty IsDarkModeProperty =
            DependencyProperty.Register(nameof(IsDarkMode), typeof(bool), typeof(MeteorShowersTable), new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = false, AffectsRender = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        public ICollection<float> MoonPhaseData
        {
            get => (ICollection<float>)GetValue(MoonPhaseDataProperty);
            set => SetValue(MoonPhaseDataProperty, value);
        }

        public int Year
        {
            get => (int)GetValue(YearProperty);
            set => SetValue(YearProperty, value);
        }

        public double JulianDay
        {
            get => (double)GetValue(JulianDayProperty);
            set => SetValue(JulianDayProperty, value);
        }

        public ICommand RowDoubleClick
        {
            get => (ICommand)GetValue(RowDoubleClickProperty);
            set => SetValue(RowDoubleClickProperty, value);
        }

        public bool IsDarkMode
        {
            get => (bool)GetValue(IsDarkModeProperty);
            set => SetValue(IsDarkModeProperty, value);
        }

        private ScrollViewer scrollViewer;

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            PropertyDescriptor pd = DependencyPropertyDescriptor.FromProperty(ScrollViewer.HorizontalOffsetProperty, typeof(ScrollViewer));
            scrollViewer = GetVisualChild<ScrollViewer>(this);
            pd.AddValueChanged(scrollViewer, ScrollPositionChanged);
        }

        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default;
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PropertyDescriptor pd = DependencyPropertyDescriptor.FromProperty(DataGridColumn.ActualWidthProperty, typeof(DataGridColumn));
            for (int c = 0; c < Columns.Count; c++)
            {
                pd.AddValueChanged(Columns[c], ColumnWidthPropertyChanged);
            }
            Background = new SolidColorBrush(Colors.Transparent);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            var row = ContainerFromElement(this, e.OriginalSource as DependencyObject) as DataGridRow;
            if (row != null && CurrentColumn.IsFrozen)
            {
                if (e.LeftButton == MouseButtonState.Pressed && SelectedItem != null)
                {
                    RowDoubleClick?.Execute(SelectedItem);
                }
            }
        }

        private void ColumnWidthPropertyChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        private void ScrollPositionChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
        }

        private Point mousePosition;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            mousePosition = e.GetPosition(this);
            double x = mousePosition.X;
            double offset = scrollViewer?.HorizontalOffset ?? 0;
            double of = CellsPanelHorizontalOffset;
            double start = Columns.Take(Columns.Count - 1).Select(c => c.ActualWidth).Sum() + of * 2;
            double width = Columns.Last().ActualWidth;
            int daysCount = MoonPhaseData.Count;
            double i = (x - start + offset) / width * daysCount;
            if (i >= 0 && i < daysCount)
            {
                JulianDay = Date.JulianDay0(Year) + i + 1;
            }
            else
            {
                JulianDay = -1;
            }
            InvalidateVisual();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            JulianDay = -1;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (MoonPhaseData == null) return;

            int daysCount = MoonPhaseData.Count;

            double jd0 = new Date(Year, 1, 1).ToJulianDay();
            double offset = scrollViewer?.HorizontalOffset ?? 0;
            double of = CellsPanelHorizontalOffset;
            double frozenColsWidth = Columns.Take(Columns.Count - 1).Select(c => c.ActualWidth).Sum();
            double width = Columns.Last().ActualWidth; 
            double start = frozenColsWidth + of * 2;
            double right = scrollViewer?.ComputedVerticalScrollBarVisibility == System.Windows.Visibility.Visible ? 24 : 0;
            double ww = ActualWidth - frozenColsWidth - right;

            if (ww > 0)
            {
                Color color = IsDarkMode ? Color.FromArgb(255, 80, 0, 0) : Color.FromArgb(255, 80, 80, 80);
                var solidPen = new Pen(new SolidColorBrush(color), 0.5);
                var dotPen = new Pen(new SolidColorBrush(color), 0.2);
                var bounds = new Rect(start, 0, ww, ActualHeight);
                drawingContext.PushClip(new RectangleGeometry(bounds));
                drawingContext.DrawRectangle(Brushes.Black, null, bounds);

                for (int i = 0; i <= daysCount; i++)
                {
                    var d = new Date(jd0 + i);
                    double x = start + (double)i / daysCount * width - offset;

                    if (MoonPhaseData != null && i < MoonPhaseData.Count)
                    {
                        float phase = MoonPhaseData.ElementAt(i);
                        byte transp = (byte)(int)(phase * 200);

                        Color moonColor = IsDarkMode ? Color.FromArgb(transp, 100, 0, 0) : Color.FromArgb(transp, 100, 100, 100);
                        var brush = new SolidColorBrush(moonColor);
                        drawingContext.DrawRectangle(brush, null, new Rect(new Point(x, 0), new Size(width / daysCount, ActualHeight)));
                    }

                    drawingContext.DrawLine(d.Day == 1 ? solidPen : dotPen, new Point(x, 0), new Point(x, ActualHeight));
                }
            }

            drawingContext.DrawLine(new Pen(new SolidColorBrush(Colors.Red), 0.5), new Point(mousePosition.X, 0), new Point(mousePosition.X, ActualHeight));
        }
    }
}
