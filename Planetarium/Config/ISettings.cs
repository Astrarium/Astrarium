using System;
using System.ComponentModel;

namespace Planetarium.Config
{
    /// <summary>
    /// Defines methods to access to application settings
    /// </summary>
    public interface ISettings : INotifyPropertyChanged
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

        /// <summary>
        /// Raised when setting has been changed
        /// </summary>
        event Action<string, object> SettingValueChanged;

        /// <summary>
        /// Gets value indicating settings have been modified
        /// </summary>
        bool IsChanged { get; }

        /// <summary>
        /// Saves settings values to persistent storage
        /// </summary>
        void Save();

        /// <summary>
        /// Loads settings values from persistent storage
        /// </summary>
        void Load();

        /// <summary>
        /// Reverts settings values to defaults
        /// </summary>
        void Reset();
    }
}
