using Astrarium.Algorithms;
using Astrarium.Plugins.Planner.ImportExport;
using Astrarium.Plugins.Planner.ViewModels;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            this.map.SelectedObjectChanged += (x) => NotifyPropertyChanged(nameof(IsPlannerConstextMenuEnabled));
            this.planManagerFactory = planManagerFactory;
            this.recentPlansManager = recentPlansManager;

            DefineSetting("PlannerDefaultSettings", new PlanningFilter(), isPermanent: true);

            /* Main app menu */

            MenuItem plannerMenu = new MenuItem("$Planner.Menu.Planner");
            MenuItem recentPlansMenu = new MenuItem("$Planner.Menu.RecentPlans");
            recentPlansMenu.AddBinding(new SimpleBinding(this, nameof(RecentPlansMenuItems), nameof(MenuItem.SubItems)));
            recentPlansMenu.AddBinding(new SimpleBinding(this, nameof(HasRecentPlans), nameof(MenuItem.IsEnabled)));
            plannerMenu.SubItems = new ObservableCollection<MenuItem>(new[] {
                new MenuItem("$Planner.Menu.CreateNewPlan", new Command<PlanImportData>(CreateNewPlan), null),
                new MenuItem("$Planner.Menu.OpenPlan", new Command(OpenPlan)),
                recentPlansMenu,
                null,
                new MenuItem("$Planner.Menu.Defaults", new Command(ShowPlannerDefaults))
            });
            MenuItems.Add(MenuItemPosition.MainMenuTop, plannerMenu);

            /* Context menu */

            MenuItem contextMenu = new MenuItem("$Planner.ContextMenu.AddToObservationPlan");
            contextMenu.AddBinding(new SimpleBinding(this, nameof(IsPlannerConstextMenuEnabled), nameof(MenuItem.IsEnabled)));
            contextMenu.AddBinding(new SimpleBinding(this, nameof(ActivePlansMenuItems), nameof(MenuItem.SubItems)));
            MenuItems.Add(MenuItemPosition.ContextMenu, contextMenu);

            /* Object info window extensions */
            ExtendObjectInfo((CelestialObject body) =>
            {
                if (body.Type == "Sun")
                {
                    var panel = new System.Windows.Controls.StackPanel() { Orientation = System.Windows.Controls.Orientation.Vertical };
                    panel.Children.Add(new System.Windows.Controls.TextBlock() { Text = "Hello" });
                    panel.Children.Add(new System.Windows.Controls.TextBlock() { Text = "from plugin" });
                    return panel;
                }
                else
                {
                    return null;
                }
            });
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

            planFilterVM.Title = Text.Get("Planner.PlanningFilter.CreateNew.Title");
            planFilterVM.Filter = defaults;
            planFilterVM.JulianDay = sky.Context.JulianDayMidnight;
            planFilterVM.TimeFrom = TimeSpan.FromHours(22);
            planFilterVM.TimeTo = TimeSpan.FromHours(0);

            // create filter window with list of celestial objects
            if (data != null && data.Objects.Any())
            {
                planFilterVM.CelestialObjects = data.Objects;
                if (data.Date != null)
                    planFilterVM.JulianDay = new Date(data.Date.Value).ToJulianEphemerisDay();
                if (data.Begin != null)
                    planFilterVM.TimeFrom = data.Begin.Value;
                if (data.End != null)
                    planFilterVM.TimeTo = data.End.Value;
                if (data.FilePath != null)
                    planFilterVM.Title = Text.Get("Planner.PlanningFilter.Import.Title");
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
            if (ViewManager.ShowMessageBox("$Warning", "$Planner.Menu.RecentPlans.Clear.WarningText", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
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
                ViewManager.ShowMessageBox("$Error", "$Planner.Menu.RecentPlans.FileDoesNotExist");
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
            string filePath = ViewManager.ShowOpenFileDialog("$Planner.Menu.OpenPlan.DialogTitle", planManagerFactory.FormatsString, multiSelect: false, out int selectedExtensionIndex)?.FirstOrDefault();
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

            planFilterVM.Title = Text.Get("Planner.PlanningFilter.Defaults");
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
            ViewManager.ShowProgress("$Planner.Importing.WaitTitle", "$Planner.Importing.WaitText", tokenSource, progress);

            try
            {
                plan = await Task.Run(() => reader.Read(filePath, tokenSource.Token, progress));
            }
            catch (Exception ex)
            {
                tokenSource.Cancel();
                recentPlansManager.RemoveFromRecentList(new RecentPlan(filePath, fileFormat));
                Log.Error($"Unable to import observation plan: {ex}");
                ViewManager.ShowMessageBox("$Error", $"{Text.Get("Planner.Importing.Error")}: {ex.Message}");
            }

            if (!tokenSource.IsCancellationRequested)
            {
                tokenSource.Cancel();
                if (plan?.Objects.Any() == true)
                {
                    CreateNewPlan(plan);
                    recentPlansManager.AddToRecentList(new RecentPlan(filePath, fileFormat));
                }
                else if (ViewManager.ShowMessageBox("$Warning", Text.Get("Planner.Importing.NoCelestialObjectImported"), System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes) 
                {
                    CreateNewPlan(null);
                }
            }
        }

        public bool IsPlannerConstextMenuEnabled => map.SelectedObject != null && map.SelectedObject is IObservableObject;

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
                    recentPlansMenuItems.Add(new MenuItem(Path.GetFileName(f.Path), new Command<RecentPlan>(RecentPlanSelected), f) { Tooltip = $"{Text.Get("Planner.Menu.RecentPlans.ItemTooltip.Path")}: {f.Path}\r\n{Text.Get("Planner.Menu.RecentPlans.ItemTooltip.Type")}: {f.Type}" });
                }

                if (recentPlanFiles.Any())
                {
                    recentPlansMenuItems.Add(null);
                    recentPlansMenuItems.Add(new MenuItem("$Planner.Menu.RecentPlans.Clear", new Command(ClearRecentPlansList)));
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
                menuItems.Add(new MenuItem("$Planner.ContextMenu.CreateNewPlan", new Command<PlanningListVM>(AddObjectToPlan), null));

                if (activePlans.Any())
                {
                    menuItems.Add(null);
                    menuItems.AddRange(activePlans.Select(plan => new MenuItem(plan.IsSaved ? Path.GetFileName(plan.FilePath) : Formatters.Date.Format(plan.Date), new Command<PlanningListVM>(AddObjectToPlan), plan) { Tooltip = plan.IsSaved ? plan.FilePath : Text.Get("Planner.ContextMenu.PlanItem.NotSavedTooltip") }));
                }

                return new ObservableCollection<MenuItem>(menuItems);
            }
        }
    }
}
