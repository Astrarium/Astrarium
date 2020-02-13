using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Config
{
    public class Settings : DynamicObject, ISettings, INotifyPropertyChanged
    {
        /// <summary>
        /// Path to store apllication settings
        /// </summary>
        private readonly string SETTINGS_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ADK", "Settings.json");

        /// <summary>
        /// Contains settings values
        /// </summary>
        private readonly Dictionary<string, object> SettingsValues = new Dictionary<string, object>();

        /// <summary>
        /// Saved states of settings values
        /// </summary>
        private readonly Dictionary<string, string> Snapshots = new Dictionary<string, string>();

        /// <summary>
        /// Raised when setting is changed
        /// </summary>
        public event Action<string, object> SettingValueChanged;

        /// <summary>
        /// Raised when property value is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets value indicating whether any setting was changed or not
        /// </summary>
        public bool IsChanged
        {
            get
            {
                if (!Snapshots.ContainsKey("Current"))
                {
                    return false;
                }
                else
                {
                    return !ToString().Equals(Snapshots["Current"]);
                }
            }
        }

        /// <summary>
        /// Saves snapshot with specified name
        /// </summary>
        /// <param name="snapshotName">Name of shapshot</param>
        public void Save(string snapshotName)
        {
            Snapshots[snapshotName] = ToString();
        }

        public void Load(string snapshotName)
        {
            if (Snapshots.ContainsKey(snapshotName))
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(Snapshots[snapshotName])))
                {
                    Load(stream);
                }
            }
            else
            {
                Load();
            }
        }

        public bool Get(string settingName, bool defaultValue = false)
        {
            return Get<bool>(settingName, defaultValue);
        }

        public T Get<T>(string settingName, T defaultValue = default(T))
        {
            if (SettingsValues.ContainsKey(settingName))
            {
                return (T)SettingsValues[settingName];
            }
            else
            {
                return defaultValue;
            }
        }

        public void Set(string settingName, object value)
        {
            if (SettingsValues.ContainsKey(settingName))
            {
                var oldValue = SettingsValues[settingName];
                if (!oldValue.Equals(value))
                {
                    SettingsValues[settingName] = value;
                    SettingValueChanged?.Invoke(settingName, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(settingName));
                }
            }
            else
            {
                SettingsValues[settingName] = value;
            }

            if (value is INotifyCollectionChanged)
            {
                var collection = (INotifyCollectionChanged)value;
                collection.CollectionChanged -= HandleObservableCollectionChanged;
                collection.CollectionChanged += HandleObservableCollectionChanged;
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.Name;
            if (!SettingsValues.TryGetValue(name, out result))
            {
                result = name;
            }
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Set(binder.Name, value);
            return true;
        }

        public void Load()
        {
            if (File.Exists(SETTINGS_PATH))
            {
                using (var stream = new FileStream(SETTINGS_PATH, FileMode.Open))
                {
                    Load(stream);
                }
            }
            else
            {
                Debug.Write($"Setting file {SETTINGS_PATH} not found, skip loading settings.");
            }
        }

        private void HandleObservableCollectionChanged(object value, NotifyCollectionChangedEventArgs e)
        {
            string settingName = SettingsValues.FirstOrDefault(x => x.Value == value).Key;
            var settingValueChangedInvocationList = SettingValueChanged.GetInvocationList();
            foreach (var item in settingValueChangedInvocationList)
            {
                (item as Action<string, object>).BeginInvoke(settingName, value, null, null);
            }

            var propertyChangedInvocationList = PropertyChanged.GetInvocationList();
            foreach (var item in propertyChangedInvocationList)
            {
                (item as PropertyChangedEventHandler).BeginInvoke(this, new PropertyChangedEventArgs(settingName), null, null);
            }
        }

        private void Load(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {
                JsonSerializer ser = new JsonSerializer();
                ser.Converters.Add(new SettingsJsonConverter(SettingsValues.ToDictionary(s => s.Key, s => s.Value.GetType())));
                try
                {
                    Load(ser.Deserialize<Dictionary<string, object>>(jsonReader));
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
        }

        private void Load(IDictionary<string, object> savedSettings)
        {
            foreach (var savedSetting in savedSettings)
            {
                Set(savedSetting.Key, savedSetting.Value);
            }
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SETTINGS_PATH));
            using (var stream = new FileStream(SETTINGS_PATH, FileMode.Create))
            {
                Save(stream);
            }
        }

        private void Save(Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(ToString());
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(SettingsValues, new JsonSerializerSettings() { Formatting = Formatting.Indented, ContractResolver = new WritablePropertiesOnlyResolver() });            
        }

        private class WritablePropertiesOnlyResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);
                return props.Where(p => p.Writable).ToList();
            }
        }

        private class SettingsJsonConverter : JsonConverter
        {
            private IDictionary<string, Type> settingsTypes;

            public SettingsJsonConverter(IDictionary<string, Type> settingsTypes)
            {
                this.settingsTypes = settingsTypes;
            }

            public override bool CanConvert(Type objectType)
            {
                return typeof(object) == objectType;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JToken jObject = JToken.ReadFrom(reader);

                string name = reader.Path;
                Type type = settingsTypes[name];

                return jObject.ToObject(type);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {

            }
        }
    }
}
