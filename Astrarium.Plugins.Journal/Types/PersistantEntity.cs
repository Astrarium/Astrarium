using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    /// <summary>
    /// Base class for all models which can be stored in database
    /// </summary>
    public abstract class PersistantEntity : PropertyChangedBase
    {
        /// <summary>
        /// This event is fired each time when model property marked with DBStored attribute is modified.
        /// Normally the event handler should reflect this change in the database.
        /// </summary>
        public event Action<object, Type, string, object> DatabasePropertyChanged;

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
