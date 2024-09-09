using Astrarium.Plugins.Journal.Database.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public class Session : JournalEntity
    {
        public string Id { get; private set; }

        public Session(string id)
        {
            Id = id;
        }

        public override DateTime SessionDate => Begin.DateTime.Date;

        public bool IsExpanded
        {
            get => GetValue(nameof(IsExpanded), false);
            set => SetValue(nameof(IsExpanded), value);
        }

        [DBStored(Entity = typeof(SessionDB), Field = "Comments")]
        public string Comments
        {
            get => GetValue<string>(nameof(Comments), null);
            set => SetValue(nameof(Comments), value);
        }

        [DBStored(Entity = typeof(SessionDB), Field = "SiteId")]
        public string SiteId
        {
            get => GetValue<string>(nameof(SiteId));
            set => SetValue(nameof(SiteId), value);
        }

        [DBStored(Entity = typeof(SessionDB), Field = "Weather")]
        public string Weather
        {
            get => GetValue<string>(nameof(Weather), null);
            set => SetValue(nameof(Weather), value);
        }

        /// <summary>
        /// Seeing (Antoniadi scale)
        /// </summary>
        [DBStored(Entity = typeof(SessionDB), Field = "Seeing")]
        public int? Seeing
        {
            get => GetValue<int?>(nameof(Seeing), null);
            set => SetValue(nameof(Seeing), value);
        }

        /// <summary>
        /// Flag indicating sky quality field is specified
        /// </summary>
        public bool SkyQualitySpecified
        {
            get => GetValue(nameof(SkyQualitySpecified), false);
            set
            {
                SetValue(nameof(SkyQualitySpecified), value);
                NotifyPropertyChanged(nameof(skyQuality));
            }
        }

        /// <summary>
        /// This used only for DB storing
        /// </summary>
        [DBStored(Entity = typeof(SessionDB), Field = "FaintestStar")]
        private double? faintestStar => FaintestStarSpecified ? (double)FaintestStar : (double?)null;

        public decimal FaintestStar
        {
            get => GetValue(nameof(FaintestStar), 6.0m);
            set
            {
                SetValue(nameof(FaintestStar), value);
                NotifyPropertyChanged(nameof(faintestStar));
            }
        }

        public bool FaintestStarSpecified
        {
            get => GetValue(nameof(FaintestStarSpecified), false);
            set
            {
                SetValue(nameof(FaintestStarSpecified), value);
                NotifyPropertyChanged(nameof(faintestStar));
            }
        }

        /// <summary>
        /// This used only for DB storing
        /// </summary>
        [DBStored(Entity = typeof(SessionDB), Field = "SkyQuality")]
        private double? skyQuality => SkyQualitySpecified ? (double)SkyQuality : (double?)null;

        public decimal SkyQuality
        {
            get => GetValue(nameof(SkyQuality), 19m);
            set
            {
                SetValue(nameof(SkyQuality), value);
                NotifyPropertyChanged(nameof(skyQuality));
            }
        }

        [DBStored(Entity = typeof(SessionDB), Field = "Equipment")]
        public string Equipment
        {
            get => GetValue<string>(nameof(Equipment), null);
            set => SetValue(nameof(Equipment), value);
        }

        public int ObservationsCount
        {
            get => Observations.Count;
        }

        public ObservableCollection<Observation> Observations { get; private set; } = new ObservableCollection<Observation>();
    }
}
