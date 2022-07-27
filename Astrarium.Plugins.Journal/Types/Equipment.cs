using Astrarium.Plugins.Journal.Database;
using Astrarium.Plugins.Journal.Database.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public static class Equipment
    {
        public static ObservableCollection<OpticsDB> Telescopes => telescopes.Value;

        public static ObservableCollection<EyepieceDB> Eyepieces => eyepieces.Value;

        public static ObservableCollection<LensDB> Lenses => lenses.Value;

        public static ObservableCollection<FilterDB> Filters => filters.Value;

        public static ObservableCollection<ImagerDB> Cameras => cameras.Value;

        public static ObservableCollection<SiteDB> Sites => sites.Value;

        private static Lazy<ObservableCollection<OpticsDB>> telescopes = new Lazy<ObservableCollection<OpticsDB>>(() =>
        {
            using (var db = new DatabaseContext())
            {
                return new ObservableCollection<OpticsDB>(db.Optics.ToArray());
            }
        });

        private static Lazy<ObservableCollection<EyepieceDB>> eyepieces = new Lazy<ObservableCollection<EyepieceDB>>(() =>
        {
            using (var db = new DatabaseContext())
            {
                return new ObservableCollection<EyepieceDB>(db.Eyepieces.ToArray());
            }
        });

        private static Lazy<ObservableCollection<LensDB>> lenses = new Lazy<ObservableCollection<LensDB>>(() =>
        {
            using (var db = new DatabaseContext())
            {
                return new ObservableCollection<LensDB>(db.Lenses.ToArray());
            }
        });

        private static Lazy<ObservableCollection<FilterDB>> filters = new Lazy<ObservableCollection<FilterDB>>(() =>
        {
            using (var db = new DatabaseContext())
            {
                return new ObservableCollection<FilterDB>(db.Filters.ToArray());
            }
        });

        private static Lazy<ObservableCollection<ImagerDB>> cameras = new Lazy<ObservableCollection<ImagerDB>>(() =>
        {
            using (var db = new DatabaseContext())
            {
                return new ObservableCollection<ImagerDB>(db.Imagers.ToArray());
            }
        });

        private static Lazy<ObservableCollection<SiteDB>> sites = new Lazy<ObservableCollection<SiteDB>>(() =>
        {
            using (var db = new DatabaseContext())
            {
                return new ObservableCollection<SiteDB>(db.Sites.ToArray());
            }
        });
    }
}
