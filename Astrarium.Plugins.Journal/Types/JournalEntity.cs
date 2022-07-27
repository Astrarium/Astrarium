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
    /// Base class for Observation and Session entities
    /// </summary>
    public abstract class JournalEntity : PersistantEntity
    {
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

        public bool IsEnabled
        {
            get => GetValue(nameof(IsEnabled), true);
            set => SetValue(nameof(IsEnabled), value);
        }

        public string DateString => Begin.ToString("dd MMM yyyy");

        public string TimeString => $"{Begin:HH:mm}-{End:HH:mm}";
    }
}
