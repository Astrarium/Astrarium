using Astrarium.Algorithms;
using Astrarium.Plugins.Planner.ViewModels;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Astrarium.Plugins.Planner
{
    public class PlannerPlugin : AbstractPlugin
    {
        private readonly ISky sky;

        public PlannerPlugin(ISky sky)
        {
            this.sky = sky;
            MenuItem plannerMenu = new MenuItem("Planner");
            plannerMenu.SubItems = new ObservableCollection<MenuItem>(new[] {
                new MenuItem("New Plan...", new Command(CreateNewPlan)),
                new MenuItem("Open recent"),
            });
            MenuItems.Add(MenuItemPosition.MainMenuTop, plannerMenu);
        }

        private void CreateNewPlan()
        {
            var vm = ViewManager.CreateViewModel<PlanningFilterVM>();
            if (ViewManager.ShowDialog(vm) ?? false)
            {
                var listViewModel = ViewManager.CreateViewModel<PlanningListVM>();
                ViewManager.ShowWindow(listViewModel);
                listViewModel.CreatePlan(vm.Filter);
                
            }
        }
      
    }
}
