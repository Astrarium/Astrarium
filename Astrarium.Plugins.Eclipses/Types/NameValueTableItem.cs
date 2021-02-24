namespace Astrarium.Plugins.Eclipses.Types
{
    public class NameValueTableItem
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public NameValueTableItem(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
