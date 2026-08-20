using Astrarium.Types;

namespace Astrarium.Plugins.Journal.Types
{
    public class Attachment : PropertyChangedBase
    {
        public string Id { get; set; }

        public string FilePath
        {
            get => GetValue(nameof(FilePath), "");
            set => SetValue(nameof(FilePath), value);
        }

        public string Title
        {
            get => GetValue(nameof(Title), "");
            set => SetValue(nameof(Title), value);
        }

        public string Comments
        {
            get => GetValue(nameof(Comments), "");
            set => SetValue(nameof(Comments), value);
        }
    }
}
