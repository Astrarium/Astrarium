using Astrarium.Algorithms;
using Astrarium.Plugins.SolarSystem.Controls;
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
    public class LunarCalendarVM : ViewModelBase
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

        public ICommand PrevDayCommand { get; private set; }
        public ICommand NextDayCommand { get; private set; }

        public ICommand ExportCommand { get; private set; }
        public ICommand PrintCommand { get; private set; }

        public DisplayOptions DisplayOptions { get; private set; }

        public Date SelectedDate
        {
            get => GetValue<Date>(nameof(SelectedDate));
            set
            {
                SetValue(nameof(SelectedDate), value);
                NotifyPropertyChanged(nameof(SelectedMonth));
            }
        }

        public LunarDay SelectedDay
        {
            get => GetValue<LunarDay>(nameof(SelectedDay));
            set
            {
                if (value != SelectedDay)
                {
                    SetValue(nameof(SelectedDay), value);
                }
                else
                {
                    SetValue(nameof(SelectedDay), null);
                }
                
                CalculateForDay();
            }
        }

        public LunarDay SelectedInstant
        {
            get => GetValue<LunarDay>(nameof(SelectedInstant));
            set
            {
                SetValue(nameof(SelectedInstant), value);
            }
        }

        public double SelectedTimeOfTheDay 
        {
            get => GetValue<double>(nameof(SelectedTimeOfTheDay));
            set
            {
                SetValue(nameof(SelectedTimeOfTheDay), value);
                CalculateForDay();
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

        public MoonDiskImageRotationMode RotationMode
        {
            get => GetValue(nameof(RotationMode), MoonDiskImageRotationMode.PA);
            set => SetValue(nameof(RotationMode), value);
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

        public Color ColorCalendarSelection
        {
            get => GetValue<Color>(nameof(ColorCalendarSelection));
            set => SetValue(nameof(ColorCalendarSelection), value);
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

        public CrdsGeographical GeoLocation => sky.Context.GeoLocation;

        public LunarCalendarVM(ISky sky, ISkyMap map, ISettings settings, LunarCalc moonCalc, SolarCalc sunCalc, LunarCalendar calendar)
        {
            this.sky = sky;
            this.map = map;
            this.settings = settings;
            this.moon = moonCalc.Moon;
            this.sun = sunCalc.Sun;
            this.lunarCalendar = calendar;

            ColorCalendarBackground = (Color)Application.Current.FindResource("ColorWindowBackground");

            DisplayOptions = new DisplayOptions();
            DisplayOptions.PropertyChanged += DisplayOptions_PropertyChanged;
            NightMode = settings.Get("NightMode");
            this.settings.SettingValueChanged += Settings_SettingValueChanged;
            Text.LocaleChanged += CalculateForMonth;

            selectedDate = Date.Now;
            
            ShowMoonRiseCommand = new Command<int>(ShowMoonRise);
            ShowMoonTransitCommand = new Command<int>(ShowMoonTransit);
            ShowMoonSetCommand = new Command<int>(ShowMoonSet);
            ShowSunRiseCommand = new Command<int>(ShowSunRise);
            ShowSunSetCommand = new Command<int>(ShowSunSet);
            ShowMoonCommand = new Command<Date>(ShowMoon);
            PrevMonthCommand = new Command(PrevMonth);
            NextMonthCommand = new Command(NextMonth);
            PrevDayCommand = new Command(PrevDay);
            NextDayCommand = new Command(NextDay);
            SelectDateCommand = new Command(SelectDate);
            ExportCommand = new Command<FrameworkElement>(Export);
            PrintCommand = new Command<FrameworkElement>(Print);

            CalculateForMonth();
        }

        private void DisplayOptions_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DisplayOptions.PrintMode))
            {
                LoadColors();
            }
        }

        private void LoadColors()
        {
            if (DisplayOptions.PrintMode)
            {
                ColorTextForeground = Colors.Black;
                ColorCalendarForeground = Colors.Gray;
                ColorCalendarBackground = Colors.White;
                ColorCalendarSelection = Colors.LightGray;
                ColorIcon = Colors.Gray;
                ColorCalendarBorder = Colors.LightGray;
                ColorLinkForeground = Colors.Black;
            }
            else
            {
                ColorTextForeground = (Color)Application.Current.FindResource("ColorForeground");
                ColorCalendarForeground = (Color)Application.Current.FindResource("ColorControlLightBackground");
                ColorCalendarBackground = (Color)Application.Current.FindResource("ColorWindowBackground");
                ColorCalendarSelection = (Color)Application.Current.FindResource("ColorControlBackground");
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
            selectedDate = new Date(year, month, 1, GeoLocation.UtcOffset);
            CalculateForMonth();
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
            selectedDate = new Date(year, month, 1, GeoLocation.UtcOffset);
            CalculateForMonth();
        }
        
        private void PrevDay()
        {
            var days = Days.ToList();
            int index = days.IndexOf(SelectedDay);
            if (index > 0)
            {
                SelectedDay = days.ElementAt(index - 1);
            }
            else
            {
                PrevMonth();
            }  
        }

        private void NextDay()
        {
            var days = Days.ToList();
            int index = days.IndexOf(SelectedDay);
            if (index < Days.Count - 1)
            {
                SelectedDay = days.ElementAt(index + 1);
            }
            else
            {
                NextMonth();
            }
        }

        private void SelectDate()
        {
            double? jd = ViewManager.ShowDateDialog(selectedDate.ToJulianEphemerisDay(), GeoLocation.UtcOffset, DateOptions.MonthYear);
            if (jd != null)
            {
                selectedDate = new Date(jd.Value, GeoLocation.UtcOffset);
                CalculateForMonth();
            }
        }

        private async void CalculateForMonth()
        {
            int needSelectDay = 0;
            if (SelectedDay != null)
            {
                if (SelectedDay.DayOfMonth == 1) needSelectDay = -1;
                if (SelectedDay.DayOfMonth == Date.DaysInMonth(SelectedDate.Year, SelectedDate.Month)) needSelectDay = 1;
            }

            SelectedDay = null;
            SetValue(nameof(SelectedDate), selectedDate);
            NotifyPropertyChanged(nameof(SelectedMonth));

            IsCalculating = true;
            Days = await lunarCalendar.Calculate(new Date(selectedDate.Year, selectedDate.Month, 1, GeoLocation.UtcOffset), GeoLocation);
            
            if (needSelectDay == 1)
            {
                SelectedDay = Days.First();
            } 
            else if (needSelectDay == -1) 
            {
                SelectedDay = Days.Last();
            }
            
            IsCalculating = false;
        }

        private async void CalculateForDay()
        {
            if (SelectedDay != null)
            {
                var jdMidnight = SelectedDay.JdMidnight;
                var timeOfDay = SelectedTimeOfTheDay;

                SelectedInstant = await lunarCalendar.Calculate(jdMidnight, timeOfDay, GeoLocation);
            }
            else
            {
                SelectedInstant = null;
            }
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

        private void ShowMoon(Date date)
        {
            ShowMoon(date.ToJulianEphemerisDay());
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
            string path = ViewManager.ShowSaveFileDialog("$LunarCalendarWindow.SaveAsImage", SelectedMonth, ".png", "PNG image|*.png", out int _);

            if (!string.IsNullOrEmpty(path))
            {
                var bitmap = new RenderTargetBitmap(
                    (int)(calendarControl.ActualWidth + calendarControl.Margin.Left + calendarControl.Margin.Right) * 2, 
                    (int)(calendarControl.ActualHeight + calendarControl.Margin.Top + calendarControl.Margin.Bottom) * 2, 
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

    public class DisplayOptions : PropertyChangedBase
    {
        public bool PrintMode
        {
            get => GetValue<bool>(nameof(PrintMode));
            set => SetValue(nameof(PrintMode), value);
        }

        public bool MonthHeader
        {
            get => GetValue(nameof(MonthHeader), false);
            set => SetValue(nameof(MonthHeader), value);
        }

        public bool SunInfo
        {
            get => GetValue(nameof(SunInfo), true);
            set => SetValue(nameof(SunInfo), value);
        }

        public bool SunRise
        {
            get => GetValue(nameof(SunRise), true);
            set => SetValue(nameof(SunRise), value);
        }

        public bool DayLength
        {
            get => GetValue(nameof(DayLength), true);
            set => SetValue(nameof(DayLength), value);
        }

        public bool SunSet
        {
            get => GetValue(nameof(SunSet), true);
            set => SetValue(nameof(SunSet), value);
        }

        public bool Icons
        {
            get => GetValue(nameof(Icons), true);
            set => SetValue(nameof(Icons), value);
        }

        public bool MoonInfo
        {
            get => GetValue(nameof(MoonInfo), true);
            set => SetValue(nameof(MoonInfo), value);
        }

        public bool MoonRise
        {
            get => GetValue(nameof(MoonRise), true);
            set => SetValue(nameof(MoonRise), value);
        }

        public bool MoonTransit
        {
            get => GetValue(nameof(MoonTransit), true);
            set => SetValue(nameof(MoonTransit), value);
        }

        public bool MoonSet
        {
            get => GetValue(nameof(MoonSet), true);
            set => SetValue(nameof(MoonSet), value);
        }

        public bool MoonImages
        {
            get => GetValue(nameof(MoonImages), true);
            set => SetValue(nameof(MoonImages), value);
        }
    }
}
