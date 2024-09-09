using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Astrarium.Views
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        public ProgressWindow()
        {
            InitializeComponent();
        }

        public string Text
        {
            set
            {
                Label.Content = value;
            }
            get
            {
                return Label.Content.ToString();
            }
        }

        private Progress<double> _Progress;
        public Progress<double> Progress
        {
            get { return _Progress; }
            set
            {
                _Progress = value;

                ProgressBar.Value = 0;
                ProgressBar.IsIndeterminate = _Progress == null;

                if (_Progress != null)
                {
                    _Progress.ProgressChanged += _Progress_ProgressChanged;
                }
            }
        }

        private Progress<string> _TextProgress;
        public Progress<string> TextProgress
        {
            get { return _TextProgress; }
            set
            {
                _TextProgress = value;
                _TextProgress.ProgressChanged += _TextProgress_ProgressChanged;
            }
        }

        private CancellationTokenSource _CancellationTokenSource;
        public CancellationTokenSource CancellationTokenSource
        {
            get
            {
                return _CancellationTokenSource;
            }
            set
            {
                _CancellationTokenSource = value;
                Task.Run(WaitForCancel);
            }
        }

        private void WaitForCancel()
        {
            _CancellationTokenSource.Token.WaitHandle.WaitOne();
            Dispatcher.Invoke(Close);
        }

        private void _Progress_ProgressChanged(object sender, double progress)
        {
            Dispatcher.Invoke(() => { ProgressBar.Value = double.IsInfinity(progress) ? 0 : progress; });
        }

        private void _TextProgress_ProgressChanged(object sender, string textProgress)
        {
            Dispatcher.Invoke(() => { Text = textProgress; });
        }

        protected override void OnClosed(EventArgs e)
        {
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Value = 0;
            if (_Progress != null)
            {
                _Progress.ProgressChanged -= _Progress_ProgressChanged;
            }

            _CancellationTokenSource?.Cancel();

            base.OnClosed(e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _CancellationTokenSource?.Cancel();
        }
    }
}
