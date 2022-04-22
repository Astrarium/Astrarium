using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Planner.ImportExport
{
    public enum PlanType
    {
        Astrarium = 0,
        SkySafari = 1,
        CartesDuCiel = 2
    }

    // TODO: singleton, interface
    public class PlanFactory
    {
        private readonly Dictionary<string, PlanType> formats = new Dictionary<string, PlanType>()
        {
            ["Astrarium Observation Plan (*.plan)|*.plan"] = PlanType.Astrarium,
            ["SkySafari Observing List (*.skylist)|*.skylist"] = PlanType.SkySafari,
            ["Cartes du Ciel Observing List (*.txt)|*.txt"] = PlanType.CartesDuCiel
        };

        private readonly ISky sky;

        public PlanFactory(ISky sky)
        {
            this.sky = sky;
        }

        public string FormatsString => string.Join("|", formats.Keys);

        public PlanType GetFormat(int index)
        {
            return formats.ElementAt(index - 1).Value;
        }

        public PlanType GetFormat(string extension)
        {
            switch (extension)
            {
                case ".plan": return PlanType.Astrarium;
                case ".skylist": return PlanType.SkySafari;
                case ".txt": return PlanType.CartesDuCiel;
                default: return PlanType.Astrarium;
            }
        }


        public IPlan Create(PlanType type)
        {
            switch (type)
            {
                case PlanType.Astrarium:
                    return new AstrariumPlan(sky);
                case PlanType.SkySafari:
                    return new SkySafariPlan(sky);
                case PlanType.CartesDuCiel:
                    return new CartesDuCielPlan(sky);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
