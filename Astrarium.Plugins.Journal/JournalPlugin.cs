using Astrarium.Plugins.Journal.Database;
using Astrarium.Plugins.Journal.OAL;
using Astrarium.Plugins.Journal.ViewModels;
using Astrarium.Types;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal
{
    public class JournalPlugin : AbstractPlugin
    {
        public JournalPlugin()
        {
            var menuItemJournal = new MenuItem("Logbook");

            menuItemJournal.SubItems.Add(new MenuItem("Show Logbook", new Command(ShowJournal)));
            menuItemJournal.SubItems.Add(new MenuItem("Import", new Command(DoImport)));
            menuItemJournal.SubItems.Add(new MenuItem("Export...", new Command(DoExport)));

            MenuItems.Add(MenuItemPosition.MainMenuTop, menuItemJournal);

            // this will avoid first slow call
            Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    bool sessionsExists = db.Sessions.Any();
                }
            });
        }

        private void ShowJournal()
        {
            ViewManager.ShowWindow<JournalVM>(isSingleInstance: true);
        }

        private void DoImport()
        {
            string file = ViewManager.ShowOpenFileDialog("Import from OAL file", "Open Astronomy Log files (*.xml)|*.xml|All files|*.*", multiSelect: false, out int filterIndex)?.FirstOrDefault();
           
            if (file != null)
            {
                //System.Data.Entity.Database.Delete("db");
                

                Import.ImportFromOAL(file);
            }
        }

        private void DoExport()
        {
            string file = ViewManager.ShowSaveFileDialog("Export to OAL file", "Observations", ".xml", "Open Astronomy Log files (*.xml)|*.xml", out int index);
            if (file != null)
            {
                Export.ExportToOAL(file);
            }
        }
    }
}
