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
            var menuItemJournal = new MenuItem("Journal");

            menuItemJournal.SubItems.Add(new MenuItem("Show Journal", new Command(ShowJournal)));
            menuItemJournal.SubItems.Add(new MenuItem("Import", new Command(DoImport)));

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
            var vm = ViewManager.CreateViewModel<JournalVM>();
            ViewManager.ShowWindow(vm);
            vm.Load();
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
    }
}
