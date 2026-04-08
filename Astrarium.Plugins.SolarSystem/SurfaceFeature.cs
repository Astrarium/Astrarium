namespace Astrarium.Plugins.SolarSystem
{
    internal class SurfaceFeature
    {
        public float Latitude { get; private set; }
        public float Longitude { get; private set; }
        public string Name { get; private set; }
        public float Diameter { get; private set; }
        public string TypeCode { get; private set; }
        public SurfaceFeature(string name, string type, double longitude, double latitude, double diameter)
        {
            Name = name;
            TypeCode = type;
            Latitude = (float)latitude;
            Longitude = (float)longitude;
            Diameter = (float)diameter;
        }
    }
}
