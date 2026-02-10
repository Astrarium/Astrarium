using Astrarium.Algorithms;
using Astrarium.Plugins.SolarSystem.Objects;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Astrarium.Plugins.SolarSystem.ViewModels
{
    public class LunarCalendarViewModel : ViewModelBase
    {
        private readonly ISky sky;
        private readonly ISkyMap map;
        private readonly ISettings settings;
        private readonly Moon moon;
        private readonly Sun sun;
        private readonly LunarCalendar lunarCalendar;

        private Date selectedDate;

        public ICommand ShowMoonRiseCommand { get; private set; }
        public ICommand ShowMoonTransitCommand { get; private set; }
        public ICommand ShowMoonSetCommand { get; private set; }
        public ICommand ShowMoonCommand { get; private set; }
        public ICommand ShowSunRiseCommand { get; private set; }
        public ICommand ShowSunSetCommand { get; private set; }

        public ICommand PrevMonthCommand { get; private set; }
        public ICommand NextMonthCommand { get; private set; }
        public ICommand SelectDateCommand { get; private set; }

        public ICommand ExportCommand { get; private set; }
        public ICommand PrintCommand { get; private set; }

        public Date SelectedDate
        {
            get => GetValue<Date>(nameof(SelectedDate));
            set
            {
                SetValue(nameof(SelectedDate), value);
                NotifyPropertyChanged(nameof(SelectedMonth));
            }
        }

        public string SelectedMonth
        {
            get => Formatters.MonthYear.Format(selectedDate);
        }

        public bool IsCalculating
        {
            get => GetValue<bool>(nameof(IsCalculating));
            set => SetValue(nameof(IsCalculating), value);
        }

        public IReadOnlyCollection<LunarDay> Days
        {
            get => GetValue<IReadOnlyCollection<LunarDay>>(nameof(Days));
            set => SetValue(nameof(Days), value);
        }

        public bool NightMode
        {
            get => GetValue<bool>(nameof(NightMode));
            set
            {
                SetValue(nameof(NightMode), value);
                LoadColors();
            }
        }

        public bool PrintMode
        {
            get => GetValue<bool>(nameof(PrintMode));
            set 
            {
                SetValue(nameof(PrintMode), value);
                LoadColors();
            } 
        }

        public bool RotateAxis
        {
            get => GetValue(nameof(RotateAxis), true);
            set => SetValue(nameof(RotateAxis), value);
        }

        public bool NorthTop
        {
            get => GetValue(nameof(NorthTop), true);
            set => SetValue(nameof(NorthTop), value);
        }

        public Color ColorTextForeground
        {
            get => GetValue<Color>(nameof(ColorTextForeground));
            set => SetValue(nameof(ColorTextForeground), value);
        }

        public Color ColorCalendarForeground
        {
            get => GetValue<Color>(nameof(ColorCalendarForeground));
            set => SetValue(nameof(ColorCalendarForeground), value);
        }

        public Color ColorCalendarBackground
        {
            get => GetValue<Color>(nameof(ColorCalendarBackground));
            set => SetValue(nameof(ColorCalendarBackground), value);
        }

        public Color ColorIcon
        {
            get => GetValue<Color>(nameof(ColorIcon));
            set => SetValue(nameof(ColorIcon), value);
        }

        public Color ColorLinkForeground
        {
            get => GetValue<Color>(nameof(ColorLinkForeground));
            set => SetValue(nameof(ColorLinkForeground), value);
        }

        public Color ColorCalendarBorder
        {
            get => GetValue<Color>(nameof(ColorCalendarBorder));
            set => SetValue(nameof(ColorCalendarBorder), value);
        }

        public LunarCalendarViewModel(ISky sky, ISkyMap map, ISettings settings, LunarCalc moonCalc, SolarCalc sunCalc, LunarCalendar calendar)
        {
            this.sky = sky;
            this.map = map;
            this.settings = settings;
            this.moon = moonCalc.Moon;
            this.sun = sunCalc.Sun;
            this.lunarCalendar = calendar;

            ColorCalendarBackground = (Color)Application.Current.FindResource("ColorWindowBackground");

            NightMode = settings.Get("NightMode");
            this.settings.SettingValueChanged += Settings_SettingValueChanged;
            Text.LocaleChanged += Calculate;

            selectedDate = Date.Now;
            
            ShowMoonRiseCommand = new Command<int>(ShowMoonRise);
            ShowMoonTransitCommand = new Command<int>(ShowMoonTransit);
            ShowMoonSetCommand = new Command<int>(ShowMoonSet);
            ShowSunRiseCommand = new Command<int>(ShowSunRise);
            ShowSunSetCommand = new Command<int>(ShowSunSet);
            ShowMoonCommand = new Command<double>(ShowMoon);
            PrevMonthCommand = new Command(PrevMonth);
            NextMonthCommand = new Command(NextMonth);
            SelectDateCommand = new Command(SelectDate);
            ExportCommand = new Command<FrameworkElement>(Export);
            PrintCommand = new Command<FrameworkElement>(Print);

            Calculate();
        }

        private void LoadColors()
        {
            if (PrintMode)
            {
                ColorTextForeground = Colors.Black;
                ColorCalendarForeground = Colors.Gray;
                ColorCalendarBackground = Colors.White;
                ColorIcon = Colors.Gray;
                ColorCalendarBorder = Colors.LightGray;
                ColorLinkForeground = Colors.Black;
            }
            else
            {
                ColorTextForeground = (Color)Application.Current.FindResource("ColorForeground");
                ColorCalendarForeground = (Color)Application.Current.FindResource("ColorControlLightBackground");
                ColorCalendarBackground = (Color)Application.Current.FindResource("ColorWindowBackground");
                ColorIcon = (Color)Application.Current.FindResource("ColorControlLightBackground");
                ColorCalendarBorder = (Color)Application.Current.FindResource("ColorControlBackground");
                ColorLinkForeground = (Color)Application.Current.FindResource("ColorHighlight");
            }
        }

        private void Settings_SettingValueChanged(string settingName, object value)
        {
            if (settingName == "NightMode")
            {
                NightMode = settings.Get("NightMode");
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
            selectedDate = new Date(year, month, 1, sky.Context.GeoLocation.UtcOffset);
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
            selectedDate = new Date(year, month, 1, sky.Context.GeoLocation.UtcOffset);
            Calculate();
        }

        private void SelectDate()
        {
            double? jd = ViewManager.ShowDateDialog(selectedDate.ToJulianEphemerisDay(), sky.Context.GeoLocation.UtcOffset, DateOptions.MonthYear);
            if (jd != null)
            {
                selectedDate = new Date(jd.Value, sky.Context.GeoLocation.UtcOffset);
                Calculate();
            }
        }

        private async void Calculate()
        {
            SelectedDate = selectedDate;

            IsCalculating = true;
            Days = await lunarCalendar.Calculate(new Date(selectedDate.Year, selectedDate.Month, 1, sky.Context.GeoLocation.UtcOffset), sky.Context.GeoLocation);
            IsCalculating = false;
        }

        private void ShowMoonRise(int d)
        {
            var day = Days.ElementAt(d - 1);
            ShowMoon(day.JdMidnight + day.Moon.Rise);
        }

        private void ShowMoonTransit(int d)
        {
            var day = Days.ElementAt(d - 1);
            ShowMoon(day.JdMidnight + day.Moon.Transit);
        }

        private void ShowMoonSet(int d)
        {
            var day = Days.ElementAt(d - 1);
            ShowMoon(day.JdMidnight + day.Moon.Set);
        }

        private void ShowSunRise(int d)
        {
            var day = Days.ElementAt(d - 1);
            ShowSun(day.JdMidnight + day.Sun.Rise);
        }

        private void ShowSunSet(int d)
        {
            var day = Days.ElementAt(d - 1);
            ShowSun(day.JdMidnight + day.Sun.Set);
        }

        private async void ShowMoon(double jd)
        {
            await Task.Run(() =>
            {
                sky.SetDate(jd);
                map.GoToObject(moon, TimeSpan.Zero, 1);
            });
        }

        private async void ShowSun(double jd)
        {
            await Task.Run(() =>
            {
                sky.SetDate(jd);
                map.GoToObject(sun, TimeSpan.Zero, 1);
            });
        }

        private void Export(FrameworkElement calendarControl)
        {
            string path = ViewManager.ShowSaveFileDialog("Save as image...", SelectedMonth, ".png", "PNG image|*.png", out int _);
        
            if (!string.IsNullOrEmpty(path))
            {
                

                var bitmap = new RenderTargetBitmap(
                    (int)calendarControl.ActualWidth * 2, 
                    (int)calendarControl.ActualHeight * 2, 
                    96 * 2, 
                    96 * 2, PixelFormats.Default);

                bitmap.Render(calendarControl);

                LoadColors();

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                using (Stream stream = File.Create(path))
                {
                    encoder.Save(stream);
                }
            }
        }

        private void Print(FrameworkElement calendarControl)
        {
            ViewManager.ShowPrintDialog(calendarControl, SelectedMonth);
        }
    }
}
