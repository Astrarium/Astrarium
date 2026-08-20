using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Astrarium.ViewModels
{
    internal class ObjectInfoVM : ViewModelBase
    {
        public double JulianDay { get; private set; }

        public ICommand CopyNameCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand LinkClickedCommand { get; private set; }
        public ICommand PropertyValueClickedCommand { get; private set; }
        public ICommand PropertyCommandClickedCommand { get; private set; }
        public ICommand JulianDateClickedCommand { get; private set; }

        public CelestialObjectInfo Info { get; private set; }

        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public CelestialObject Body { get; private set; }

        public IList<ObjectInfoTabViewModel> Tabs { get; private set; } = new List<ObjectInfoTabViewModel>();

        public bool IsTabHeaderVisible { get; private set; }

        private ISky sky;

        public ObjectInfoVM(ISky sky)
        {
            this.sky = sky;

            CopyNameCommand = new Command(CopyName);
            CloseCommand = new Command(Close);
            LinkClickedCommand = new Command<Uri>(LinkClicked);
            PropertyValueClickedCommand = new Command<object>(PropertyValueClicked);
            JulianDateClickedCommand = new Command<Date>(JulianDateClicked);
            PropertyCommandClickedCommand = new Command<Action>(PropertyCommandClicked);
        }

        public ObjectInfoVM WithObjectInfo(CelestialObjectInfo info, List<ObjectInfoExtension> extensions)
        {
            Info = info;

            Tabs.Add(new ObjectInfoGeneralVM(Text.Get("ObjectInfoWindow.Tab.Info")));

            foreach (var ext in extensions)
            {
                var vm = ext.ViewModelProvider.DynamicInvoke(sky.Context, Info.GetBody());
                if (vm != null)
                {
                    var control = Activator.CreateInstance(ext.ViewType) as FrameworkElement;
                    control.SetValue(FrameworkElement.DataContextProperty, vm);
                    Tabs.Add(new ObjectInfoExtensionVM(ext.Title, control));
                }
            }

            IsTabHeaderVisible = Tabs.Count > 1;

            return this;
        }

        private void LinkClicked(Uri uri)
        {
            try
            {
                Process.Start(new ProcessStartInfo(uri.ToString()));
            }
            catch (Exception ex)
            {
                Log.Error("Unable to open browser: " + ex);
            }
        }

        private void PropertyValueClicked(object value)
        {
            try
            {
                Clipboard.SetText(value.ToString());
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to copy property value. Reason: {ex.Message}");
            }
        }

        private void JulianDateClicked(Date date)
        {
            JulianDay = date.ToJulianEphemerisDay();
            Close(true);
        }

        private void PropertyCommandClicked(Action action)
        {
            action?.Invoke();
        }

        private void CopyName()
        {
            try
            {
                Clipboard.SetText(Info.Title);
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to copy object name. Reason: {ex.Message}");
            }
        }

        public override object Payload => new { Body = $"{Info.ObjectType}/{Info.ObjectCommonName}" };
    }

    public abstract class ObjectInfoTabViewModel
    {
        public string Header { get; protected set; }
        public ObjectInfoTabViewModel(string header)
        {
            Header = header;
        }
    }

    public class ObjectInfoGeneralVM : ObjectInfoTabViewModel
    {
        public ObjectInfoGeneralVM(string header) : base(header) { }
    }

    public class ObjectInfoExtensionVM : ObjectInfoTabViewModel
    {
        public FrameworkElement Control { get; private set; }

        public ObjectInfoExtensionVM(string header, FrameworkElement control) : base(header)
        {
            Control = control;
        }
    }
}