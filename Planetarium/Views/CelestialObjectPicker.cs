using ADK;
using Planetarium.Objects;
using Planetarium.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Documents;

namespace Planetarium.Views
{
    public class CelestialObjectPicker : Control
    {
        public CelestialObjectPicker() { }

        public CelestialObject SelectedBody
        {
            get { return (CelestialObject)GetValue(SelectedBodyProperty); }
            set { SetValue(SelectedBodyProperty, value); }
        }
        public readonly static DependencyProperty SelectedBodyProperty = DependencyProperty.Register(
            nameof(SelectedBody), 
            typeof(CelestialObject), 
            typeof(CelestialObjectPicker), 
            new FrameworkPropertyMetadata(null, (o, e) =>
            {
                var searcher = (ISearcher)o.GetValue(SearcherProperty);
                o.SetValue(SelectedBodyNameProperty, searcher.GetObjectName((CelestialObject)e.NewValue));
            })
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

        public ISearcher Searcher
        {
            get { return (ISearcher)GetValue(SearcherProperty); }
            set { SetValue(SearcherProperty, value); }
        }

        public readonly static DependencyProperty SearcherProperty = DependencyProperty.Register(
            nameof(Searcher), 
            typeof(ISearcher), 
            typeof(CelestialObjectPicker));

        public string SelectedBodyName
        {
            get { return (string)GetValue(SelectedBodyNameProperty); }
            private set { SetValue(SelectedBodyNameProperty, value); }
        }
        public readonly static DependencyProperty SelectedBodyNameProperty = DependencyProperty.Register(
            nameof(SelectedBodyName), 
            typeof(string), 
            typeof(CelestialObjectPicker),
            new PropertyMetadata("Not set"));

        public IViewManager ViewManager
        {
            get { return (IViewManager)GetValue(ViewManagerProperty); }
            set { SetValue(ViewManagerProperty, value); }
        }
        public readonly static DependencyProperty ViewManagerProperty = DependencyProperty.Register(
            nameof(ViewManager), 
            typeof(IViewManager), 
            typeof(CelestialObjectPicker), 
            new UIPropertyMetadata(null));

        public Func<CelestialObject, bool> Filter
        {
            get { return (Func<CelestialObject, bool>)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public readonly static DependencyProperty FilterProperty = DependencyProperty.Register(
           nameof(Filter),
           typeof(Func<CelestialObject, bool>),
           typeof(CelestialObjectPicker),
           new UIPropertyMetadata((Func<CelestialObject, bool>)(c => true)));

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            var vm = ViewManager.CreateViewModel<SearchVM>();
            vm.Filter = Filter;
            if (ViewManager.ShowDialog(vm) ?? false)
            {
                SelectedBody = vm.SelectedItem.Body;
            }
        }
    }
}
