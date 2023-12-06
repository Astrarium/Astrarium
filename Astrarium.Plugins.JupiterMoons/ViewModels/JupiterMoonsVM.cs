using Astrarium.Algorithms;
using Astrarium.Plugins.JupiterMoons.ImportExport;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Astrarium.Plugins.JupiterMoons
{
    public class JupiterMoonsVM : ViewModelBase
    {
        private readonly ISky sky;
        private readonly ISkyMap map;
        private readonly JupiterMoonsCalculator calculator = null;
        private readonly CelestialObject jupiter = null;
        private readonly CelestialObject[] moons = new CelestialObject[4];
        private Date selectedDate;
        private Date chartDate;
        private ICollection<JovianEvent> events = null;
        private ICollection<GRSEvent> grsEvents = null;

        #region Formatters

        private readonly IEphemFormatter MonthYearFormatter = Formatters.MonthYear;
        private readonly IEphemFormatter DateFormatter = Formatters.Date;
        private readonly IEphemFormatter TimeFormatter = Formatters.Time;
        private readonly IEphemFormatter DurationFormatter = new Formatters.TimeFormatter(withSeconds: true);
        private readonly IEphemFormatter AltitudeFormatter = new Formatters.SignedDoubleFormatter(1, "\u00B0");

        #endregion Formatters

        #region Commands

        public ICommand ChangeMonthCommand => new Command(ChangeMonth);
        public ICommand PrevMonthCommand => new Command(PrevMonth);
        public ICommand NextMonthCommand => new Command(NextMonth);
        public ICommand ShowEventBeginCommand => new Command<EventsTableItem>(ShowEventBegin);
        public ICommand ShowEventEndCommand => new Command<EventsTableItem>(ShowEventEnd);
        public ICommand ExportJovianEventsCommand => new Command(ExportJovianEvents);
        public ICommand ShowGRSTransitCommand => new Command<GRSTableItem>(ShowGRSTransit);
        public ICommand ShowGRSAppearCommand => new Command<GRSTableItem>(ShowGRSAppear);
        public ICommand ShowGRSDisappearCommand => new Command<GRSTableItem>(ShowGRSDisappear);
        public ICommand ExportGRSEventsCommand => new Command(ExportGRSEvents);
        public ICommand SaveChartBitmapCommand => new Command<IBitmapWriter>(SaveChartBitmap);
        public ICommand ChartShowDateCommand => new Command(ChartShowDate);

        #endregion Commands

        #region Bindable properties

        public string SelectedMonth
        {
            get => GetValue(nameof(SelectedMonth), "");
            set => SetValue(nameof(SelectedMonth), value);
        }

        public int DaysCount
        {
            get => GetValue(nameof(DaysCount), 0);
            set => SetValue(nameof(DaysCount), value);
        }

        public bool IsCalculating
        {
            get => GetValue(nameof(IsCalculating), false);
            set => SetValue(nameof(IsCalculating), value);
        }

        public bool FilterBodyIo
        {
            get => GetValue(nameof(FilterBodyIo), true);
            set { SetValue(nameof(FilterBodyIo), value); ApplyFilter(); }
        }

        public bool FilterBodyEuropa
        {
            get => GetValue(nameof(FilterBodyEuropa), true);
            set { SetValue(nameof(FilterBodyEuropa), value); ApplyFilter(); }
        }

        public bool FilterBodyGanymede
        {
            get => GetValue(nameof(FilterBodyGanymede), true);
            set { SetValue(nameof(FilterBodyGanymede), value); ApplyFilter(); }
        }

        public bool FilterBodyCallisto
        {
            get => GetValue(nameof(FilterBodyCallisto), true);
            set { SetValue(nameof(FilterBodyCallisto), value); ApplyFilter(); }
        }

        public bool FilterTransits
        {
            get => GetValue(nameof(FilterTransits), true);
            set { SetValue(nameof(FilterTransits), value); ApplyFilter(); }
        }

        public bool FilterShadowTransits
        {
            get => GetValue(nameof(FilterShadowTransits), true);
            set { SetValue(nameof(FilterShadowTransits), value); ApplyFilter(); }
        }

        public bool FilterEclipses
        {
            get => GetValue(nameof(FilterEclipses), true);
            set { SetValue(nameof(FilterEclipses), value); ApplyFilter(); }
        }

        public bool FilterOccultations
        {
            get => GetValue(nameof(FilterOccultations), true);
            set { SetValue(nameof(FilterOccultations), value); ApplyFilter(); }
        }

        public bool FilterMutualEclipses
        {
            get => GetValue(nameof(FilterMutualEclipses), true);
            set { SetValue(nameof(FilterMutualEclipses), value); ApplyFilter(); }
        }

        public bool FilterMutualOccultations
        {
            get => GetValue(nameof(FilterMutualOccultations), true);
            set { SetValue(nameof(FilterMutualOccultations), value); ApplyFilter(); }
        }

        public bool FilterJupiterAboveHorizon
        {
            get => GetValue(nameof(FilterJupiterAboveHorizon), false);
            set { SetValue(nameof(FilterJupiterAboveHorizon), value); ApplyFilter(); }
        }

        public bool FilterSunBelowHorizon
        {
            get => GetValue(nameof(FilterSunBelowHorizon), false);
            set { SetValue(nameof(FilterSunBelowHorizon), value); ApplyFilter(); }
        }

        public CrdsRectangular[,] CurrentPositions
        {
            get => GetValue<CrdsRectangular[,]>(nameof(CurrentPositions));
            set => SetValue(nameof(CurrentPositions), value);
        }

        public bool ChartExportAddHeader
        {
            get => GetValue(nameof(ChartExportAddHeader), true);
            set => SetValue(nameof(ChartExportAddHeader), value);
        }

        public bool ChartExportAddLegend
        {
            get => GetValue(nameof(ChartExportAddLegend), true);
            set => SetValue(nameof(ChartExportAddLegend), value);
        }

        /// <summary>
        /// Flag indicating dark mode is used
        /// </summary>
        public bool IsDarkMode
        {
            get => GetValue<bool>(nameof(IsDarkMode));
            protected set => SetValue(nameof(IsDarkMode), value);
        }

        #endregion Bindable properties

        public JupiterMoonsVM(ISky sky, ISkyMap map, ISettings settings)
        {
            this.sky = sky;
            this.map = map;
            this.calculator = new JupiterMoonsCalculator(settings);
            settings.SettingValueChanged += Settings_SettingValueChanged;

            jupiter = sky.Search("Planet", "Jupiter");

            string[] moonNames = new string[] { "Io", "Europa", "Ganymede", "Callisto" };
            for (int i = 0; i < moonNames.Length; i++)
            {
                moons[i] = sky.Search("PlanetMoon", moonNames[i]);
            }

            Date now = new Date(sky.Context.JulianDay, sky.Context.GeoLocation.UtcOffset);
            selectedDate = new Date(now.Year, now.Month, 1, now.UtcOffset);
            IsDarkMode = settings.Get("NightMode");

            Calculate();
        }

        private void Settings_SettingValueChanged(string settingName, object value)
        {
            if (settingName == "Schema")
            {
                IsDarkMode = (bool)value;
            }
        }

        private async void Calculate()
        {
            IsCalculating = true;
            SelectedMonth = MonthYearFormatter.Format(selectedDate);
            DaysCount = Date.DaysInMonth(selectedDate.Year, selectedDate.Month);       
            
            await calculator.SetDate(selectedDate, sky.Context.GeoLocation);
            events = await calculator.GetEvents();
            grsEvents = await calculator.GetGRSTimes();
            MoonsPositions = await calculator.GetPositions();

            ApplyFilter();

            IsCalculating = false;
        }

        private bool IsMatchFilter(JovianEvent e, bool filter, string code)
        {
            return !filter && Regex.IsMatch(e.Code, code) ? false : true;
        }

        private async void ApplyFilter()
        {
            await Task.Run(() =>
            {
                IsCalculating = true;

                // Moons events table
                {
                    var items = new List<EventsTableItem>();
                    foreach (var e in events.OrderBy(e => e.JdBegin)
                        .Where(e => IsMatchFilter(e, FilterBodyIo, "^.+1$"))
                        .Where(e => IsMatchFilter(e, FilterBodyEuropa, "^.+2$"))
                        .Where(e => IsMatchFilter(e, FilterBodyGanymede, "^.+3$"))
                        .Where(e => IsMatchFilter(e, FilterBodyCallisto, "^.+4$"))
                        .Where(e => IsMatchFilter(e, FilterTransits, "^T\\d$"))
                        .Where(e => IsMatchFilter(e, FilterShadowTransits, "^S\\d$"))
                        .Where(e => IsMatchFilter(e, FilterEclipses, "^E\\d$"))
                        .Where(e => IsMatchFilter(e, FilterOccultations, "^O\\d$"))
                        .Where(e => IsMatchFilter(e, FilterMutualEclipses, "^\\dE\\d$"))
                        .Where(e => IsMatchFilter(e, FilterMutualOccultations, "^\\dO\\d$"))
                        .Where(e => FilterJupiterAboveHorizon ? e.JupiterAltBegin > 0 || e.JupiterAltEnd > 0 : true)
                        .Where(e => FilterSunBelowHorizon ? e.SunAltBegin < 0 || e.SunAltEnd < 0 : true))
                    {
                        string notes = null;

                        if (e.IsEclipsedAtBegin)
                            notes = Text.Get("JovianEvent.EclipsedAtBegin");
                        if (e.IsOccultedAtBegin)
                            notes = Text.Get("JovianEvent.OccultedAtBegin");
                        if (e.IsEclipsedAtEnd)
                            notes = Text.Get("JovianEvent.EclipsedAtEnd");
                        if (e.IsOccultedAtEnd)
                            notes = Text.Get("JovianEvent.OccultedAtEnd");

                        var dateBegin = new Date(e.JdBegin, sky.Context.GeoLocation.UtcOffset);
                        var dateEnd = new Date(e.JdEnd, sky.Context.GeoLocation.UtcOffset);

                        items.Add(new EventsTableItem()
                        {
                            Event = e,
                            BeginDate = DateFormatter.Format(dateBegin),
                            BeginTime = TimeFormatter.Format(dateBegin),
                            EndTime = TimeFormatter.Format(dateEnd),
                            Duration = DurationFormatter.Format(e.JdEnd - e.JdBegin),
                            JupiterAltBegin = AltitudeFormatter.Format(e.JupiterAltBegin),
                            JupiterAltEnd = AltitudeFormatter.Format(e.JupiterAltEnd),
                            SunAltBegin = AltitudeFormatter.Format(e.SunAltBegin),
                            SunAltEnd = AltitudeFormatter.Format(e.SunAltEnd),
                            Code = e.Code,
                            Text = e.Text,
                            Notes = notes
                        });
                    }

                    EventsTable = items.ToArray();
                }

                // GRS table
                {
                    var items = new List<GRSTableItem>();

                    foreach (var e in grsEvents
                        .Where(e => FilterJupiterAboveHorizon ? e.JupiterAltTransit > 0 : true)
                        .Where(e => FilterSunBelowHorizon ? e.SunAltTransit < 0 : true))
                    {
                        var date = new Date(e.JdTransit, sky.Context.GeoLocation.UtcOffset);
                        var appear = new Date(e.JdAppear, sky.Context.GeoLocation.UtcOffset);
                        var disappear = new Date(e.JdDisappear, sky.Context.GeoLocation.UtcOffset);

                        items.Add(new GRSTableItem()
                        {
                            Event = e,
                            Date = DateFormatter.Format(date),
                            TransitTime = TimeFormatter.Format(date),
                            AppearTime = TimeFormatter.Format(appear),
                            DisappearTime = TimeFormatter.Format(disappear),
                            SunAltTransit = AltitudeFormatter.Format(e.SunAltTransit),
                            JupiterAltTransit = AltitudeFormatter.Format(e.JupiterAltTransit),
                            SunAltAppear = AltitudeFormatter.Format(e.SunAltAppear),
                            JupiterAltAppear = AltitudeFormatter.Format(e.JupiterAltAppear),
                            SunAltDisappear = AltitudeFormatter.Format(e.SunAltDisappear),
                            JupiterAltDisappear = AltitudeFormatter.Format(e.JupiterAltDisappear),
                        });
                    }

                    GRSTable = items.ToArray();
                }

                IsCalculating = false;
            });
        }

        private void ChangeMonth()
        {
            double? jd = ViewManager.ShowDateDialog(selectedDate.ToJulianEphemerisDay(), selectedDate.UtcOffset, DateOptions.MonthYear);
            if (jd != null)
            {
                var date = new Date(jd.Value, selectedDate.UtcOffset);
                selectedDate = new Date(date.Year, date.Month, 1, selectedDate.UtcOffset);
                Calculate();
            }
        }

        private void PrevMonth()
        {
            int month = selectedDate.Month - 1;
            int year = selectedDate.Year;
            if (month < 1)
            {
                month = 12;
                year--;
            }
            selectedDate = new Date(year, month, 1, selectedDate.UtcOffset);
            Calculate();
        }

        private void NextMonth()
        {
            int month = selectedDate.Month + 1;
            int year = selectedDate.Year;
            if (month > 12)
            {
                month = 1;
                year++;
            }
            selectedDate = new Date(year, month, 1, selectedDate.UtcOffset);
            Calculate();
        }

        private void ExportJovianEvents()
        {
            var formats = new Dictionary<string, string>
            {
                [Text.Get("JupiterMoonsView.OutputFormat.CsvWithFormatting")] = "*.csv",
                [Text.Get("JupiterMoonsView.OutputFormat.CsvRawData")] = "*.csv",
            };
            string filter = string.Join("|", formats.Select(kv => $"{kv.Key}|{kv.Value}"));
            var file = ViewManager.ShowSaveFileDialog("$JupiterMoonsView.ExportTitle", "JovianEvents", ".csv", filter, out int selectedFilterIndex);
            if (file != null)
            {
                bool rawData = selectedFilterIndex == 2;
                var writer = new JovianEventsCsvWriter(rawData);
                writer.Write(file, EventsTable);

                var answer = ViewManager.ShowMessageBox("$JupiterMoonsView.ExportDone.Title", "$JupiterMoonsView.ExportDone.Message", System.Windows.MessageBoxButton.YesNo);
                if (answer == System.Windows.MessageBoxResult.Yes)
                {
                    Process.Start(file);
                }
            }
        }

        private void ExportGRSEvents()
        {
            var formats = new Dictionary<string, string>
            {
                [Text.Get("JupiterMoonsView.OutputFormat.CsvWithFormatting")] = "*.csv",
                [Text.Get("JupiterMoonsView.OutputFormat.CsvRawData")] = "*.csv",
            };
            string filter = string.Join("|", formats.Select(kv => $"{kv.Key}|{kv.Value}"));
            var file = ViewManager.ShowSaveFileDialog("$JupiterMoonsView.ExportTitle", "GreatRedSpot", ".csv", filter, out int selectedFilterIndex);
            if (file != null)
            {
                bool rawData = selectedFilterIndex == 2;
                var writer = new GRSEventsCsvWriter(rawData);
                writer.Write(file, GRSTable);

                var answer = ViewManager.ShowMessageBox("$JupiterMoonsView.ExportDone.Title", "$JupiterMoonsView.ExportDone.Message", System.Windows.MessageBoxButton.YesNo);
                if (answer == System.Windows.MessageBoxResult.Yes)
                {
                    Process.Start(file);
                }
            }
        }

        private void ShowEventBegin(EventsTableItem e)
        {
            ShowEvent(e.Event, e.Event.JdBegin);
        }

        private void ShowEventEnd(EventsTableItem e)
        {
            ShowEvent(e.Event, e.Event.JdEnd);
        }

        private void ShowEvent(JovianEvent e, double jd)
        {
            var body = e.MoonNumber > 0 ? moons[e.MoonNumber - 1] : jupiter;
            if (body != null)
            {
                sky.SetDate(jd);
                map.GoToObject(body, TimeSpan.Zero);
            }
        }

        private void ShowGRSAppear(GRSTableItem e)
        {
            sky.SetDate(e.Event.JdAppear);
            map.GoToObject(jupiter, TimeSpan.Zero);
        }

        private void ShowGRSDisappear(GRSTableItem e)
        {
            sky.SetDate(e.Event.JdDisappear);
            map.GoToObject(jupiter, TimeSpan.Zero);
        }

        private void ShowGRSTransit(GRSTableItem e)
        {
            sky.SetDate(e.Event.JdTransit);
            map.GoToObject(jupiter, TimeSpan.Zero);
        }

        private void ChartShowDate()
        {
            if (chartDate != null)
            {
                sky.SetDate(chartDate.ToJulianEphemerisDay());
                map.GoToObject(jupiter, TimeSpan.Zero);
            }
        }

        private void SaveChartBitmap(IBitmapWriter bitmapWriter)
        {
            var filters = new Dictionary<string, string>
            {
                ["PNG (*.png)"] = "*.png",
                ["BMP (*.bmp)"] = "*.bmp",
                ["GIF (*.gif)"] = "*.gif",
                ["TIFF (*.tiff)"] = "*.tiff"
            };
            var formats = new ImageFormat[] 
            {
                ImageFormat.Png,
                ImageFormat.Bmp,
                ImageFormat.Gif,
                ImageFormat.Tiff
            };

            string filter = string.Join("|", filters.Select(kv => $"{kv.Key}|{kv.Value}"));
            var file = ViewManager.ShowSaveFileDialog("$JupiterMoonsView.SaveImageTitle", $"JupiterMoons-{SelectedMonth.Replace(" ", "-")}", ".png", filter, out int selectedFilterIndex);
            if (file != null)
            {
                var options = new WriteToBitmapOptions() 
                { 
                    IsWriteToFile = true,
                    Header = ChartExportAddHeader, 
                    Legend = ChartExportAddLegend 
                };
                var bmp = bitmapWriter.WriteToBitmap(options);
                bmp.Save(file, formats[selectedFilterIndex - 1]);

                var answer = ViewManager.ShowMessageBox("$JupiterMoonsView.SaveImageDone.Title", "$JupiterMoonsView.SaveImageDone.Message", System.Windows.MessageBoxButton.YesNo);
                if (answer == System.Windows.MessageBoxResult.Yes)
                {
                    Process.Start(file);
                }
            }            
        }

        public int CurrentChartPosition
        {
            get => GetValue(nameof(CurrentChartPosition), -1);
            set 
            { 
                SetValue(nameof(CurrentChartPosition), value);

                chartDate = value != -1 ? new Date(selectedDate.Year, selectedDate.Month, 1 + value / 24.0 + TimeSpan.FromSeconds(1).TotalDays, utcOffset : sky.Context.GeoLocation.UtcOffset) : null;
                CurrentDateTime = value != -1 ? Formatters.DateTime.Format(chartDate) : null;
            }
        }

        public string CurrentDateTime
        {
            get => GetValue<string>(nameof(CurrentDateTime));
            set => SetValue(nameof(CurrentDateTime), value);
        }

        public ICollection<CrdsRectangular[,]> MoonsPositions
        {
            get => GetValue<ICollection<CrdsRectangular[,]>>(nameof(MoonsPositions));
            set => SetValue(nameof(MoonsPositions), value);
        }

        public EventsTableItem[] EventsTable
        {
            get => GetValue(nameof(EventsTable), new EventsTableItem[0]);
            set => SetValue(nameof(EventsTable), value);
        }

        public GRSTableItem[] GRSTable
        {
            get => GetValue(nameof(GRSTable), new GRSTableItem[0]);
            set => SetValue(nameof(GRSTable), value);
        }
    }

}
