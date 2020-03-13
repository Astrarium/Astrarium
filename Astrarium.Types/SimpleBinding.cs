using System.ComponentModel;

namespace Astrarium.Types
{
    public class SimpleBinding
    {
        public INotifyPropertyChanged Source { get; private set; }
        public string PropertyName { get; private set; }
        public SimpleBinding(INotifyPropertyChanged source, string propertyName)
        {
            Source = source;
            PropertyName = propertyName;
        }
    }
}
