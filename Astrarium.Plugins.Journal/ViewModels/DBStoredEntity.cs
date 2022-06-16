using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.ViewModels
{
    public abstract class DBStoredEntity : PropertyChangedBase
    {
        public event Action<object, Type, string, object> DatabasePropertyChanged;

        public DateTime Date
        {
            get => GetValue<DateTime>(nameof(Date));
            set
            {
                SetValue(nameof(Date), value);
                NotifyPropertyChanged(nameof(DateString));
            }
        }

        public string DateString => Date.ToString("dd MMM yyyy");

        public string TimeString
        {
            get => GetValue<string>(nameof(TimeString));
            set => SetValue(nameof(TimeString), value);
        }

        protected override void NotifyPropertyChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var prop = GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var attr = prop.GetCustomAttribute<DBStoredAttribute>();
                if (attr != null)
                {
                    var keyProp = GetType().GetProperty(attr.Key ?? "Id");
                    var key = keyProp.GetValue(this);

                    DatabasePropertyChanged?.Invoke(prop.GetValue(this), attr.Entity, attr.Field, key);
                }
            }

            base.NotifyPropertyChanged(propertyNames);
        }
    }
}
