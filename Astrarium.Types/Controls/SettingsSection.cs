using System.Windows;
using System.Windows.Controls;

namespace Astrarium.Types.Controls
{
    public class SettingsSection : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(SettingsSection), new UIPropertyMetadata(null));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
    }
}
