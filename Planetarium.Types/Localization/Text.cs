using System;
using System.Collections.Generic;
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

namespace Planetarium.Types.Localization
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

        public static string FileName { get; set; } = "Translation";
        public static string FileExtension { get; set; } = "txt";
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

                System.Diagnostics.Trace.WriteLine("Live refs: " + refs.Count);

                LocalizedObjectsRefs.Clear();
                LocalizedObjectsRefs.AddRange(refs);
            }
        }

        public static event Action LocaleChanged;

        public static CultureInfo[] GetLocales()
        {
            Regex regex = new Regex($"^.*\\.{FileName}-(.+)\\.{FileExtension}$");

            var resourceNames = AppDomain.CurrentDomain.GetAssemblies()
                .Where(p => !p.IsDynamic).Distinct()
                .SelectMany(a => a.GetManifestResourceNames()
                .Where(rn => regex.IsMatch(rn)));
            
            return resourceNames
                .Select(rn => regex.Match(rn).Groups[1].Value)
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

            string[] commentSigns = new string[] { "\\\\", "#", "-", ";", "!" };

            string[] localizationFiles = GetCurrentLocales()
                .Concat(new[] { DefaultLanguage })
                .Distinct()
                .Select(c => $"{FileName}-{c}.{FileExtension}").ToArray();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic).Distinct())
            {
                foreach (string lf in localizationFiles)
                {
                    string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(rn => rn.EndsWith($".{lf}"));
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
                                        if (!string.IsNullOrEmpty(line) &&
                                            commentSigns.All(comment => !line.StartsWith(comment)))
                                        {
                                            string[] keyValue = line.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);

                                            if (keyValue.Length == 2)
                                            {
                                                string key = keyValue[0].Trim();
                                                string value = keyValue[1].Trim();

                                                if (!LocalizationStrings.ContainsKey(key))
                                                {
                                                    try
                                                    {
                                                        value = Regex.Unescape(value);
                                                    }
                                                    catch { }
                                                    LocalizationStrings[key] = value;
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

        private static string[] GetCurrentLocales()
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

        ~Text()
        {

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
