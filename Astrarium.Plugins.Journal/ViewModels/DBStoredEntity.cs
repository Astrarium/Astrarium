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

        public abstract DateTime SessionDate { get; }

        public DateTime Begin
        {
            get => GetValue<DateTime>(nameof(Begin));
            set => SetValue(nameof(Begin), value);
        }

        public DateTime End
        {
            get => GetValue<DateTime>(nameof(End));
            set => SetValue(nameof(End), value);
        }

        public ICollection<Attachment> Attachments
        {
            get => GetValue<ICollection<Attachment>>(nameof(Attachments), new List<Attachment>());
            set => SetValue(nameof(Attachments), value);
        }

        public string DateString => Begin.ToString("dd MMM yyyy");

        public string TimeString => $"{Begin:HH:mm}-{End:HH:mm}";

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
