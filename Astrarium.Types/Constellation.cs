namespace Astrarium.Types
{
    /// <summary>
    /// Holds constellation code and names 
    /// </summary>
    public class Constellation
    {
        /// <summary>
        /// IAU constellation code, 3 letters
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// IAU international latin name
        /// </summary>
        public string LatinName { get; set; }

        /// <summary>
        /// IAU international latin name, genitive form
        /// </summary>
        public string LatinGenitiveName { get; set; }

        /// <summary>
        /// Local constellation name (depends on selected UI language)
        /// </summary>
        public string LocalName => Text.Get($"ConName.{Code}");

        /// <summary>
        /// Local constellation name, genitive form (depends on selected UI language)
        /// </summary>
        public string LocalGenitiveName => Text.Get($"ConGenName.{Code}");
    }
}
