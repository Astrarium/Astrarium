using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    public abstract class PropertyChangedBase : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Raised when the ViewModel property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies subscribers about changing property or properties. 
        /// </summary>
        /// <param name="propertyName">Changed property name(s).</param>
        protected virtual void NotifyPropertyChanged(params string[] propertyName)
        {
            foreach (string pn in propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pn));
            }
        }

        private List<SimpleBinding> bindings = new List<SimpleBinding>();
        public void AddBinding(SimpleBinding binding)
        {
            bindings.Add(binding);
            binding.Source.PropertyChanged += SourcePropertyChangedHandler;
        }

        public SimpleBinding FindBinding(string targetPropertyName)
        {
            return bindings.FirstOrDefault(b => b.TargetPropertyName == targetPropertyName);
        }

        private void SourcePropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            var binding = bindings.FirstOrDefault(b => b.Source == sender && b.SourcePropertyName == e.PropertyName);
            if (binding != null)
            {
                NotifyPropertyChanged(binding.TargetPropertyName);
            }
        }

        private Dictionary<string, object> backingFields = new Dictionary<string, object>();
        protected T GetValue<T>(string propertyName, T defaultValue = default(T))
        {
            var binding = bindings.FirstOrDefault(b => b.TargetPropertyName == propertyName);
            if (binding != null)
                return binding.GetValue<T>();
            else
            {
                if (!backingFields.ContainsKey(propertyName))
                {
                    backingFields[propertyName] = defaultValue;
                }
                return (T)backingFields[propertyName];
            }
        }

        protected void SetValue(string propertyName, object value)
        {
            var binding = bindings.FirstOrDefault(b => b.TargetPropertyName == propertyName);
            if (binding != null)
                binding.SetValue(value);
            else
                backingFields[propertyName] = value;
            NotifyPropertyChanged(propertyName);
        }

        /// <summary>
        /// Disposes allocated resources
        /// </summary>
        public virtual void Dispose()
        {
            foreach (var binding in bindings)
            {
                binding.Source.PropertyChanged -= SourcePropertyChangedHandler;
            }
            bindings.Clear();
        }
    }
}
