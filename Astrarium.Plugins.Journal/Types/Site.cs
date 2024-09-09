using Astrarium.Plugins.Journal.Database.Entities;

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

        [DBStored(Entity = typeof(SiteDB), Field = "Latitude")]
        public double Latitude
        {
            get => GetValue<double>(nameof(Latitude));
            set => SetValue(nameof(Latitude), value);
        }

        [DBStored(Entity = typeof(SiteDB), Field = "Longitude")]
        public double Longitude
        {
            get => GetValue<double>(nameof(Longitude));
            set => SetValue(nameof(Longitude), value);
        }

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}
