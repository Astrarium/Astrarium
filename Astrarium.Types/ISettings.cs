using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Astrarium.Types
{
    /// <summary>
    /// Defines methods to access to application settings
    /// </summary>
    public interface ISettings : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets boolean setting with specified name.
        /// </summary>
        /// <param name="settingName">Unique name of setting</param>
        /// <param name="defaultValue">Default value of the setting</param>
        /// <returns>Setting value.</returns>
        bool Get(string settingName, bool defaultValue = false);

        /// <summary>
        /// Gets setting with specified name and casts its value to desired type.
        /// </summary>
        /// <typeparam name="T">Type of setting value</typeparam>
        /// <param name="settingName">Unique name of setting</param>
        /// <param name="defaultValue">Default value of the setting</param>
        /// <returns>Setting value, of default value for type <typeparamref name="T"/>.</returns>
        T Get<T>(string settingName, T defaultValue = default(T));

        /// <summary>
        /// Sets value of setting with specified name
        /// </summary>
        /// <param name="settingName"></param>
        /// <param name="value"></param>
        void Set(string settingName, object value);

        /// <summary>
        /// Sets value of setting with specified name and saves settings
        /// </summary>
        /// <param name="settingName"></param>
        /// <param name="value"></param>
        void SetAndSave(string settingName, object value);

        /// <summary>
        /// Gets all settings with specified type
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        ICollection<string> OfType<TValue>();

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
        /// Saves settings into in-memory snapshot with specified name.
        /// </summary>
        /// <param name="snapshotName">Name of the snapshot to be saved.</param>
        void Save(string snapshotName);

        /// <summary>
        /// Loads settings from in-memory snapshot with specified name.
        /// </summary>
        /// <param name="snapshotName">Name of the snapshot to be loaded.</param>
        void Load(string snapshotName);

        /// <summary>
        /// Defines set of settings.
        /// </summary>
        void Define(ICollection<SettingDefinition> definitions);
    }
}
