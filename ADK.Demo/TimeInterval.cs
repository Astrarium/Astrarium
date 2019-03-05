using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    //public enum TimeIntervalUnit
    //{
    //    Second  = 0,
    //    Minute  = 1,
    //    Hour    = 2,
    //    Day     = 3
    //}

    //public class TimeInterval
    //{
    //    public TimeIntervalUnit IntervalUnit { get; set; } = TimeIntervalUnit.Day;
    //    public double IntervalValue { get; set; } = 1;

    //    public double TotalDays
    //    {
    //        get
    //        {
    //            double interval = 1;
    //            switch (IntervalUnit)
    //            {
    //                case TimeIntervalUnit.Second:
    //                    interval = 1.0 / (24 * 3600);
    //                    break;
    //                case TimeIntervalUnit.Minute:
    //                    interval = 1.0 / (24 * 60);
    //                    break;
    //                case TimeIntervalUnit.Hour:
    //                    interval = 1.0 / 24;
    //                    break;
    //                default:
    //                case TimeIntervalUnit.Day:
    //                    interval = 1.0;
    //                    break;
    //            }
    //            return IntervalValue * interval;
    //        }
    //    }

    //    public override string ToString()
    //    {
    //        string unit = "";
    //        switch (IntervalUnit)
    //        {
    //            case TimeIntervalUnit.Second:
    //                unit = "s";
    //                break;
    //            case TimeIntervalUnit.Minute:
    //                unit = "m";
    //                break;
    //            case TimeIntervalUnit.Hour:
    //                unit = "h";
    //                break;
    //            default:
    //            case TimeIntervalUnit.Day:
    //                unit = "d";
    //                break;
    //        }

    //        return $"{IntervalValue} {unit}";
    //    }

    //    public TimeInterval(double intervalValue, TimeIntervalUnit intervalUnit)
    //    {
    //        IntervalValue = intervalValue;
    //        IntervalUnit = intervalUnit;
    //    }
    //}
}
