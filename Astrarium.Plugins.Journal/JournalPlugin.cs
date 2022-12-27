using Astrarium.Plugins.Journal.Database;
using Astrarium.Plugins.Journal.OAL;
using Astrarium.Plugins.Journal.Types;
using Astrarium.Plugins.Journal.ViewModels;
using Astrarium.Types;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal
{
    public class JournalPlugin : AbstractPlugin
    {
        private readonly IOALImporter importer;
        private readonly IOALExporter exporter;

        public JournalPlugin(IOALImporter importer, IOALExporter exporter)
        {
            this.importer = importer;
            this.exporter = exporter;

            var menuItemJournal = new MenuItem("Logbook");

            menuItemJournal.SubItems.Add(new MenuItem("Show Logbook", new Command(ShowJournal)));
            menuItemJournal.SubItems.Add(null);
            menuItemJournal.SubItems.Add(new MenuItem("Import from OAL file...", new Command(DoImport)));
            menuItemJournal.SubItems.Add(new MenuItem("Export to OAL file...", new Command(DoExport)));

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

        private async void DoImport()
        {
            string file = ViewManager.ShowOpenFileDialog("Import from OAL file", "Open Astronomy Log files (*.xml)|*.xml|All files|*.*", multiSelect: false, out int filterIndex)?.FirstOrDefault();
           
            if (file != null)
            {
                var tokenSource = new CancellationTokenSource();
                var progress = new Progress<double>();
                ViewManager.ShowProgress("Please wait", "Importing data...", tokenSource, progress);

                try
                {
                    await Task.Run(() => importer.ImportFromOAL(file, tokenSource.Token, progress));
                }
                catch (Exception ex)
                {
                    tokenSource.Cancel();
                    Log.Error($"Unable to import OAL data: {ex}");
                    ViewManager.ShowMessageBox("$Error", $"Import error: {ex.Message}");
                }

                if (!tokenSource.IsCancellationRequested)
                {
                    tokenSource.Cancel();
                }
            }
        }

        private async void DoExport()
        {
            string file = ViewManager.ShowSaveFileDialog("Export to OAL file", "Observations", ".xml", "Open Astronomy Log files (*.xml)|*.xml", out int index);
            if (file != null)
            {
                var tokenSource = new CancellationTokenSource();
                ViewManager.ShowProgress("Please wait", "Exporing data...", tokenSource);

                try
                {
                    await Task.Run(() => exporter.ExportToOAL(file, tokenSource.Token));
                }
                catch (Exception ex)
                {
                    tokenSource.Cancel();
                    Log.Error($"Unable to export to OAL: {ex}");
                    ViewManager.ShowMessageBox("$Error", $"Export error: {ex.Message}");
                }

                if (!tokenSource.IsCancellationRequested)
                {
                    tokenSource.Cancel();
                }
            }
        }
    }
}
