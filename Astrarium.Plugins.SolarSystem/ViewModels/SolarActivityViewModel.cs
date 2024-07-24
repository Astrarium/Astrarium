using Astrarium.Plugins.SolarSystem;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem.ViewModels
{
    public class SolarActivityViewModel : ViewModelBase
    {
        private readonly SolarRegionSummaryManager srsManager;
        private readonly SolarSystemRenderer renderer;

        private double julianDay;
        private double utcOffset;

        public Command<string> MagTypeInfoCommand { get; private set; }
        public Command<string> ZurichClassificationCommand { get; private set; }

        public SolarActivityViewModel(SolarSystemRenderer renderer, SolarRegionSummaryManager srsManager)
        {
            this.srsManager = srsManager;
            this.renderer = renderer;
            this.srsManager.OnRequestComplete += Update;

            MagTypeInfoCommand = new Command<string>(MagTypeInfo);
            ZurichClassificationCommand = new Command<string>(ZurichClassification);
        }

        public override void Dispose()
        {
            srsManager.OnRequestComplete -= Update;
            renderer.SelectedSolarRegion = null;
        }

        public ICollection<SolarRegionI> RegionsI
        {
            get => GetValue<ICollection<SolarRegionI>>(nameof(RegionsI));
            set => SetValue(nameof(RegionsI), value);
        }

        public ICollection<SolarRegionIa> RegionsIa
        {
            get => GetValue<ICollection<SolarRegionIa>>(nameof(RegionsIa));
            set => SetValue(nameof(RegionsIa), value);
        }

        public ICollection<SolarRegionII> RegionsII
        {
            get => GetValue<ICollection<SolarRegionII>>(nameof(RegionsII));
            set => SetValue(nameof(RegionsII), value);
        }

        public int WolfNumber
        {
            get => GetValue<int>(nameof(WolfNumber));
            set => SetValue(nameof(WolfNumber), value);
        }

        public SolarRegion SelectedSolarRegion
        {
            get => renderer.SelectedSolarRegion;
            set => renderer.SelectedSolarRegion = value;
        }

        public void SetDate(double julianDay, double utcOffset)
        {
            this.julianDay = julianDay;
            this.utcOffset = utcOffset;
            Update();
        }

        private void Update()
        {
            var srs = srsManager.GetSRSForJulianDate(julianDay, utcOffset);
            if (srs != null)
            {
                RegionsI = srs.RegionsI;
                RegionsIa = srs.RegionsIa;
                RegionsII = srs.RegionsII;
                WolfNumber = srs.WolfNumber;
            }
        }

        private void MagTypeInfo(string magType)
        {
            ViewManager.ShowMessageBox($"MagType: {MagTypeFormatter.MagType(magType)}", Text.Get($"SolarActivity.MagType.{magType}"));
        }

        private void ZurichClassification(string z)
        {
            if (z.Length != 3) return;

            string Z = new string(z[0], 1);
            string p = new string(z[1], 1);
            string c = new string(z[2], 1);

            string Z_descr = Text.Get($"SolarActivity.Zpc.{Z}**");
            string p_descr = Text.Get($"SolarActivity.Zpc.*{p}*");
            string c_descr = Text.Get($"SolarActivity.Zpc.**{c}");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"**{Text.Get("SolarActivity.Zpc.Z")}:** *{Z}*");
            sb.AppendLine();
            sb.AppendLine(Z_descr);
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine($"**{Text.Get("SolarActivity.Zpc.p")}:** *{p}*");
            sb.AppendLine();
            sb.AppendLine(p_descr);
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine($"**{Text.Get("SolarActivity.Zpc.c")}:** *{c}*");
            sb.AppendLine();
            sb.AppendLine(c_descr);
            sb.AppendLine();

            ViewManager.ShowMessageBox($"Zurich/McIntosh classification", sb.ToString());
        }
    }
}
