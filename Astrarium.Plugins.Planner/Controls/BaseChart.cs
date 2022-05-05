using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Astrarium.Plugins.Planner.Controls
{
    public abstract class BaseChart : Canvas
    {
        public readonly static DependencyProperty ShowChartProperty = DependencyProperty.Register(nameof(ShowChart), typeof(bool), typeof(BaseChart), new FrameworkPropertyMetadata(defaultValue: false) { AffectsRender = true, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DepPropertyChanged) });
        public readonly static DependencyProperty SunCoordinatesProperty = DependencyProperty.Register(nameof(SunCoordinates), typeof(CrdsEquatorial), typeof(BaseChart), new FrameworkPropertyMetadata(defaultValue: null) { AffectsRender = true, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DepPropertyChanged) });
        public readonly static DependencyProperty BodyCoordinatesProperty = DependencyProperty.Register(nameof(BodyCoordinates), typeof(CrdsEquatorial), typeof(BaseChart), new FrameworkPropertyMetadata(defaultValue: null) { AffectsRender = true, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DepPropertyChanged) });
        public readonly static DependencyProperty GeoLocationProperty = DependencyProperty.Register(nameof(GeoLocation), typeof(CrdsGeographical), typeof(BaseChart), new FrameworkPropertyMetadata(defaultValue: null) { AffectsRender = true, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DepPropertyChanged) });
        public readonly static DependencyProperty SiderealTimeProperty = DependencyProperty.Register(nameof(SiderealTime), typeof(double), typeof(BaseChart), new FrameworkPropertyMetadata(0.0) { AffectsRender = true, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged, PropertyChangedCallback = new PropertyChangedCallback(DepPropertyChanged) });
        public readonly static DependencyProperty FromTimeProperty = DependencyProperty.Register(nameof(FromTime), typeof(TimeSpan), typeof(BaseChart), new FrameworkPropertyMetadata(TimeSpan.Zero) { AffectsRender = true, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });
        public readonly static DependencyProperty ToTimeProperty = DependencyProperty.Register(nameof(ToTime), typeof(TimeSpan), typeof(BaseChart), new FrameworkPropertyMetadata(TimeSpan.Zero) { AffectsRender = true, BindsTwoWayByDefault = true, DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });

        protected CrdsHorizontal[] bodyCoordinatesInterpolated = null;
        protected CrdsHorizontal[] sunCoordinatesInterpolated = null;

        private static void DepPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((BaseChart)sender).Interpolate();
        }

        public bool ShowChart
        {
            get => (bool)GetValue(ShowChartProperty);
            set => SetValue(ShowChartProperty, value);
        }

        public CrdsGeographical GeoLocation
        {
            get => (CrdsGeographical)GetValue(GeoLocationProperty);
            set => SetValue(GeoLocationProperty, value);
        }

        public double SiderealTime
        {
            get => (double)GetValue(SiderealTimeProperty);
            set => SetValue(SiderealTimeProperty, value);
        }

        public CrdsEquatorial SunCoordinates
        {
            get => (CrdsEquatorial)GetValue(SunCoordinatesProperty);
            set => SetValue(SunCoordinatesProperty, value);
        }

        public CrdsEquatorial BodyCoordinates
        {
            get => (CrdsEquatorial)GetValue(BodyCoordinatesProperty);
            set => SetValue(BodyCoordinatesProperty, value);
        }

        public TimeSpan FromTime
        {
            get => (TimeSpan)GetValue(FromTimeProperty);
            set => SetValue(FromTimeProperty, value);
        }

        public TimeSpan ToTime
        {
            get => (TimeSpan)GetValue(ToTimeProperty);
            set => SetValue(ToTimeProperty, value);
        }

        private void Interpolate()
        {
            if (SunCoordinates != null && GeoLocation != null)
            {
                sunCoordinatesInterpolated = Interpolate(SunCoordinates, GeoLocation, SiderealTime, 120);
                InvalidateVisual();
            }
            else
            {
                sunCoordinatesInterpolated = null;
                InvalidateVisual();
            }

            if (BodyCoordinates != null && GeoLocation != null)
            {
                bodyCoordinatesInterpolated = Interpolate(BodyCoordinates, GeoLocation, SiderealTime, 120);
                InvalidateVisual();
            }
            else
            {
                bodyCoordinatesInterpolated = null;
                InvalidateVisual();
            }
        }

        private CrdsHorizontal[] Interpolate(CrdsEquatorial eq, CrdsGeographical location, double theta0, int count)
        {
            List<CrdsHorizontal> hor = new List<CrdsHorizontal>();
            for (int i = 0; i <= count; i++)
            {
                double n = i / (double)count;
                double sidTime = Angle.To360(theta0 + n * 360.98564736629);
                hor.Add(eq.ToHorizontal(location, sidTime));
            }

            return hor.ToArray();
        }
    }
}
