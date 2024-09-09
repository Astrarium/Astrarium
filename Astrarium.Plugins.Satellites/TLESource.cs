using Astrarium.Types;
using System;

namespace Astrarium.Plugins.Satellites
{
    /// <summary>
    /// Stores info about TLE (two-line elements) source located in the web.
    /// </summary>
    public class TLESource : PropertyChangedBase
    {
        /// <summary>
        /// Web address to download TLE data
        /// </summary>
        public string Url
        {
            get => GetValue<string>(nameof(Url));
            set => SetValue(nameof(Url), value);
        }

        /// <summary>
        /// Flag indicating TLE source is used
        /// </summary>
        public bool IsEnabled
        {
            get => GetValue<bool>(nameof(IsEnabled));
            set => SetValue(nameof(IsEnabled), value);
        }

        /// <summary>
        /// File name (without extension) for local saving TLE data.
        /// </summary>
        public string FileName
        {
            get => GetValue<string>(nameof(FileName));
            set => SetValue(nameof(FileName), value);
        }

        /// <summary>
        /// Timestamp when TLEs have been updated last time.
        /// </summary>
        public DateTime? LastUpdated
        {
            get => GetValue<DateTime?>(nameof(LastUpdated));
            set => SetValue(nameof(LastUpdated), value);
        }
    }
}
