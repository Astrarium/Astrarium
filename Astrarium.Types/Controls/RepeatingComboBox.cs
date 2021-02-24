using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Astrarium.Types.Controls
{
    public class RepeatingComboBox : ComboBox
    {
        public bool Loop
        {
            get { return (bool)GetValue(LoopProperty); }
            set { SetValue(LoopProperty, value); }
        }

        public readonly static DependencyProperty LoopProperty = DependencyProperty.Register(
            "Loop", typeof(bool), typeof(RepeatingComboBox), new UIPropertyMetadata(false));

        public readonly static DependencyProperty LoopIncrementCommandProperty = DependencyProperty.Register(
            "LoopIncrementCommand", typeof(ICommand), typeof(RepeatingComboBox));

        public ICommand LoopIncrementCommand
        {
            get { return (ICommand)GetValue(LoopIncrementCommandProperty); }
            set { SetValue(LoopIncrementCommandProperty, value); }
        }

        public readonly static DependencyProperty LoopDecrementCommandProperty = DependencyProperty.Register(
            "LoopDecrementCommand", typeof(ICommand), typeof(RepeatingComboBox));

        public ICommand LoopDecrementCommand
        {
            get { return (ICommand)GetValue(LoopDecrementCommandProperty); }
            set { SetValue(LoopDecrementCommandProperty, value); }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (Loop)
            {
                if (SelectedIndex == 0 && e.Key == Key.Up)
                {
                    LoopDecrementCommand?.Execute(null);
                    e.Handled = true;
                    return;
                }

                if (SelectedIndex == Items.Count - 1 && e.Key == Key.Down)
                {
                    LoopIncrementCommand?.Execute(null);
                    e.Handled = true;
                    return;
                }
            }

            base.OnPreviewKeyDown(e);
        }
    }
}
