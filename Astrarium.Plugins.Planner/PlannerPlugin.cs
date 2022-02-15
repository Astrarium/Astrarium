using Astrarium.Algorithms;
using Astrarium.Plugins.Planner.ImportExport;
using Astrarium.Plugins.Planner.ViewModels;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Planner
{
    public class PlannerPlugin : AbstractPlugin
    {
        private readonly ISky sky;
        private readonly ISkyMap map;
        private readonly PlanFactory readWriterFactory;

        public PlannerPlugin(ISky sky, ISkyMap map, PlanFactory readWriterFactory)
        {
            this.sky = sky;
            this.map = map;
            this.map.SelectedObjectChanged += (x) => NotifyPropertyChanged(nameof(HasSelectedObject));
            this.readWriterFactory = readWriterFactory;

            /* Main app menu */

            MenuItem plannerMenu = new MenuItem("Planner");
            MenuItem recentPlansMenu = new MenuItem("Recent plans");
            recentPlansMenu.AddBinding(new SimpleBinding(this, nameof(RecentPlansMenuItems), nameof(MenuItem.SubItems)));
            recentPlansMenu.AddBinding(new SimpleBinding(this, nameof(HasRecentPlans), nameof(MenuItem.IsEnabled)));
            plannerMenu.SubItems = new ObservableCollection<MenuItem>(new[] {
                new MenuItem("Create new plan...", new Command<ICollection<CelestialObject>>(CreateNewPlan), null),
                new MenuItem("Open plan...", new Command(OpenPlan)),
                recentPlansMenu,
                null,
                new MenuItem("Defaults...")
            });
            MenuItems.Add(MenuItemPosition.MainMenuTop, plannerMenu);

            /* Context menu */

            MenuItem contextMenu = new MenuItem("Add to observation plan");
            contextMenu.AddBinding(new SimpleBinding(this, nameof(HasSelectedObject), nameof(MenuItem.IsEnabled)));
            contextMenu.AddBinding(new SimpleBinding(this, nameof(ActivePlansMenuItems), nameof(MenuItem.SubItems)));
            MenuItems.Add(MenuItemPosition.ContextMenu, contextMenu);
        }

        /// <summary>
        /// Creates new plan, from filter or from collection of items.
        /// </summary>
        /// <param name="body"></param>
        private void CreateNewPlan(ICollection<CelestialObject> celestialObjects)
        {
            var vm = ViewManager.CreateViewModel<PlanningFilterVM>();

            // create filter window with treeview of object types
            if (celestialObjects == null || !celestialObjects.Any())
            {
                vm.CelestialObjectsTypes = sky.CelestialObjects.Select(c => c.Type).Where(t => t != null).Distinct().ToArray();
            }
            // create filter window with list of celestial objects
            else
            {
                vm.CelestialObjects = celestialObjects;
            }

            if (ViewManager.ShowDialog(vm) ?? false)
            {
                var plan = ViewManager.CreateViewModel<PlanningListVM>();
                ViewManager.ShowWindow(plan);
                plan.CreatePlan(vm.Filter);
                AddActivePlan(plan);
                plan.Closing += x => RemoveActivePlan(plan);
            }
        }

        private List<PlanningListVM> activePlans = new List<PlanningListVM>();

        private void AddActivePlan(PlanningListVM plan)
        {
            activePlans.Insert(0, plan);
            NotifyPropertyChanged(nameof(ActivePlansMenuItems));
        }

        private void RemoveActivePlan(PlanningListVM plan)
        {
            activePlans.Remove(plan);
            NotifyPropertyChanged(nameof(ActivePlansMenuItems));
        }

        private void RecentPlanSelected(string path)
        {
            // TODO: load and open plan
        }

        private void AddObjectToPlan(PlanningListVM plan)
        {
            var body = map.SelectedObject;
            if (body != null)
            {
                if (plan == null)
                {
                    CreateNewPlan(new CelestialObject[] { body });
                }
                else
                {
                    plan.AddObject(body);
                }
            }
        }

        private async void OpenPlan()
        {
            string filePath = ViewManager.ShowOpenFileDialog("Open", readWriterFactory.FormatsString, out int selectedExtensionIndex);
            if (filePath != null)
            {
                var fileFormat = readWriterFactory.GetFormat(selectedExtensionIndex);
                IPlan reader = readWriterFactory.Create(fileFormat);

                ICollection<CelestialObject> celestialObjects = new CelestialObject[0];
                var tokenSource = new CancellationTokenSource();
                var progress = new Progress<double>();
                ViewManager.ShowProgress("Please wait", "Reading data...", tokenSource, progress);

                try
                {
                    celestialObjects = await Task.Run(() => reader.Read(filePath, tokenSource.Token, progress));
                }
                catch (Exception ex)
                {
                    tokenSource.Cancel();
                    // TODO: log
                    ViewManager.ShowMessageBox("Error", $"Unable to import observation plan.\r\nError: {ex.Message}");
                }

                if (!tokenSource.IsCancellationRequested)
                {
                    tokenSource.Cancel();
                    if (celestialObjects.Any())
                    {
                        CreateNewPlan(celestialObjects);
                    }
                    else
                    {
                        // TODO: inform user
                    }
                }
                
            }
        }

        public bool HasSelectedObject => map.SelectedObject != null;

        public bool HasRecentPlans => RecentPlansMenuItems.Count > 0;

        public ObservableCollection<MenuItem> RecentPlansMenuItems
        {
            get
            {
                // TODO: take from settings, filter only existing files, order by file change name, take 10 latest
                string[] recentPlanFiles = new[]
                {
                    @"C:\\PathToTheFile\File1.plan",
                    @"C:\\Second\PathToTheFile\File 2.plan"
                };

                return new ObservableCollection<MenuItem>(recentPlanFiles.Select(f => new MenuItem(Path.GetFileNameWithoutExtension(f), new Command<string>(RecentPlanSelected))));
            }
        }

        /// <summary>
        /// Gets list of active (opened) plans
        /// </summary>
        public ObservableCollection<MenuItem> ActivePlansMenuItems
        {
            get
            {
                List<MenuItem> menuItems = new List<MenuItem>();
                menuItems.Add(new MenuItem("Create new plan...", new Command<PlanningListVM>(AddObjectToPlan), null));

                if (activePlans.Any())
                {
                    menuItems.Add(null); // separator
                    menuItems.AddRange(activePlans.Select(plan => new MenuItem(plan.Name, new Command<PlanningListVM>(AddObjectToPlan), plan)));
                }

                return new ObservableCollection<MenuItem>(menuItems);
            }
        }
    }
}
