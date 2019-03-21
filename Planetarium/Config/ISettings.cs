using System;

namespace Planetarium.Config
{
    /// <summary>
    /// Defines methods to access to application settings
    /// </summary>
    public interface ISettings
    {
        /// <summary>
        /// Gets setting with specified name and casts its value to desired type.
        /// </summary>
        /// <typeparam name="T">Type of setting value</typeparam>
        /// <param name="settingName">Unique name of setting</param>
        /// <param name="defaultValue">Default value of the setting</param>
        /// <returns>Setting value, of defailt value for type <typeparamref name="T"/>.</returns>
        T Get<T>(string settingName, T defaultValue = default(T));

        /// <summary>
        /// Sets value of setting with specified name
        /// </summary>
        /// <param name="settingName"></param>
        /// <param name="value"></param>
        void Set(string settingName, object value);

        event Action<string, object> SettingValueChanged;

        bool IsChanged { get; }

        void Save();
        void Load();
        void Reset();
    }
}
