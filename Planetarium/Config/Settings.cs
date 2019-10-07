using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Config
{
    public class Settings : DynamicObject, ISettings, INotifyPropertyChanged
    {
        private readonly string SETTINGS_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ADK", "Settings.json");

        /// <summary>
        /// Contains settings values
        /// </summary>
        private Dictionary<string, object> SettingsValues = new Dictionary<string, object>();

        public event Action<string, object> SettingValueChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private Dictionary<string, string> snapshots = new Dictionary<string, string>();

        public bool IsChanged
        {
            get
            {
                if (!snapshots.ContainsKey("Current"))
                {
                    return false;
                }
                else
                {
                    string currentCache = Serialize();
                    return !currentCache.Equals(snapshots["Current"]);
                }
            }
        }

        public void Save(string snapshotName)
        {
            snapshots[snapshotName] = Serialize();
        }

        public void Load(string snapshotName)
        {
            if (snapshots.ContainsKey(snapshotName))
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(snapshots[snapshotName])))
                {
                    Load(stream);
                }
            }
            else
            {
                Load();
            }
        }

        private ILogger Logger;

        public Settings(ILogger logger)
        {
            Logger = logger;
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
                Logger.Info($"Setting file {SETTINGS_PATH} not found, skip loading settings.");
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
                try
                {
                    var settingsTree = ser.Deserialize<SavedSettings>(jsonReader);
                    Load(settingsTree);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            }
        }

        private void Load(SavedSettings settingsTree)
        {
            foreach (var setting in settingsTree)
            {
                var type = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.Equals(setting.Value.GetType()));

                if (type != null)
                {
                    Set(setting.Name, setting.Value);
                }
                else
                {
                    throw new FileFormatException($"Setting `{setting.Name}` has unknown type `{setting.Value.GetType()}`.");
                }
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
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                SavedSettings saved = new SavedSettings();
                foreach (var s in SettingsValues)
                {
                    saved.Add(new SavedSetting() { Name = s.Key, Value = s.Value });
                }

                JsonSerializer ser = new JsonSerializer() { Formatting = Formatting.Indented, ContractResolver = new WritablePropertiesOnlyResolver() };
                ser.Serialize(jsonWriter, saved);
                jsonWriter.Flush();
            }
        }

        private string Serialize()
        {
            SavedSettings saved = new SavedSettings();
            foreach (var s in SettingsValues)
            {
                saved.Add(new SavedSetting() { Name = s.Key, Value = s.Value });
            }

            return JsonConvert.SerializeObject(saved, new JsonSerializerSettings() { ContractResolver = new WritablePropertiesOnlyResolver() });            
        }

        private class WritablePropertiesOnlyResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);
                return props.Where(p => p.Writable).ToList();
            }
        }
    }
}
