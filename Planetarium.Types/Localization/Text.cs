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

        public static string Get(string key)
        {
            if (LocalizationStrings.ContainsKey(key))
                return LocalizationStrings[key];
            else
                return $"?{key}";
        }

        public static string FileName { get; set; } = "Translation";
        public static string FileExtension { get; set; } = "txt";
        public static string DefaultLanguage { get; set; } = "en";

        public static void SetLocale(CultureInfo culture)
        {
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            LoadLocalizationStrings();
        }

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

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Get(resourceKey);
        }

        #endregion MarkupExtension
    }
}
