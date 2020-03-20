using System;
using System.ComponentModel;

namespace Astrarium.Types
{
    public class SimpleBinding
    {
        public INotifyPropertyChanged Source { get; private set; }
        public string SourcePropertyName { get; private set; }
        public string TargetPropertyName { get; private set; }
        public Func<object, object> SourceToTargetConverter { get; set; }
        public Func<object, object> TargetToSourceConverter { get; set; }

        public SimpleBinding(INotifyPropertyChanged source, string sourcePropertyName, string targetPropertyName)
        {
            Source = source;
            SourcePropertyName = sourcePropertyName;
            TargetPropertyName = targetPropertyName;
        }

        public T GetValue<T>()
        {
            object value;
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
    }
}
