using System.Windows;

namespace Astrarium.Types.Controls
{
    public class SettingsSection : DisposableUserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(SettingsSection), new UIPropertyMetadata(null));
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(string), typeof(SettingsSection), new UIPropertyMetadata(null));

        /// <summary>
        /// Application settings section title.
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Application settings section icon. It's a SVG path.
        /// </summary>
        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
    }
}
