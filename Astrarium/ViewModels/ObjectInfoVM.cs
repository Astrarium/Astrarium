using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public double JulianDay { get; private set; }

        public ICommand CopyNameCommand { get; private set; }
        public ICommand LinkClickedCommand { get; private set; }
        public ICommand UriClickedCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }

        public IList<ObjectInfoTabViewModel> Tabs { get; private set; } = new List<ObjectInfoTabViewModel>();

        public ObjectInfoVM(CelestialObjectInfo info)
        {
            Title = info.Title;
            Subtitle = info.Subtitle;

            // add general info tab by default
            Tabs.Add(new ObjectInfoTabViewModel("GENERAL", new ObjectInfoView() { DataContext = info.InfoElements }) { IsHeaderVisible = false });

            CopyNameCommand = new Command(CopyName);
            LinkClickedCommand = new Command<double>(SelectJulianDay);
            UriClickedCommand = new Command<Uri>(NavigateToUri);
            CloseCommand = new Command(Close);
        }

        public void AddExtension(string header, FrameworkElement extension)
        {
            Tabs.Add(new ObjectInfoTabViewModel(header, extension));
            Tabs[0].IsHeaderVisible = true;
        }

        private void SelectJulianDay(double jd)
        {
            JulianDay = jd;
            Close(true);
        }

        private void NavigateToUri(Uri uri)
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

            public ObjectInfoTabViewModel(string header, FrameworkElement content)
            {
                Header = header;
                Content = content;
                IsHeaderVisible = true;
            }
        }
    }
}
