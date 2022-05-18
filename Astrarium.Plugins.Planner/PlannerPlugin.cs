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
using System.Windows.Data;

namespace Astrarium.Plugins.Planner
{
    public class PlannerPlugin : AbstractPlugin
    {
        private readonly ISky sky;
        private readonly ISkyMap map;
        private readonly IPlanManagerFactory planManagerFactory;
        private readonly IRecentPlansManager recentPlansManager;
        private readonly ISettings settings;

        public PlannerPlugin(ISky sky, ISkyMap map, ISettings settings, IRecentPlansManager recentPlansManager, IPlanManagerFactory planManagerFactory)
        {
            this.sky = sky;
            this.map = map;
            this.settings = settings;
            this.map.SelectedObjectChanged += (x) => NotifyPropertyChanged(nameof(HasSelectedObject));
            this.planManagerFactory = planManagerFactory;
            this.recentPlansManager = recentPlansManager;

            DefineSetting("PlannerDefaultSettings", new PlanningFilter(), isPermanent: true);

            /* Main app menu */

            MenuItem plannerMenu = new MenuItem("Planner");
            MenuItem recentPlansMenu = new MenuItem("Recent plans");
            recentPlansMenu.AddBinding(new SimpleBinding(this, nameof(RecentPlansMenuItems), nameof(MenuItem.SubItems)));
            recentPlansMenu.AddBinding(new SimpleBinding(this, nameof(HasRecentPlans), nameof(MenuItem.IsEnabled)));
            plannerMenu.SubItems = new ObservableCollection<MenuItem>(new[] {
                new MenuItem("Create new plan...", new Command<PlanImportData>(CreateNewPlan), null),
                new MenuItem("Open plan...", new Command(OpenPlan)),
                recentPlansMenu,
                null,
                new MenuItem("Defaults...", new Command(ShowPlannerDefaults))
            });
            MenuItems.Add(MenuItemPosition.MainMenuTop, plannerMenu);

            /* Context menu */

            MenuItem contextMenu = new MenuItem("Add to observation plan");
            contextMenu.AddBinding(new SimpleBinding(this, nameof(HasSelectedObject), nameof(MenuItem.IsEnabled)));
            contextMenu.AddBinding(new SimpleBinding(this, nameof(ActivePlansMenuItems), nameof(MenuItem.SubItems)));
            MenuItems.Add(MenuItemPosition.ContextMenu, contextMenu);
        }

        public override void Initialize()
        {
            recentPlansManager.LoadRecentPlansList();
            recentPlansManager.RecentPlansListChanged += HandleRecentPlansListChanged;
            HandleRecentPlansListChanged();
        }

        private void HandleRecentPlansListChanged()
        {
            NotifyPropertyChanged(nameof(RecentPlansMenuItems), nameof(HasRecentPlans));
        }

