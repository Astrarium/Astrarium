using Planetarium.Objects;
using Planetarium.Types;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Planetarium.Controls
{
    public class CelestialObjectPicker : Control
    {
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
                var picker = o as CelestialObjectPicker;
                o.SetValue(SelectedBodyNameProperty, ((CelestialObject)e.NewValue).Names.First());
            })
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

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
            CelestialObject body = ViewManager.ShowSearchDialog(Filter);
            if (body != null)
            {
                SelectedBody = body;
            }
        }
    }
}
