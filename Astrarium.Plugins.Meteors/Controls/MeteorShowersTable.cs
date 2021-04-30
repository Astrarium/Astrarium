using Astrarium.Algorithms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Astrarium.Plugins.Meteors.Controls
{
    public class MeteorShowersTable : DataGrid
    {
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

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var jd0 = new Date(2001, 1, 1).ToJulianDay();


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


                drawingContext.PushClip(new RectangleGeometry(new Rect(start, 0, ww, ActualHeight)));

                for (int i = 0; i <= 365; i++)
                {
                    var d = new Date(jd0 + i);

                    

                    double x = start + i / 365.0 * width - offset;
                    drawingContext.DrawLine(d.Day == 1 ? solidPen : dotPen, new Point(x, 0), new Point(x, ActualHeight));
                }
            }

        }
    }
}
