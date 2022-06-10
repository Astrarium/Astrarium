using Astrarium.Plugins.Journal.OAL;
using Astrarium.Plugins.Journal.ViewModels;
using Astrarium.Types;
using ObservationPlannerDatabase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal
{
    public class JournalPlugin : AbstractPlugin
    {
        public JournalPlugin()
        {
            MenuItems.Add(MenuItemPosition.MainMenuTop,
                    new MenuItem("Journal",
                    new Command(() => ViewManager.ShowWindow<JournalVM>(isSingleInstance: true))));

            using (var db = new DatabaseContext())
            {
                var obs = db.Observations.FirstOrDefault();
            }
        }
    }
}
