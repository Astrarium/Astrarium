using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.Planner.ViewModels
{
    public class SmartFilterVM : ViewModelBase
    {
        private readonly ObservationPlanSmartFilter smartFilter = new ObservationPlanSmartFilter();

        public ICommand OkCommand { get; }

        public string FilterString
        {
            get
            {
                return string.Join(", ", FilterItems.Where(x => x.IsEnabled && !string.IsNullOrWhiteSpace(x.Value) && !string.IsNullOrWhiteSpace(x.Operator)).Select(x => $"{x.PropertyName} {x.Operator} {x.Value}"));
            }
            set
            {
                var items = smartFilter.ParseItems(value);
                foreach (var item in items)
                {
                    var filterItem = FilterItems.FirstOrDefault(x => x.PropertyName == item.Property);
                    if (filterItem != null)
                    {
                        filterItem.IsEnabled = true;
                        filterItem.Operator = item.Operator;
                        filterItem.Value = item.Value;
                    }
                }
                NotifyPropertyChanged(nameof(OkButtonEnabled));
            }
        }

        public bool OkButtonEnabled
        {
            get => !string.IsNullOrEmpty(FilterString);
        }

        public List<FilterItem> FilterItems { get; } = new List<FilterItem>();

        public SmartFilterVM()
        {
            OkCommand = new Command(() => Close(true));

            FilterItems.Add(new FilterItem("Name", "name", new string[] { "=" }));
            FilterItems.Add(new FilterItem("Type", "type", new string[] { "=" }));
            FilterItems.Add(new FilterItem("Constallation", "con", new string[] { "=" }));
            FilterItems.Add(new FilterItem("Magnitude", "mag"));
            FilterItems.Add(new FilterItem("Beginning observation time", "begin"));
            FilterItems.Add(new FilterItem("Best observation time", "best"));
            FilterItems.Add(new FilterItem("End observation time", "end"));

            FilterItems.ForEach(x => x.PropertyChanged += (s, e) => NotifyPropertyChanged(nameof(OkButtonEnabled)));
        }

        public class FilterItem : PropertyChangedBase
        {
            public FilterItem() { }
            public FilterItem(string title, string propertyName, string[] operators = null)
            {
                Title = title;
                PropertyName = propertyName;
                Operators = operators ?? new string[] { "=", ">", "<" };
            }

            public string[] Operators { get; }
            public string PropertyName { get; }
            public string Title { get; }

            public bool IsEnabled
            {
                get => GetValue<bool>(nameof(IsEnabled));
                set => SetValue(nameof(IsEnabled), value);
            }

            public string Operator
            {
                get => GetValue<string>(nameof(Operator));
                set => SetValue(nameof(Operator), value);
            }

            public string Value
            {
                get => GetValue<string>(nameof(Value));
                set => SetValue(nameof(Value), value);
            }
        }

    }
}
