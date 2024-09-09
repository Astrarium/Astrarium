﻿using Astrarium.Types.Themes;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Astrarium.ViewModels
{
    public class EphemerisSettingsVM : ViewModelBase
    {
        private readonly ISky sky;

        public ObservableCollection<Node> Nodes { get; private set; } = new ObservableCollection<Node>();
        public Command OkCommand { get; private set; }
        public Command CancelCommand { get; private set; }

        public double JulianDayFrom { get; set; }
        public double JulianDayTo { get; set; }
        public TimeSpan Step { get; set; } = TimeSpan.FromDays(1);
        public double UtcOffset { get; private set; }

        // TODO: check object can provide ephemerides (IEphemeridable ?)
        public Func<CelestialObject, bool> Filter => filter;

        private bool filter(CelestialObject obj)
        {
            return sky.GetEphemerisCategories(obj).Any();
        }

        private CelestialObject _SelectedBody = null;
        public CelestialObject SelectedBody
        {
            get
            {
                return _SelectedBody;
            }
            set
            {
                _SelectedBody = value;
                BuildCategoriesTree();
                NotifyPropertyChanged(nameof(SelectedBody));
                NotifyPropertyChanged(nameof(OkButtonEnabled));
            }
        }

        public IEnumerable<string> Categories => Nodes.First().CheckedChildIds;

        public bool OkButtonEnabled
        {
            get
            {
                return Nodes.Any() && Nodes.First().IsChecked != false;
            }
        }

        public EphemerisSettingsVM(ISky sky)
        {
            this.sky = sky;

            UtcOffset = sky.Context.GeoLocation.UtcOffset;

            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
        }

        public void Ok()
        {
            if (JulianDayFrom > JulianDayTo)
            {
                ViewManager.ShowMessageBox("$EphemeridesSettingsWindow.WarningTitle", "$EphemeridesSettingsWindow.DateWarningText", System.Windows.MessageBoxButton.OK);
                return;
            }

            if (Step < TimeSpan.FromSeconds(1))
            {
                ViewManager.ShowMessageBox("$EphemeridesSettingsWindow.WarningTitle", "$EphemeridesSettingsWindow.StepWarningText", System.Windows.MessageBoxButton.OK);
                return;
            }

            if ((JulianDayTo - JulianDayFrom) / Step.TotalDays > 10000)
            {
                ViewManager.ShowMessageBox("$EphemeridesSettingsWindow.WarningTitle", "$EphemeridesSettingsWindow.LargeTableWarningText", System.Windows.MessageBoxButton.OK);
                return;
            }

            // everything is fine
            Close(true);
        }

        private void BuildCategoriesTree()
        {
            Nodes.Clear();

            if (SelectedBody != null)
            {
                var categories = sky.GetEphemerisCategories(SelectedBody);

                var groups = categories.GroupBy(cat => cat.Split('.').First());

                Node root = new Node(Text.Get("EphemeridesSettingsWindow.Ephemerides.All"));
                root.CheckedChanged += Root_CheckedChanged;

                string selectedBodyTypeName = SelectedBody.GetType().Name;

                foreach (var group in groups)
                {
                    Node node = new Node(Text.Get($"{selectedBodyTypeName}.{group.Key}"), group.Key);

                    if (group.Count() > 1)
                    {
                        foreach (var item in group)
                        {
                            node.Children.Add(new Node(Text.Get($"{selectedBodyTypeName}.{item}"), item));
                        }
                    }

                    root.Children.Add(node);
                }

                Nodes.Add(root);
                root.IsChecked = true;
            }
        }

        private void Root_CheckedChanged(object sender, bool? e)
        {
            NotifyPropertyChanged(nameof(OkButtonEnabled));
        }
    }
}
