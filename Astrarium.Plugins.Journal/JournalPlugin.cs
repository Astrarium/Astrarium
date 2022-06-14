using Astrarium.Plugins.Journal.Database;
using Astrarium.Plugins.Journal.OAL;
using Astrarium.Plugins.Journal.ViewModels;
using Astrarium.Types;
using System.IO;
using System.Linq;

namespace Astrarium.Plugins.Journal
{
    public class JournalPlugin : AbstractPlugin
    {
        public JournalPlugin()
        {
            var menuItemJournal = new MenuItem("Journal");

            menuItemJournal.SubItems.Add(new MenuItem("Show Journal",
                    new Command(() => ViewManager.ShowWindow<JournalVM>(isSingleInstance: true))));

            menuItemJournal.SubItems.Add(new MenuItem("Import", new Command(DoImport)));

            MenuItems.Add(MenuItemPosition.MainMenuTop, menuItemJournal);
        }

        private void DoImport()
        {
            string file = ViewManager.ShowOpenFileDialog("Import from OAL file", "Open Astronomy Log files (*.xml)|*.xml|All files|*.*", out int filterIndex);
           
            if (file != null)
            {
                //System.Data.Entity.Database.Delete("db");
                

                Import.ImportFromOAL(file);
            }
        }
    }
}
