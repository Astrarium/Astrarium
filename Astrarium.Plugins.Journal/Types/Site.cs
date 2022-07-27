using Astrarium.Algorithms;
using Astrarium.Plugins.Journal.Database.Entities;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public class Site : PersistantEntity
    {
        public string Id
        {
            get => GetValue<string>(nameof(Id));
            set => SetValue(nameof(Id), value);
        }

        [DBStored(Entity = typeof(SiteDB), Field = "Name")]
        public string Name
        {
            get => GetValue<string>(nameof(Name));
            set => SetValue(nameof(Name), value);
        }

        [DBStored(Entity = typeof(SiteDB), Field = "Timezone")]
        public double Timezone
        {
            get => GetValue(nameof(Timezone), 0.0);
            set => SetValue(nameof(Timezone), value);
        }

        [DBStored(Entity = typeof(SiteDB), Field = "Elevation")]
        public double Elevation
        {
            get => GetValue(nameof(Elevation), 0.0);
            set => SetValue(nameof(Elevation), value);
        }

        [DBStored(Entity = typeof(SiteDB), Field = "IAUCode")]
        public string IAUCode
        {
            get => GetValue<string>(nameof(IAUCode));
            set => SetValue(nameof(IAUCode), value);
        }

        public bool LatitudePositive
        {
            get => GetValue(nameof(LatitudePositive), true);
            set
            {
                SetValue(nameof(LatitudePositive), value);
                NotifyPropertyChanged(nameof(Latitude));
            }
        }

        public decimal LatitudeDeg
        {
            get => GetValue(nameof(LatitudeDeg), 0m);
            set
            {
                SetValue(nameof(LatitudeDeg), value);
                NotifyPropertyChanged(nameof(Latitude));
            }
        }

        public decimal LatitudeMin
        {
            get => GetValue(nameof(LatitudeMin), 0m);
            set
            {
                SetValue(nameof(LatitudeMin), value);
                NotifyPropertyChanged(nameof(Latitude));
            }
        }

        public decimal LatitudeSec
        {
            get => GetValue(nameof(LatitudeSec), 0m);
            set
            {
                SetValue(nameof(LatitudeSec), value);
                NotifyPropertyChanged(nameof(Latitude));
            }
        }

        [DBStored(Entity = typeof(SiteDB), Field = "Latitude")]
        public double Latitude
        {
            get => (LatitudePositive ? 1 : -1) * new DMS((uint)(double)LatitudeDeg, (uint)(double)LatitudeMin, (double)LatitudeSec).ToDecimalAngle();
            set
            {
                LatitudePositive = Math.Sign(value) >= 0;
                var lat = new DMS(Math.Abs(value));
                LatitudeDeg = lat.Degrees;
                LatitudeMin = lat.Minutes;
                LatitudeSec = (decimal)lat.Seconds;
            }
        }

        public bool LongitudePositive
        {
            get => GetValue(nameof(LongitudePositive), true);
            set
            {
                SetValue(nameof(LongitudePositive), value);
                NotifyPropertyChanged(nameof(Longitude));
            }
        }

        public decimal LongitudeDeg
        {
            get => GetValue(nameof(LongitudeDeg), 0m);
            set
            {
                SetValue(nameof(LongitudeDeg), value);
                NotifyPropertyChanged(nameof(Longitude));
            }
        }

        public decimal LongitudeMin
        {
            get => GetValue(nameof(LongitudeMin), 0m);
            set
            {
                SetValue(nameof(LongitudeMin), value);
                NotifyPropertyChanged(nameof(Longitude));
            }
        }

        public decimal LongitudeSec
        {
            get => GetValue(nameof(LongitudeSec), 0m);
            set
            {
                SetValue(nameof(LongitudeSec), value);
                NotifyPropertyChanged(nameof(Longitude));
            }
        }

        [DBStored(Entity = typeof(SiteDB), Field = "Longitude")]
        public double Longitude
        {
            get => (LongitudePositive ? 1 : -1) * new DMS((uint)(double)LongitudeDeg, (uint)(double)LongitudeMin, (double)LongitudeSec).ToDecimalAngle();
            set
            {
                LongitudePositive = Math.Sign(value) >= 0;
                var lon = new DMS(Math.Abs(value));
                LongitudeDeg = lon.Degrees;
                LongitudeMin = lon.Minutes;
                LongitudeSec = (decimal)lon.Seconds;
            }
        }

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}
