using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Astrarium.Types.Localization
{
    public class Text : MarkupExtension
    {
        private static Dictionary<string, string> LocalizationStrings = new Dictionary<string, string>();

        private static List<LocalizedObjectHolder> LocalizedObjectsRefs = new List<LocalizedObjectHolder>();

        public static string Get(string key)
        {
            if (LocalizationStrings.ContainsKey(key))
                return LocalizationStrings[key];
            else
                return $"{{{key}}}";
        }

        public static string Get(string key, params (string key, string value)[] args)
        {
            if (LocalizationStrings.ContainsKey(key))
            {
                string format = LocalizationStrings[key];
                foreach (var arg in args)
                {
                    format = format.Replace($"{{{arg.key}}}", arg.value);
                }
                return format;
            }
            else
                return $"{{{key}}}";
        }

        public static string Get(IDictionary<string, string> localizations)
        {
            string key = localizations.Keys.FirstOrDefault(k => k == currentCulture.Name);
            if (!string.IsNullOrEmpty(key))
                return localizations[key];
            else
                return localizations.First().Value;
        }

        public static string FileName { get; set; } = "Text";
        public static string FileExtension { get; set; } = "ini";
        public static string DefaultLanguage { get; set; } = "en";

        private static CultureInfo currentCulture = null;

        public static void SetLocale(CultureInfo culture)
        {
            if (!culture.Equals(currentCulture))
            {
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                LoadLocalizationStrings();
                LocaleChanged?.Invoke();
                currentCulture = culture;

                foreach (var lo in LocalizedObjectsRefs)
                {
                    WeakReference wr = lo.ObjectReference;

                    if (wr.IsAlive)
                    {                        
                        object targetObject = lo.ObjectReference.Target;
                        object targetProperty = lo.Property;
                        string text = Get(lo.ResourceKey);

                        if (targetObject != null)
                        {
                            if (targetProperty is DependencyProperty)
                            {
                                DependencyObject obj = targetObject as DependencyObject;
                                DependencyProperty prop = targetProperty as DependencyProperty;

                                Action updateAction = () => obj.SetValue(prop, text);

                                // Check whether the target object can be accessed from the
                                // current thread, and use Dispatcher.Invoke if it can't

                                if (obj.CheckAccess())
                                    updateAction();
                                else
                                    obj.Dispatcher.Invoke(updateAction);
                            }
                            else // _targetProperty is PropertyInfo
                            {
                                PropertyInfo prop = targetProperty as PropertyInfo;
                                prop.SetValue(targetObject, text, null);
                            }
                        }                        
                    }
                }

                var refs = LocalizedObjectsRefs.Where(lo => lo.ObjectReference.IsAlive).ToList();

                LocalizedObjectsRefs.Clear();
                LocalizedObjectsRefs.AddRange(refs);
            }
        }

        public static event Action LocaleChanged;

        private static List<string> languages = new List<string>();
        public static CultureInfo[] GetLocales()
        {
            return languages
                .Select(lang =>
                {
                    try { return CultureInfo.GetCultureInfo(lang); }
                    catch { return null; }
                })
                .Where(ci => ci != null).Distinct().ToArray();
        }

        static Text()
        {
            LoadLocalizationStrings();
        }

        private static void LoadLocalizationStrings()
        {
            LocalizationStrings.Clear();

            string[] commentSigns = new string[] { "#", ";" };
            string localizationFile = $"{FileName}.{FileExtension}";
            Regex keyRegex = new Regex("^\\s*\\[\\s*(.+)\\s*\\]\\s*$");
            string[] currentLanguages = GetCurrentLanguages();
            string key = null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic).Distinct())
            {
                string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(rn => rn.EndsWith($".{localizationFile}"));
                if (resourceName != null)
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    line = line.Trim();

                                    // if not empty or not a comment
                                    if (!string.IsNullOrEmpty(line) && commentSigns.All(comment => !line.StartsWith(comment)))
                                    {
                                        var match = keyRegex.Match(line);
                                        
                                        // key string
                                        if (match.Success)
                                        {
                                            key = match.Groups[1].Value.Trim();
                                        }
                                        // value string
                                        else 
                                        {
                                            string[] langValue = line.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);

                                            if (langValue.Length == 2)
                                            {
                                                string lang = langValue[0].Trim();

                                                if (currentLanguages.Contains(lang))
                                                {
                                                    string value = langValue[1].Trim();
                                                    try
                                                    {
                                                        value = Regex.Unescape(value);
                                                    }
                                                    catch { }

                                                    if (!LocalizationStrings.ContainsKey(key))
                                                    {
                                                        LocalizationStrings[key] = value;
                                                    }
                                                }
                                                
                                                if (!languages.Contains(lang))
                                                {
                                                    languages.Add(lang);
                                                } 
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static string[] GetCurrentLanguages()
        {
            List<string> cultures = new List<string>();
            CultureInfo culture = CultureInfo.CurrentUICulture;
            do
            {
                cultures.Add(culture.Name);
                culture = culture.Parent;
            }
            while (!string.IsNullOrEmpty(culture.Name));

            return cultures.ToArray();
        }

        #region MarkupExtension

        private string resourceKey;

        public Text(string resourceKey)
        {
            this.resourceKey = resourceKey;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            
            if (target != null)
            {
                LocalizedObjectsRefs.Add(new LocalizedObjectHolder()
                {
                    ObjectReference = new WeakReference(target.TargetObject),
                    Property = target.TargetProperty,
                    ResourceKey = resourceKey
                });
            }
            
            return Get(resourceKey);
        }
        
        #endregion MarkupExtension

        private class LocalizedObjectHolder
        {
            public WeakReference ObjectReference { get; set; }
            public object Property { get; set; }
            public string ResourceKey { get; set; }
        }
    }
}
