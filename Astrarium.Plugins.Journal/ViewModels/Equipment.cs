using Astrarium.Plugins.Journal.Database;
using Astrarium.Plugins.Journal.Database.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.ViewModels
{
    public static class Equipment
    {
        private static ObservableCollection<OpticsDB> telescopes;
        public static ObservableCollection<OpticsDB> Telescopes
        {
            get
            {
                if (telescopes == null)
                {
                    using (var db = new DatabaseContext())
                    {
                        telescopes = new ObservableCollection<OpticsDB>(db.Optics.ToArray());
                    }
                }
                return telescopes;
            }
        }
    }
}
