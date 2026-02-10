using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;

namespace Astrarium.Plugins.SolarSystem.Views
{
    public class LunarCalendarView : Grid
    {
        public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register(nameof(SelectedDate), typeof(Date), typeof(LunarCalendarView), new PropertyMetadata(Date.Now, OnPropertyChanged));
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(LunarCalendarView), new PropertyMetadata(null, OnPropertyChanged));
        public static readonly DependencyProperty DayCellTemplateProperty = DependencyProperty.Register(nameof(DayCellTemplate), typeof(DataTemplate), typeof(LunarCalendarView), new PropertyMetadata(null, OnPropertyChanged));
        public static readonly DependencyProperty HeaderCellTemplateProperty = DependencyProperty.Register(nameof(HeaderCellTemplate), typeof(DataTemplate), typeof(LunarCalendarView), new PropertyMetadata(null, OnPropertyChanged));
        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(LunarCalendarView), new PropertyMetadata(Brushes.Transparent, OnPropertyChanged));
       
        public Date SelectedDate
        {
            get => (Date)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LunarCalendarView)d;
            control.UpdateDate();
            control.UpdateGridLayout();
            control.InvalidateVisual();
        }

        public DataTemplate DayCellTemplate
        {
            get => (DataTemplate)GetValue(DayCellTemplateProperty);
            set => SetValue(DayCellTemplateProperty, value);
        }

        public DataTemplate HeaderCellTemplate
        {
            get => (DataTemplate)GetValue(HeaderCellTemplateProperty);
            set => SetValue(HeaderCellTemplateProperty, value);
        }

        private void UpdateGridLayout()
        {
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();

            // day of week header
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

            // days grid layout
            for (int row = 0; row < WeeksCount; row++)
            {
                RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            }
            for (int col = 0; col < 7; col++)
            {
                ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }

            Children.Clear();

            for (int col = 0; col < 7; col++)
            {
                if (HeaderCellTemplate != null)
                {
                    FrameworkElement cell = HeaderCellTemplate.LoadContent() as FrameworkElement;
                    cell.DataContext = (DayOfWeek)((col + (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek) % 7);
                    AddChildToCell(cell, col, 0);
                }
            }

            int currentDay = 0;
            var enumerator = ItemsSource?.GetEnumerator();

            if (enumerator != null)
            {
                for (int row = 0; row < WeeksCount; row++)
                {
                    for (int col = 0; col < 7; col++)
                    {
                        if (row == 0 && col < FirstDayOffset) continue;
                        if (currentDay >= DaysInMonth) continue;

                        if (enumerator.MoveNext())
                        {
                            var item = enumerator.Current;
                            if (DayCellTemplate != null && item != null)
                            {
                                FrameworkElement cell = DayCellTemplate.LoadContent() as FrameworkElement;
                                cell.DataContext = item;
                                AddChildToCell(cell, col, row + 1);
                            }
                        }
                    }
                }
            }
        }

        private void AddChildToCell(UIElement child, int col, int row)
        {
            SetColumn(child, col);
            SetRow(child, row);
            Children.Add(child);
        }

        protected override void OnRender(DrawingContext dc)
        {
            var borderPen = new Pen(BorderBrush, 1);
            Rect bounds = new Rect(0, 0, ActualWidth, ActualHeight);
            dc.DrawRectangle(Background, null, bounds);
            
            for (int col = 0; col < ColumnDefinitions.Count; col++)
            {
                double x = ColumnDefinitions.Take(col).Sum(c => c.ActualWidth);
                dc.DrawLine(borderPen, new Point(x, bounds.Top), new Point(x, bounds.Bottom));
            }
            dc.DrawLine(borderPen, new Point(bounds.Right, bounds.Top), new Point(bounds.Right, bounds.Bottom));

            for (int row = 0; row < RowDefinitions.Count; row++)
            {
                double y = RowDefinitions.Take(row).Sum(r => r.ActualHeight);
                dc.DrawLine(borderPen, new Point(0, y), new Point(bounds.Right, y));
            }
            dc.DrawLine(borderPen, new Point(0, bounds.Bottom), new Point(bounds.Right, bounds.Bottom));
        }

        private void UpdateDate()
        {
            var date = SelectedDate;
            int year = date.Year;
            int month = date.Month;
            DaysInMonth = DateTime.DaysInMonth(year, month);
            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            FirstDayWeek = firstDayOfMonth.DayOfWeek;

            int fd = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            FirstDayOffset = (FirstDayWeek == DayOfWeek.Sunday ? 7 - fd : (int)FirstDayWeek - fd) % 7;

            int totalCells = FirstDayOffset + DaysInMonth;
            WeeksCount = (int)Math.Ceiling(totalCells / 7.0);
        }

        /// <summary>
        /// Number of weeks in selected month
        /// </summary>
        private int WeeksCount;

        /// <summary>
        /// DayOfWeek of the first day of selected month
        /// </summary>
        private DayOfWeek FirstDayWeek;

        /// <summary>
        /// Days count for selected month
        /// </summary>
        private int DaysInMonth;

        /// <summary>
        /// Numeric offset in the grid for the first day of month
        /// </summary>
        private int FirstDayOffset;
    }
}
