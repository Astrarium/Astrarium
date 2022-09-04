using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ASCOM
{
    public class AxisRates
    {
        public double MinPrimary { get; private set; }
        public double MaxPrimary { get; private set; }
        public double MinSecondary { get; private set; }
        public double MaxSecondary { get; private set; }

        private List<double> speedsPrimary;
        private List<double> speedsSecondary;

        private int currentPrimaryIndex = 0;
        private int currentSecondaryIndex = 0;

        public double Primary
        {
            get => speedsPrimary[currentPrimaryIndex];
        }

        public double Secondary
        {
            get => speedsPrimary[currentSecondaryIndex];
        }

        public AxisRates(double minPrimary, double maxPrimary, double minSecondary, double maxSecondary)
        {
            MinPrimary = minPrimary;
            MaxPrimary = maxPrimary;
            MinSecondary = minSecondary;
            MaxSecondary = maxSecondary;

            speedsPrimary = GetSpeeds(minPrimary, maxPrimary);
            speedsSecondary = GetSpeeds(minSecondary, maxSecondary);

            currentPrimaryIndex = speedsPrimary.Count / 2;
            currentSecondaryIndex = speedsSecondary.Count / 2;
        }

        private List<double> GetSpeeds(double min, double max)
        {
            List<double> speeds = new List<double>();
            double step = max - min;
            do
            {
                speeds.Add(step);
                step /= 2;
            }
            while (step > 4.0 / 3600.0);
            speeds.Reverse();
            return speeds;
        }

        public void Increase()
        {
            int primaryIndex = currentPrimaryIndex + 1;
            if (primaryIndex < speedsPrimary.Count)
            {
                currentPrimaryIndex = primaryIndex;
            }

            int secondaryIndex = currentSecondaryIndex + 1;
            if (secondaryIndex < speedsSecondary.Count)
            {
                currentSecondaryIndex = secondaryIndex;
            }
        }

        public void Decrease()
        {
            int primaryIndex = currentPrimaryIndex - 1;
            if (primaryIndex > 0)
            {
                currentPrimaryIndex = primaryIndex;
            }

            int secondaryIndex = currentSecondaryIndex - 1;
            if (secondaryIndex > 0)
            {
                currentSecondaryIndex = secondaryIndex;
            }
        }

        public void SetMax()
        {
            currentPrimaryIndex = speedsPrimary.Count - 1;
            currentSecondaryIndex = speedsSecondary.Count - 1;
        }

        public void SetMin()
        {
            currentPrimaryIndex = 0;
            currentSecondaryIndex = 0;
        }
    }
}
