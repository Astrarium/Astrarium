using Astrarium.Plugins.Journal.Database.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.ViewModels
{
    public class TreeItemSession : DBStoredEntity
    {
        public string Id { get; private set; }

        public TreeItemSession(string id)
        {
            Id = id;
        }

        public override DateTime SessionDate => Begin;

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

        [DBStored(Entity = typeof(SessionDB), Field = "Weather")]
        public string Weather
        {
            get => GetValue<string>(nameof(Weather), null);
            set => SetValue(nameof(Weather), value);
        }

        [DBStored(Entity = typeof(SessionDB), Field = "Seeing")]
        public int? Seeing
        {
            get => GetValue<int?>(nameof(Seeing), null);
            set => SetValue(nameof(Seeing), value);
        }

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

        public Attachment[] Attachments
        {
            get => GetValue<Attachment[]>(nameof(Attachments), new Attachment[0]);
            set => SetValue(nameof(Attachments), value);
        }

        public int ObservationsCount
        {
            get => Observations.Count;
        }

        public ObservableCollection<TreeItemObservation> Observations { get; private set; } = new ObservableCollection<TreeItemObservation>();
    }
}
