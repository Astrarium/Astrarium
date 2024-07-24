using Astrarium.Plugins.Journal.Database;
using Astrarium.Plugins.Journal.Types;
using Astrarium.Plugins.Journal.ViewModels;
using Astrarium.Types;
using System.Data.Entity;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace Astrarium.Plugins.Journal
{
    public class JournalPlugin : AbstractPlugin
    {
        /// <summary>
        /// Path to data directory used by this plugin
        /// </summary>
        public static string PluginDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "Observations");

        /// <summary>
        /// Path to database file, located in data directory
        /// </summary>
        public static string DatabaseFilePath => Path.Combine(PluginDataPath, "Observations.db");

        /// <summary>
        /// Path to folder "images" inside data directory
        /// </summary>
        public static string ImagesDirectoryPath => Path.Combine(PluginDataPath, "images");

        private readonly IOALImporter importer;
        private readonly IOALExporter exporter;

        public JournalPlugin(IOALImporter importer, IOALExporter exporter)
        {
            this.importer = importer;
            this.exporter = exporter;

            var menuItemJournal = new MenuItem("Logbook");

            menuItemJournal.SubItems.Add(new MenuItem("Show Logbook", new Command(ShowJournal)) { HotKey = new KeyGesture(Key.J, ModifierKeys.Control, "Ctrl+J") });
            menuItemJournal.SubItems.Add(null);
            menuItemJournal.SubItems.Add(new MenuItem("Import from OAL file...", new Command(DoImport)));
            menuItemJournal.SubItems.Add(new MenuItem("Export to OAL file...", new Command(DoExport)));

            MenuItems.Add(MenuItemPosition.MainMenuTop, menuItemJournal);

            // TODO: use this as a reference, implement this

            /* Object info window extensions */
            /*
            ExtendObjectInfo("Observations", (CelestialObject body) =>
            {
                var panel = new System.Windows.Controls.StackPanel() { Orientation = System.Windows.Controls.Orientation.Vertical };

                Task.Run(() =>
                {
                    using (var db = new DatabaseContext())
                    {
                        var observations = db.Observations.Include(x => x.Target).Where(x =>
                            x.Target.CommonName == body.CommonName &&
                            x.Target.Type == body.Type
                        ).ToArray();

                        if (observations.Any())
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                panel.Children.Add(new System.Windows.Controls.TextBlock() { Text = "Count of observations of " + body.Names.First() });
                                panel.Children.Add(new System.Windows.Controls.TextBlock() { Text = observations.Length.ToString() });
                            });
                        }

                    }
                });

                return panel;
            });
            */

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
            ViewManager.ShowWindow<JournalVM>(ViewFlags.SingleInstance);
        }

        private async void DoImport()
        {
            string file = ViewManager.ShowOpenFileDialog("Import from OAL file", "Open Astronomy Log files (*.xml)|*.xml|Zipped Open Astronomy Log archives (*.zip)|*.zip|All files|*.*", multiSelect: false, out int filterIndex)?.FirstOrDefault();
           
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
            string file = ViewManager.ShowSaveFileDialog("Export to OAL file", "Observations", ".xml", "Open Astronomy Log files (*.xml)|*.xml|Zipped Open Astronomy Log archives (*.zip)|*.zip|All files|*.*", out int index);
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
