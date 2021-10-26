using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private ScrollViewer scrollViewer;

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            PropertyDescriptor pd = DependencyPropertyDescriptor.FromProperty(ScrollViewer.HorizontalOffsetProperty, typeof(ScrollViewer));
            scrollViewer = GetVisualChild<ScrollViewer>(this);
            pd.AddValueChanged(scrollViewer, ScrollPositionChanged);
        }

        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);

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

            {
                PropertyDescriptor pd = DependencyPropertyDescriptor.FromProperty(DataGridColumn.ActualWidthProperty, typeof(DataGridColumn));
                for (int c = 0; c < Columns.Count; c++)
                {
                    pd.AddValueChanged(Columns[c], ColumnWidthPropertyChanged);
                }
            }

            Background = new SolidColorBrush(Colors.Transparent);
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

            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (MoonPhaseData == null) return;

            int daysCount = MoonPhaseData.Count;

            var jd0 = new Date(Year, 1, 1).ToJulianDay();

            double offset = scrollViewer?.HorizontalOffset ?? 0;

            var of = this.CellsPanelHorizontalOffset;
            
            var width = this.Columns[2].ActualWidth;
            double start = this.Columns[0].ActualWidth + this.Columns[1].ActualWidth + of * 2;

            var right = scrollViewer?.ComputedVerticalScrollBarVisibility == System.Windows.Visibility.Visible ? 24 : 0;

            double ww = ActualWidth - (this.Columns[0].ActualWidth + this.Columns[1].ActualWidth) - right;

            if (ww > 0)
            {
                var solidPen = new Pen(new SolidColorBrush(Color.FromArgb(255, 80, 80, 80)), 0.5);
                var dotPen = new Pen(new SolidColorBrush(Color.FromArgb(255, 80, 80, 80)), 0.2);

                var bounds = new Rect(start, 0, ww, ActualHeight);
                drawingContext.PushClip(new RectangleGeometry(bounds));

                drawingContext.DrawRectangle(Brushes.Black, null, bounds);

                for (int i = 0; i <= daysCount; i++)
                {
                    var d = new Date(jd0 + i);

                    double x = start + (double)i / daysCount * width - offset;

                    if (MoonPhaseData != null && i < MoonPhaseData.Count)
                    {
                        var phase = MoonPhaseData.ElementAt(i);
                        byte transp = (byte)((int)(phase * 200));

                        var brush = new SolidColorBrush(Color.FromArgb(transp, 100, 100, 100));
                        drawingContext.DrawRectangle(brush, null, new Rect(new Point(x, 0), new Size(width / daysCount, ActualHeight)));
                    }

                    drawingContext.DrawLine(d.Day == 1 ? solidPen : dotPen, new Point(x, 0), new Point(x, ActualHeight));
                }
            }

            drawingContext.DrawLine(new Pen(new SolidColorBrush(Colors.Red), 0.5), new Point(mousePosition.X, 0), new Point(mousePosition.X, ActualHeight));

        }
    }
}
