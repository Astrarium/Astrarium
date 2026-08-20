using System;

namespace Astrarium.Types
{
    public class ObjectInfoExtension
    {
        public Delegate ViewModelProvider { get; private set; }
        public string Title { get; private set; }
        public Type ViewType { get; private set; }
        public ObjectInfoExtension(string title, Type viewType, Delegate viewModelProvider)
        {
            Title = title;
            ViewType = viewType;
            ViewModelProvider = viewModelProvider;
        }
    }
}
