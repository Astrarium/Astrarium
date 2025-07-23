using Astrarium.Algorithms;
using Astrarium.Types;
using System;

namespace Astrarium.ViewModels
{
    /// <summary>
    /// Defines ViewModel for the <see cref="Views.ViewAngleWindow"/> View. 
    /// </summary>
    public class ViewAngleVM : ViewModelBase
    {
        /// <summary>
        /// Sky map reference
        /// </summary>
        private readonly ISkyMap map;

        /// <summary>
        /// Default value of view angle
        /// </summary>
        private double defaultViewAngle;

        private bool applyImmediately;

        /// <summary>
        /// Handles "Close" button
        /// </summary>
        public Command CloseCommand { get; private set; }

        /// <summary>
        /// Handles "Select" / "Apply" button (positive dialog result)
        /// </summary>
        public Command SelectCommand { get; private set; }

        /// <summary>
        /// Creates new instance of the ViewModel
        /// </summary>
        public ViewAngleVM(ISkyMap map)
        {
            this.map = map;
            CloseCommand = new Command(Close);
            SelectCommand = new Command(Select);
        }

        /// <summary>
        /// Sets default values for the view model
        /// </summary>
        /// <param name="viewAngle">View angle, in degrees</param>
        /// <param name="min">Minimal allowed value of view angle, in degrees</param>
        /// <param name="max">Maximal allowed value of view angle, in degrees</param>
        /// <param name="applyImmediately">Apply view angle changes immediately</param>
        public ViewAngleVM WithDefaults(double viewAngle, double min, double max, bool applyImmediately)
        {
            MinFov = min;
            MaxFov = max;
            ViewAngle = viewAngle;
            defaultViewAngle = viewAngle;
            this.applyImmediately = applyImmediately;
            return this;
        }

        /// <summary>
        /// Selected View Angle, in degrees
        /// </summary>
        public double ViewAngle 
        { 
            get => GetValue<double>(nameof(ViewAngle));
            set
            {
                SetValue(nameof(ViewAngle), value);
                var dms = new DMS(value);
                SetValue(nameof(Degrees), Math.Ceiling((decimal)dms.Degrees));
                SetValue(nameof(Minutes), Math.Ceiling((decimal)dms.Minutes));
                SetValue(nameof(Seconds), Math.Ceiling((decimal)dms.Seconds));

                if (applyImmediately)
                {
                    map.Projection.Fov = value;
                    map.Invalidate();
                }
            }
        }

        /// <summary>
        /// Degrees portion of ViewAngle
        /// </summary>
        public decimal Degrees
        {
            get => GetValue(nameof(Degrees), 0m);
            set
            {
                SetValue(nameof(Degrees), (decimal)(uint)value);
                UpdateViewAngle();
            }
        }

        /// <summary>
        /// Minutes portion of ViewAngle
        /// </summary>
        public decimal Minutes
        {
            get => GetValue(nameof(Minutes), 0m);
            set
            {
                SetValue(nameof(Minutes), Math.Min(59, (decimal)(uint)value));
                UpdateViewAngle();
            }
        }

        /// <summary>
        /// Seconds portion of ViewAngle
        /// </summary>
        public decimal Seconds
        {
            get => GetValue(nameof(Seconds), 0m);
            set
            {
                SetValue(nameof(Seconds), Math.Min(59, (decimal)(uint)value));
                UpdateViewAngle();
            }
        }

        /// <summary>
        /// Updates ViewAngle value when one of portions (degrees, minutes, seconds) is changed
        /// </summary>
        private void UpdateViewAngle()
        {
            double value = new DMS((uint)Degrees, (uint)Minutes, (uint)Seconds).ToDecimalAngle();
            if (value < MinFov) 
                ViewAngle = MinFov;
            else if (value > MaxFov)
                ViewAngle = MaxFov;
            else
                SetValue(nameof(ViewAngle), value);
        }

        /// <summary>
        /// Minimal allowed value of view angle, in degrees
        /// </summary>
        public double MinFov
        {
            get => GetValue(nameof(MinFov), 0.0);
            private set => SetValue(nameof(MinFov), value);
        }

        /// <summary>
        /// Maximal allowed value of view angle, in degrees
        /// </summary>
        public double MaxFov
        {
            get => GetValue(nameof(MaxFov), 0.0);
            private set => SetValue(nameof(MaxFov), value);
        }

        /// <summary>
        /// Handler for <see cref="SelectCommand"/>
        /// </summary>
        private void Select()
        {
            defaultViewAngle = ViewAngle;
            if (applyImmediately)
            {
                map.Projection.Fov = defaultViewAngle;
                map.Invalidate();
            }
            Close(true);
        }

        public override void Close()
        {
            if (applyImmediately)
            {
                map.Projection.Fov = defaultViewAngle;
                map.Invalidate();
            }
            base.Close();
        }

        /// <summary>
        /// Logging is not required
        /// </summary>
        public override bool Loggable => false;
    }
}
