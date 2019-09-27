using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Planetarium.Types.Localization
{
    public class Loc : MarkupExtension
    {
        static Dictionary<string, string> LocalizationStrings = new Dictionary<string, string>();

        static Loc()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.IsDynamic))
            {
                string[] resourceNames = assembly.GetManifestResourceNames();
                foreach (string resourceName in resourceNames)
                {
                    if (resourceName.EndsWith("Localization.txt"))
                    {
                        string[] lines = ReadAllResourceLines(assembly, resourceName);
                        if (lines != null)
                        {
                            foreach (string line in lines)
                            {
                                try
                                {
                                    string[] val = line.Split('=');
                                    LocalizationStrings[val[0].Trim()] = val[1].Trim();
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
        }

        private static string[] ReadAllResourceLines(Assembly assembly, string resourceName)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return EnumerateLines(reader).ToArray();
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        private static IEnumerable<string> EnumerateLines(TextReader reader)
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }


        private string resourceKey;

        public Loc(string resourceKey)
        {
            this.resourceKey = resourceKey;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (LocalizationStrings.ContainsKey(resourceKey))
            {
                return LocalizationStrings[resourceKey];
            }
            else
            {
                return $"?{{{resourceKey}}}";
            }
        }
    }
}
