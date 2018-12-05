using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    /// <summary>
    /// Defines methods to work with application settings
    /// </summary>
    public interface ISettings
    {
        /// <summary>
        /// Gets setting with specified name and casts its value to desired type.
        /// </summary>
        /// <typeparam name="T">Type of setting value</typeparam>
        /// <param name="settingName">Unique name of setting</param>
        /// <returns>Setting value, of defailt value for type <typeparamref name="T"/>.</returns>
        T Get<T>(string settingName);

        /// <summary>
        /// Sets value of setting with specified name
        /// </summary>
        /// <param name="settingName"></param>
        /// <param name="value"></param>
        void Set(string settingName, object value);

        /// <summary>
        /// Loads settings
        /// </summary>
        void Load();

        /// <summary>
        /// Saves settings
        /// </summary>
        void Save();
    }
}