        /// <summary>
        /// Creates new plan, from filter or from collection of items.
        /// </summary>
        private void CreateNewPlan(PlanImportData data)
        {
            var planFilterVM = ViewManager.CreateViewModel<PlanningFilterVM>();
            var defaults = settings.Get("PlannerDefaultSettings", new PlanningFilter());

            planFilterVM.Title = "Creating new plan";
            planFilterVM.Filter = defaults;
            planFilterVM.JulianDay = sky.Context.JulianDayMidnight;
            planFilterVM.TimeFrom = TimeSpan.FromHours(22);
            planFilterVM.TimeTo = TimeSpan.FromHours(0);

            // create filter window with list of celestial objects
            if (data != null && data.Objects.Any())
            {
                planFilterVM.Title = "Importing plan";
                planFilterVM.CelestialObjects = data.Objects;
                if (data.Date != null)
                    planFilterVM.JulianDay = new Date(data.Date.Value).ToJulianEphemerisDay();
                if (data.Begin != null)
                    planFilterVM.TimeFrom = data.Begin.Value;
                if (data.End != null)
                    planFilterVM.TimeTo = data.End.Value;
            }

            if (ViewManager.ShowDialog(planFilterVM) ?? false)
            {
                var planListVM = ViewManager.CreateViewModel<PlanningListVM>();
                ViewManager.ShowWindow(planListVM);
                planListVM.CreatePlan(planFilterVM.Filter);
                AddActivePlan(planListVM);
                planListVM.Closing += x => RemoveActivePlan(planListVM);
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

        private void ClearRecentPlansList()
        {
            if (ViewManager.ShowMessageBox("Warning", "Do you really want to clear the recent plans list?", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                recentPlansManager.ClearRecentPlansList();
            }
        }

        private void RecentPlanSelected(RecentPlan recentPlan)
        {
            if (File.Exists(recentPlan.Path))
            {
                LoadPlan(recentPlan.Path, recentPlan.Type);
            }
            else
            {
                recentPlansManager.RemoveFromRecentList(recentPlan);
                ViewManager.ShowMessageBox("$Error", "File does not exist anymore.");
            }
        }

        private void AddObjectToPlan(PlanningListVM plan)
        {
            var body = map.SelectedObject;
            if (body != null)
            {
                if (plan == null)
                {
                    CreateNewPlan(new PlanImportData() { Objects = new CelestialObject[] { body } });
                }
                else
                {
                    plan.AddObject(body);
                }
            }
        }

        private void OpenPlan()
        {
            string filePath = ViewManager.ShowOpenFileDialog("Open", planManagerFactory.FormatsString, out int selectedExtensionIndex);
            if (filePath != null)
            {
                LoadPlan(filePath, planManagerFactory.GetFormat(selectedExtensionIndex));
            }
        }

        private void ShowPlannerDefaults()
        {
            var planFilterVM = ViewManager.CreateViewModel<PlanningFilterVM>();

            var defaults = settings.Get("PlannerDefaultSettings", new PlanningFilter());

            defaults.JulianDayMidnight = sky.Context.JulianDayMidnight;
            defaults.ObserverLocation = sky.Context.GeoLocation;

            planFilterVM.Title = "Planner Default Settings";
            planFilterVM.IsDateTimeControlsVisible = false;
            planFilterVM.Filter = defaults;
            if (!defaults.CelestialObjectsTypes.Any())
            {
                planFilterVM.Nodes.First().IsChecked = true;
            }

            if (ViewManager.ShowDialog(planFilterVM) ?? false)
            {
                settings.SetAndSave("PlannerDefaultSettings", planFilterVM.Filter);
            }
        }

        private async void LoadPlan(string filePath, PlanType fileFormat)
        {
            IPlanManager reader = planManagerFactory.Create(fileFormat);

            PlanImportData plan = null;
            var tokenSource = new CancellationTokenSource();
            var progress = new Progress<double>();
            ViewManager.ShowProgress("Please wait", "Reading data...", tokenSource, progress);

            try
            {
                plan = await Task.Run(() => reader.Read(filePath, tokenSource.Token, progress));
            }
            catch (Exception ex)
            {
                tokenSource.Cancel();
                recentPlansManager.RemoveFromRecentList(new RecentPlan(filePath, fileFormat));
                Log.Error($"Unable to import observation plan: {ex}");
                ViewManager.ShowMessageBox("$Error", $"Unable to import observation plan.\r\nError: {ex.Message}");
            }

            if (!tokenSource.IsCancellationRequested)
            {
                tokenSource.Cancel();
                if (plan?.Objects.Any() == true)
                {
                    CreateNewPlan(plan);
                    recentPlansManager.AddToRecentList(new RecentPlan(filePath, fileFormat));
                }
                else if (ViewManager.ShowMessageBox("$Warning", $"There are no celestial objects found in the observation plan file.\r\nDo you want to create a new plan?", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes) 
                {
                    CreateNewPlan(null);
                }
            }
        }

        public bool HasSelectedObject => map.SelectedObject != null;

        public bool HasRecentPlans => RecentPlansMenuItems.Count > 0;

        private ObservableCollection<MenuItem> recentPlansMenuItems = new ObservableCollection<MenuItem>();

        public ObservableCollection<MenuItem> RecentPlansMenuItems
        {
            get
            {
                recentPlansMenuItems.Clear();
                var recentPlanFiles = recentPlansManager.RecentList.Where(x => File.Exists(x.Path)).ToArray();
                foreach (var f in recentPlanFiles)
                {
                    recentPlansMenuItems.Add(new MenuItem(Path.GetFileName(f.Path), new Command<RecentPlan>(RecentPlanSelected), f) { Tooltip = $"Path: {f.Path}\r\nType: {f.Type}" });
                }

                if (recentPlanFiles.Any())
                {
                    recentPlansMenuItems.Add(null);
                    recentPlansMenuItems.Add(new MenuItem("Clear list", new Command(ClearRecentPlansList)));
                }

                return recentPlansMenuItems;
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
                    menuItems.Add(null);
                    menuItems.AddRange(activePlans.Select(plan => new MenuItem(plan.IsSaved ? Path.GetFileName(plan.FilePath) : Formatters.Date.Format(plan.Date), new Command<PlanningListVM>(AddObjectToPlan), plan) { Tooltip = plan.IsSaved ? plan.FilePath : "Not saved yet" }));
                }

                return new ObservableCollection<MenuItem>(menuItems);
            }
        }
    }
}
