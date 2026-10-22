using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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

        

        public IBinding AddBinding(INotifyPropertyChanged source, string sourcePropertyName, string targetPropertyName)
        {
            var binding = new Binding(source, sourcePropertyName, targetPropertyName);
            bindings.Add(binding);
            binding.Source.PropertyChanged += SourcePropertyChangedHandler;
            return binding;
        }

        private Dictionary<string, object> backingFields = new Dictionary<string, object>();
        private List<Binding> bindings = new List<Binding>();

        private void SourcePropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            var binding = bindings.FirstOrDefault(b => b.Source == sender && b.SourcePropertyName == e.PropertyName);
            if (binding != null)
            {
                NotifyPropertyChanged(binding.TargetPropertyName);
            }
        }

        
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

        #region IBinding interface

        public interface IBinding
        {
            IBinding WithSourceToTargetConverter<T>(Func<T, object> converter);
            IBinding WithTargetToSourceConverter<T>(Func<T, object> converter);

            IBinding WithConverter<TSource, TTarget>(
                Func<TSource, TTarget> sourceToTarget,
                Func<TTarget, TSource> targetToSource);
        }

        #endregion IBinding interface

        #region IBinding implementation

        private class Binding : IBinding
        {
            public INotifyPropertyChanged Source { get; private set; }
            public string SourcePropertyName { get; private set; }
            public string TargetPropertyName { get; private set; }
            private Func<object, object> SourceToTargetConverter { get; set; }
            private Func<object, object> TargetToSourceConverter { get; set; }

            public Binding(INotifyPropertyChanged source, string sourcePropertyName, string targetPropertyName)
            {
                Source = source;
                SourcePropertyName = sourcePropertyName;
                TargetPropertyName = targetPropertyName;
            }

            public T GetValue<T>()
            {
                object value;
                // TODO: use dynamic here?
                if (Source is ISettings settings)
                    value = settings.Get<object>(SourcePropertyName);
                else
                    value = Source.GetType().GetProperty(SourcePropertyName).GetValue(Source);

                if (SourceToTargetConverter != null)
                    return (T)SourceToTargetConverter(value);
                else
                    return (T)value;
            }

            public void SetValue(object value)
            {
                object converted = value;
                if (TargetToSourceConverter != null)
                {
                    converted = TargetToSourceConverter(value);
                }

                if (Source is ISettings settings)
                    settings.Set(SourcePropertyName, converted);
                else
                    Source.GetType().GetProperty(SourcePropertyName).SetValue(Source, converted);
            }

            IBinding IBinding.WithSourceToTargetConverter<T>(Func<T, object> converter)
            {
                SourceToTargetConverter = x => converter((T)x);
                return this;
            }

            IBinding IBinding.WithTargetToSourceConverter<T>(Func<T, object> converter)
            {
                TargetToSourceConverter = x => converter((T)x);
                return this;
            }

            IBinding IBinding.WithConverter<TSource, TTarget>(Func<TSource, TTarget> sourceToTarget, Func<TTarget, TSource> targetToSource)
            {
                SourceToTargetConverter = x => sourceToTarget((TSource)x);
                TargetToSourceConverter = x => targetToSource((TTarget)x);
                return this;
            }
        }

        #endregion IBinding implementation
    }
}
