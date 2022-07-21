using Astrarium.Plugins.Journal.Types;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.Journal.ViewModels
{
    public class ObservationVM : ViewModelBase
    {
        /// <summary>
        /// Holder class to adapt observation target entry to CelestialObjectPicker control
        /// </summary>
        private class DummyCelestialObject : CelestialObject
        {
            public string TypeHolder { get; set; }
            public string CommonNameHolder { get; set; }
            public string NameHolder { get; set; }

            public override string[] Names => new string[] { NameHolder };
            public override string[] DisplaySettingNames => new string[0];
            public override string Type => TypeHolder;
            public override string CommonName => CommonNameHolder;
        }

        public ObservationVM()
        {
            OkCommand = new Command(Ok);

            CelestialBody = new DummyCelestialObject()
            {
                NameHolder = "(17) Acrux",
                CommonNameHolder = "(17) Acrux",
                TypeHolder = "Asteroid"
            };
        }

        public ICommand OkCommand { get; private set; }

        public DateTime Begin { get; set; }
        public DateTime End { get; set; }

        public Observation Observation
        {
            get; set;
        }

        public CelestialObject CelestialBody
        {
            get => GetValue<CelestialObject>(nameof(CelestialBody));
            set => SetValue(nameof(CelestialBody), value);
        }

        private void Ok()
        {
            Close(true);
        }
    }
}
