using System.Windows;
using System.Windows.Media;

namespace Astrarium.Plugins.JupiterMoons.ImportExport
{
    public interface IBitmapWriter
    {
        System.Drawing.Bitmap WriteToBitmap(WriteToBitmapOptions options);
    }

    public class WriteToBitmapOptions
    {
        public bool IsWriteToFile { get; set; }
        public bool Header { get; set; }
        public bool Legend { get; set; }
        public SolidColorBrush Background { get; set; } = Brushes.Transparent;
        public Size Size { get; set; }
        public int HorizontalScale { get; set; } = 1;
        public int VerticalScale { get; set; } = 1;
    }
}
