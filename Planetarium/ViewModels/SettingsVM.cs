using ADK.Demo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Planetarium.ViewModels
{
    public class SettingsVM : ViewModelBase
    {
        public ICommand CloseCommand { get; private set; }
        public ObservableCollection<SettingsSection> SettingsSections { get; private set; }

        private SettingsSection selectedSection;
        public SettingsSection SelectedSection
        {
            get
            {
                return selectedSection;
            }
            set
            {
                selectedSection = value;
                BuildSectionContent();
            }
        }
        public UIElement SectionContent { get; private set; } 

        public SettingsVM(ISettings settings)
        {
            CloseCommand = new Command(Close);

            SettingsSections = new ObservableCollection<SettingsSection>();
            SettingsSections.Add(new SettingsSection() { Title = "Section 1" });
            SettingsSections.Add(new SettingsSection() { Title = "Section 2" });
            SettingsSections.Add(new SettingsSection() { Title = "Section 3" });

            SelectedSection = SettingsSections.First();
        }

        private void BuildSectionContent()
        {
            SectionContent = new TextBlock() { Text = SelectedSection.Title };
            NotifyPropertyChanged(nameof(SectionContent));
        }

        public class SettingsSection
        {
            public string Title { get; set; }
        }
    }
}
