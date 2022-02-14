using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Planner.ImportExport
{
    public enum ReadWriterType
    {
        Astrarium = 0,
        SkySafari = 1,
        CartesDuCiel = 2
    }

    public class PlanReadWriterFactory 
    {
        private readonly Dictionary<string, ReadWriterType> formats = new Dictionary<string, ReadWriterType>()
        {
            ["Astrarium Observation Plan (*.plan)|*.plan"] = ReadWriterType.Astrarium,
            ["SkySafari Observing List (*.skylist)|*.skylist"] = ReadWriterType.SkySafari,
            ["Cartes du Ciel Observing List (*.txt)|*.txt"] = ReadWriterType.CartesDuCiel
        };

        private readonly ISky sky;

        public PlanReadWriterFactory(ISky sky)
        {
            this.sky = sky;
        }

        public string FormatsString => string.Join("|", formats.Keys);

        public ReadWriterType GetFormat(int index)
        {
            return formats.ElementAt(index - 1).Value;
        }

        public IPlanReadWriter Create(ReadWriterType type)
        {
            switch (type)
            {
                case ReadWriterType.Astrarium:
                    throw new NotImplementedException();
                case ReadWriterType.SkySafari:
                    return new SkySafariPlanReadWriter(sky);
                case ReadWriterType.CartesDuCiel:
                    return new CartesDuCielPlanReadWriter(sky);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
