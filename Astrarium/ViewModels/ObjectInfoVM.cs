using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows;
using System.Diagnostics;
using Astrarium.Views;

namespace Astrarium.ViewModels
{
    internal class ObjectInfoVM : ViewModelBase
    {
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string ObjectType { get; private set; }
        public string ObjectCommonName { get; private set; }
        public double JulianDay { get; private set; }

        public ICommand CopyNameCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }

        public IList<ObjectInfoTabViewModel> Tabs { get; private set; } = new List<ObjectInfoTabViewModel>();

        public ObjectInfoVM(CelestialObjectInfo info)
        {
            Title = info.Title;
            Subtitle = info.Subtitle;
            ObjectType = info.ObjectType;
            ObjectCommonName = info.ObjectCommonName;

            CopyNameCommand = new Command(CopyName);
            CloseCommand = new Command(Close);

            var objectInfoView = new ObjectInfoView() { DataContext = info.InfoElements };
            objectInfoView.LinkClicked += LinkClicked;
            objectInfoView.JulianDateClicked += JulianDateClicked;
            objectInfoView.PropertyValueClicked += PropertyValueClicked;

            // add general info tab by default
            Tabs.Add(new ObjectInfoTabViewModel(Text.Get("ObjectInfoWindow.Tab.Info"), objectInfoView, isHeaderVisible: false));
        }

        public void AddExtension(string header, FrameworkElement extension)
        {
            if (header.StartsWith("$")) header = Text.Get(header.Substring(1));
            Tabs.Add(new ObjectInfoTabViewModel(header, extension));
            Tabs[0].IsHeaderVisible = true;
        }

        private void JulianDateClicked(double jd)
        {
            JulianDay = jd;
            Close(true);
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
                Clipboard.SetText(value.ToString(), TextDataFormat.UnicodeText);
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to copy property value. Reason: {ex.Message}");
            }
        }

        private void CopyName()
        {
            try
            {
                Clipboard.SetText(Title, TextDataFormat.UnicodeText);
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to copy object name. Reason: {ex.Message}");
            }
        }

        internal class ObjectInfoTabViewModel
        {
            public bool IsHeaderVisible { get; set; }
            public string Header { get; protected set; }
            public FrameworkElement Content { get; protected set; }

            public ObjectInfoTabViewModel(string header, FrameworkElement content, bool isHeaderVisible = true)
            {
                Header = header;
                Content = content;
                IsHeaderVisible = isHeaderVisible;
            }
        }

        public override object Payload => new { Body = $"{ObjectCommonName}/{ObjectType}" };
    }
}